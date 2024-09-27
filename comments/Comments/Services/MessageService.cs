using Comments.Models;
using Comments.Models.DTOs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Comments.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;
        private IServiceProvider? provider;
        private HttpClient httpClient;

        public MessageService(IServiceProvider? provider)
        {
            this.provider = provider;
            this.httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5199")
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
            Console.WriteLine("TEST!!");
            // Use default username and password to connect
            var connectionFactory = new ConnectionFactory { HostName = "rabbit-service", Port = 5672, UserName = "guest", Password = "guest" };
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            // Register three different exchanges
            channel.ExchangeDeclare("add-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("delete-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("edit-comment", ExchangeType.Fanout);
        }

        public void ListenForMessages()
        {
            channel.ExchangeDeclare("add-comment", ExchangeType.Fanout);
            var queue = channel.QueueDeclare("comment", true, false, false);
            channel.QueueBind(queue, "add-comment", string.Empty);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                try
                {
                    var comment = JsonSerializer.Deserialize<AddCommentDTO>(json);
                    Console.WriteLine("Created comment " + comment.Body);

                    using (var scope = provider.CreateScope())
                    {
                        var commentsService = scope.ServiceProvider.GetRequiredService<CommentsService>();
                        commentsService.AddComment(comment);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            };

            channel.BasicConsume(queue, true, consumer);
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
    }
}
