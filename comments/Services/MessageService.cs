using LedditModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Comments.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace Comments.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;
        private IServiceProvider? provider;
        private HttpClient? httpClient;

        public MessageService(IServiceProvider? provider)
        {
            this.provider = provider;
            this.httpClient = null;
        }

        // Notifies about certain events regarding comments, exchanges are passed to the method
        public void NotifyCommentChanged(string exchange, Comment comment)
        {
            var queue = channel.QueueDeclare("events", true, false, false);
            var commentJson = JsonSerializer.Serialize(comment);
            var message = Encoding.UTF8.GetBytes($"{exchange}: {commentJson}");

            channel.BasicPublish(string.Empty, "events", null, message);
        }

        public void Connect()
        {
            // Use default username and password to connect
            var connectionFactory = new ConnectionFactory { HostName = "rabbit-service", Port = 5672, UserName = "guest", Password = "guest" };
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            // Register three different exchanges
            channel.ExchangeDeclare("add-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("delete-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("edit-comment", ExchangeType.Fanout);
        }
        public Task StartAsync(CancellationToken token)
        {
            Connect();
            ListenForMessages();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            channel?.Close();
            connection?.Close();

            return Task.CompletedTask;
        }

        private void ListenForMessages()
        {
            channel.ExchangeDeclare("delete-post", ExchangeType.Fanout);
            var queue = channel.QueueDeclare("post", true, false, false);
            channel.QueueBind(queue, "delete-post", string.Empty);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("In CM");
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine("json:" + json);
                Console.WriteLine("After json");

                try
                {
                    // Get the post object
                    var post = JsonSerializer.Deserialize<Post>(json);
                    Console.WriteLine("post:" + post);

                    // Create scope for CommentsService
                    using (var scope = provider.CreateScope())
                    {
                        var commentsService = scope.ServiceProvider.GetService<CommentsService>();

                        // Get all comments from the now deleted post
                        List<Comment> commentsToDelete = commentsService.GetCommentsFromPostId(post.Id);
                        Console.WriteLine("comments:" + commentsToDelete, "postID:" + post.Id);

                        // If post had no comments, return
                        if (commentsToDelete.Count == 0)
                        {
                            return;
                        }

                        // Otherwise delete them all
                        foreach (Comment comment in commentsToDelete)
                        {
                            commentsService.DeleteComment(comment.Id);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            channel.BasicConsume(queue, true, consumer);
        }

        public Post? GetPost(Guid id)
        {
            // Connect to service
            httpClient = new HttpClient { BaseAddress = new Uri("http://post-service:8080") };

            // Send request to post-service
            var request = new HttpRequestMessage(HttpMethod.Get, "api/posts?id=" + id);
            var response = httpClient.Send(request);

            // If response is 404, return null
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            // Otherwise read the response
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var json = reader.ReadToEnd();

            try
            {
                // Read the json and return the post object
                var post = JsonSerializer.Deserialize<Post>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return post;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            httpClient.Dispose();

            return null;
        }

        public ApplicationUser? GetUser(Guid id)
        {
            // Connect to service
            httpClient = new HttpClient { BaseAddress = new Uri("http://useridentityapi-service:8080") };

            // Send request to user-service
            var request = new HttpRequestMessage(HttpMethod.Get, "api/user/userid/" + id);
            var response = httpClient.Send(request);

            // If response is 404, return null
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            // Otherwise read the response
            using var reader = new StreamReader(response.Content.ReadAsStream());
            var json = reader.ReadToEnd();

            try
            {
                // Read the json and return the post object
                var user = JsonSerializer.Deserialize<ApplicationUser>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            httpClient.Dispose();

            return null;
        }
    }
}
