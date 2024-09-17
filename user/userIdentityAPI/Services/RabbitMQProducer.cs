using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace userIdentityAPI.Services
{
    public class RabbitMQProducer : IHostedService
    {
        private IConnection? _connection;
        private IModel? _channel;
        private readonly ConnectionFactory _connectionFactory;

        public RabbitMQProducer()
        {
            // Configure the RabbitMQ connection factory
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };
        }

        public void SendMessage(string exchange, object data)
        {
            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange, routingKey: "", basicProperties: null, body: body);
        }

        public void Connect()
        {
            // Create connection and channel
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("user-registered", ExchangeType.Fanout);
            _channel.ExchangeDeclare("user-updated", ExchangeType.Fanout);
            _channel.ExchangeDeclare("user-deleted", ExchangeType.Fanout);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Connect();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancelToken)
        {
            _channel?.Close();
            _connection.Close();
            return Task.CompletedTask;
        }
    }
}