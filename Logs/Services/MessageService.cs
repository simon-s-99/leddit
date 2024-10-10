using LedditModels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Logs.DTOs;

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
            channel.ExchangeDeclare("delete-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("update-post", ExchangeType.Fanout);

            channel.ExchangeDeclare("delete-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("add-comment", ExchangeType.Fanout);
            channel.ExchangeDeclare("edit-comment", ExchangeType.Fanout);

            channel.ExchangeDeclare("delete-user", ExchangeType.Fanout);
            channel.ExchangeDeclare("register-user", ExchangeType.Fanout);
            channel.ExchangeDeclare("update-user", ExchangeType.Fanout);

            // Declare new queue, which will handle all events
            var queue = channel.QueueDeclare("events", true, false, false);

            channel.QueueBind(queue.QueueName, "delete-post", string.Empty);
            channel.QueueBind(queue.QueueName, "add-post", string.Empty);
            channel.QueueBind(queue.QueueName, "update-post", string.Empty);
            
            channel.QueueBind(queue.QueueName, "delete-comment", string.Empty);
            channel.QueueBind(queue.QueueName, "add-comment", string.Empty);
            channel.QueueBind(queue.QueueName, "edit-comment", string.Empty);

            channel.QueueBind(queue.QueueName, "delete-user", string.Empty);
            channel.QueueBind(queue.QueueName, "register-user", string.Empty);
            channel.QueueBind(queue.QueueName, "update-user", string.Empty);

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

                        AddLogDTO newLog = new AddLogDTO
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