using Comments.Models;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace Comments.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;

        // Notifies about certain events regarding comments, exhanges are passed to the method
        public void NotifyCommentChanged(string exchange, Comment comment)
        {
			var commentJson = JsonSerializer.Serialize(comment);
			var message = Encoding.UTF8.GetBytes(commentJson);

			channel.BasicPublish(exchange, string.Empty, null, message);
        }

		public void Connect()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest"};
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            Console.WriteLine("Connected");
            // Register three different exchanges
            channel.ExchangeDeclare("add-movie", ExchangeType.Fanout);
			channel.ExchangeDeclare("delete-movie", ExchangeType.Fanout);
            channel.ExchangeDeclare("edit-movie", ExchangeType.Fanout);
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
