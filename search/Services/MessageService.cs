using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LedditModels;
using Newtonsoft.Json;
using System.Text;

namespace Search.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? _connection;
        private IModel? _exchange; // _exchange
        private IServiceProvider? _serviceProvider;

        public MessageService(IServiceProvider? serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken token)
        {
            Connect();
            ListenForMessages();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            _exchange?.Close();
            _connection?.Close();

            return Task.CompletedTask;
        }

        private void Connect()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbit-service",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _exchange = _connection.CreateModel();
        }

        private void ListenForMessages()
        {
            // ========= Posts
            //channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            //channel.ExchangeDeclare("update-post", ExchangeType.Fanout);
            //channel.ExchangeDeclare("delete-post", ExchangeType.Fanout);
            // ========= Posts

            // ========= Comments
            //channel.ExchangeDeclare("add-comment", ExchangeType.Fanout);
            //channel.ExchangeDeclare("edit-comment", ExchangeType.Fanout);
            //channel.ExchangeDeclare("delete-comment", ExchangeType.Fanout);
            // ========= Comments

            // ========= Users
            //_channel.ExchangeDeclare(exchange: "register-user", type: ExchangeType.Fanout);
            //_channel.ExchangeDeclare(exchange: "update-user", type: ExchangeType.Fanout);
            //_channel.ExchangeDeclare(exchange: "delete-user", type: ExchangeType.Fanout);
            // ========= Users

            List<string> postExchangeNames = new(["add-post", "update-post", "delete-post"]);
            List<string> commentExchangeNames = new(["add-comment", "edit-comment", "delete-comment"]);
            List<string> userExchangeNames = new(["register-user", "update-user", "delete-user"]);

            _exchange.ExchangeDeclare("add-post", ExchangeType.Fanout);
            //var queue = _exchange.QueueDeclare(queue: "post", durable: true, exclusive: false, autoDelete: false);
            _exchange.QueueBind(queue:
                _exchange.QueueDeclare(queue:
                    "post",
                    durable: true,
                    exclusive: false,
                    autoDelete: false
                ),
                exchange: "add-post",
                routingKey: ""
            );




            _exchange.ExchangeDeclare("delete-post", ExchangeType.Fanout);
            var queue = _exchange.QueueDeclare("post", true, false, false);
            _exchange.QueueBind(queue, "delete-post", string.Empty);

            var consumer = new EventingBasicConsumer(_exchange);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToString();
                var json = JsonConvert.SerializeObject(body);

                try
                {
                    // Get the post object
                    var post = JsonSerializer.Deserialize<Post>(json);

                    // Create scope for CommentsService
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var commentsService = scope.ServiceProvider.GetService<SearchService>();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            _exchange.BasicConsume(queue, true, consumer);
        }

    }
}
