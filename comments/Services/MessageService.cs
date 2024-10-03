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
        private HttpClient httpClient; // Not finished

        public MessageService(IServiceProvider? provider)
        {
            this.provider = provider;
            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://post-service:8080")
            };
        }

        // Notifies about certain events regarding comments, exchanges are passed to the method
        public void NotifyCommentChanged(string exchange, Comment comment)
        {
            var commentJson = JsonSerializer.Serialize(comment);
            var message = Encoding.UTF8.GetBytes(commentJson);

            channel.BasicPublish(exchange, string.Empty, null, message);
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
            // ListenForMessages();
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
            channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            var queue = channel.QueueDeclare("post", true, false, false);
            channel.QueueBind(queue, "add-post", string.Empty);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var post = JsonSerializer.Deserialize<Post>(json);
                    Console.WriteLine("Post created " + post.Content);

                    // using (var scope = provider.CreateScope())
                    // {
                    //     var commentsService = scope.ServiceProvider.GetRequiredService<CommentsService>();
                    //     commentsService.AddComment(comment);
                    // }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            };

            channel.BasicConsume(queue, true, consumer);
        }

        public Post? GetPost(Guid id)
        {
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

            return null;
        }
    }
}
