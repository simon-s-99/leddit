using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;

namespace Comments.Services
{
    public class MessageService : IHostedService
    {
        private IConnection connection;
        private RabbitMQ.Client.IModel channel;
        private readonly IServiceProvider _serviceProvider;
        private readonly HttpClient _httpClient;

        public MessageService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:5199")
            };
        }

        public void ListenForMessages()
        {
            channel.ExchangeDeclare(exchange: "add-comment", type: ExchangeType.Fanout);

            var queue = channel.QueueDeclare("comments", true, false, false);
            channel.QueueBind(queue: queue, exchange: "add-comment", routingKey: String.Empty);


        }

        public void Connect()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost", Port = 5199};
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();
        }
    }
}
