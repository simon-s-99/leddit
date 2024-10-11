using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using LedditModels;

namespace post.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;

        public void Connect()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbit-service",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("update-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("delete-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("delete-post-comment", ExchangeType.Fanout);

        }

        public void NotifyPostChanged(string exchange, Post post)
        {
            var postJson = JsonSerializer.Serialize(post);

            // If exchange is "delete-post-comment", also publish message to Comments.Services.MessageService
            if (exchange == "delete-post-comment")
            {
                // Create queue, the same as in Comments.Services.MessageService
                var postQueue = channel.QueueDeclare("posts", true, false, false);
                var commentMessage = Encoding.UTF8.GetBytes(postJson);

                // Publish message to queue
                channel.BasicPublish(exchange, "posts", null, commentMessage);
                return;
            }

            // Create queue, the same as in Logs.Services.MessageService
            var queue = channel.QueueDeclare("events", true, false, false);

            var message = Encoding.UTF8.GetBytes($"{exchange}: {postJson}");

            // Publish message to queue
            channel.BasicPublish(exchange, "events", null, message);
        }

        public Task StartAsync(CancellationToken token)
        {
            Connect();
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
