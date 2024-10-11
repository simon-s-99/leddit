using LedditModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Logs.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;
        private IServiceProvider? provider;

        public MessageService(IServiceProvider? provider)
        {
            this.provider = provider;
        }

        public void Connect()
        {
            // Use default username and password to connect
            var connectionFactory = new ConnectionFactory { HostName = "rabbit-service", Port = 5672, UserName = "guest", Password = "guest" };
            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();
        }
        public Task StartAsync(CancellationToken token)
        {
            Connect();
            ListenForMessages();
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
            string[] exchanges = [
                "delete-post",
                "add-post",
                "update-post",
                "delete-comment",
                "add-comment",
                "edit-comment",
                "delete-user",
                "register-user",
                "update-user",
            ];

            // Declare a new queue, which will handle all events relating to the exchanges
            var queue = channel.QueueDeclare("events", true, false, false);

            // Declare nine different exchanges, and bind each of them to the queue
            foreach (string exchange in exchanges)
            {
                channel.ExchangeDeclare(exchange, ExchangeType.Fanout);
                channel.QueueBind(queue.QueueName, exchange, string.Empty);
            }

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received: " + json);

                try
                {
                    using (var scope = provider.CreateScope())
                    {
                        // Create LogsService scope
                        var logsService = scope.ServiceProvider.GetService<LogsService>();

                        Log newLog = new Log
                        {
                            Body = json,
                            CreatedDate = DateTime.UtcNow
                        };

                        logsService.AddLog(newLog);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            channel.BasicConsume(queue, true, consumer);
        }
    }
}