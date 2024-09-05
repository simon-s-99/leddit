using Comments.Models;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Metadata;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

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

        public void NotifyNewCommentAdded(Comment comment)
        {
            var commentJson = JsonSerializer.Serialize(comment);
            var message = Encoding.UTF8.GetBytes(commentJson);

            channel.BasicPublish("add-comment", string.Empty, null, message);
        }

        public void Connect()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost", Port = 5199};
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("add-movie", ExchangeType.Fanout);
        }

        public async Task StartAsync(CancellationToken token)
        {

        }

		public async Task StopAsync(CancellationToken token)
		{

		}
	}
}
