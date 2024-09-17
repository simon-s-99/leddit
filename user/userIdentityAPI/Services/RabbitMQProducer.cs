using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace userIdentityAPI.Services
{
    public class RabbitMQProducer
    {
        private readonly ConnectionFactory _factory;

        public RabbitMQProducer()
        {
            _factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public void SendMessage(string eventType, object data)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "UserExchange", type: ExchangeType.Fanout);

            var message = JsonSerializer.Serialize(data);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "UserExchange", routingKey: "", basicProperties: null, body: body);
        }
    }
}
