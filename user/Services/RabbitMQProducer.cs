using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace userIdentityAPI.Services
{
    public class RabbitMQProducer : IHostedService
    {
        private IConnection? _connection;
        private IModel? _channel;

        public void Connect()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbit-svc",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare necessary exchanges
            _channel.ExchangeDeclare(exchange: "register-user", type: ExchangeType.Fanout);
            _channel.ExchangeDeclare(exchange: "update-user", type: ExchangeType.Fanout);
            _channel.ExchangeDeclare(exchange: "delete-user", type: ExchangeType.Fanout);
        }


        // A flexible method that can handle different DTOs based on the action
        public void NotifyUserEvent(string exchange, object dto)
        {
            // Create queue, the same as in Logs.Services.MessageService
            var queue = _channel.QueueDeclare("events", true, false, false);

            var json = JsonSerializer.Serialize(dto);
            var message = Encoding.UTF8.GetBytes(json);

            // Publish message to queue
            _channel.BasicPublish(exchange, "events", null, message);
            Console.WriteLine($"Message sent to exchange '{exchange}': {json}");
        }

        public Task StartAsync(CancellationToken token)
        {
            Connect();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            _channel?.Close();
            _connection?.Close();
            return Task.CompletedTask;
        }
    }
}
