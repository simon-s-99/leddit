using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LedditModels;
using Newtonsoft.Json;
using System.Text;
using Newtonsoft.Json.Bson;

namespace Search.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? _connection;
        private IModel? _exchange; // _exchange
        //private IServiceProvider? _serviceProvider;

        //public MessageService(IServiceProvider? serviceProvider)
        //{
        //    _serviceProvider = serviceProvider;
        //}

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
            BindQueuesToExchanges(new(["add-post", "update-post", "delete-post"]), "post");
            BindQueuesToExchanges(new(["add-comment", "edit-comment", "delete-comment"]), "comment");
            BindQueuesToExchanges(new(["register-user", "update-user", "delete-user"]), "user");

            /*
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
                    //var post = JsonSerializer.Deserialize<Post>(json);

                    // Create scope for CommentsService
                    //using (var scope = _serviceProvider.CreateScope())
                    //{
                    //    var commentsService = scope.ServiceProvider.GetService<SearchService>();
                    //}
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            };

            _exchange.BasicConsume(queue, true, consumer);
            */
        }

        /// <summary>
        /// Binds exchangeNames to specified queue.
        /// </summary>
        /// <param name="exchangeNames">Names of declared exchanges to bind to queue.</param>
        /// <param name="queueName">Name of queue that exchanges will bind to.</param>
        private void BindQueuesToExchanges(List<string> exchangeNames, string queueName)
        {
            foreach (string exchangeName in exchangeNames)
            {
                _exchange.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                //var queue = _exchange.QueueDeclare(queue: "post", durable: true, exclusive: false, autoDelete: false);
                _exchange.QueueBind(queue:
                    _exchange.QueueDeclare(queue:
                        queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false
                    ),
                    exchange: exchangeName,
                    routingKey: ""
                );
            }
        }
    }
}
