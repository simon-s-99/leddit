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
                HostName = "rabbit-svc",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("update-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("delete-post", ExchangeType.Fanout);
        }

        public void NotifyPostChanged(string exchange, Post post)
        {
            // Create queue, the same as in Logs.Services.MessageService
            var queue = channel.QueueDeclare("events", true, false, false);

            var postJson = JsonSerializer.Serialize(post);
            var message = Encoding.UTF8.GetBytes(postJson);

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
