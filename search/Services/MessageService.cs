using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LedditModels;
using Newtonsoft.Json;

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
            string postQueue = "post";
            string commentQueue = "comment";
            string userQueue = "user";

            List<string> postExchanges = new(["add-post", "update-post", "delete-post"]);
            List<string> commentExchanges = new(["add-comment", "edit-comment", "delete-comment"]);
            List<string> userExchanges = new(["register-user", "update-user", "delete-user"]);

            BindQueuesToExchanges(postExchanges, postQueue);
            BindQueuesToExchanges(commentExchanges, commentQueue);
            BindQueuesToExchanges(userExchanges, userQueue);

            var consumer = new EventingBasicConsumer(_exchange);

            consumer.Received += async (model, ea) =>
            {
                string body = ea.Body.ToString();
                string json = JsonConvert.SerializeObject(body);

                if (postExchanges.Contains(ea.Exchange)) // if exchange type belongs to post queue 
                {
                    // Get the post object
                    Post post = System.Text.Json.JsonSerializer.Deserialize<Post>(json);
                    
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        //var searchService = scope.ServiceProvider.GetService<SearchService>();
                    }
                }
                else if (commentExchanges.Contains(ea.Exchange)) // if exchange type belongs to comment queue
                {

                }
                else if (userExchanges.Contains(ea.Exchange)) // if exchange type belongs to user queue
                {

                }

                try
                {

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

            _exchange.BasicConsume(postQueue, true, consumer);
            _exchange.BasicConsume(commentQueue, true, consumer);
            _exchange.BasicConsume(userQueue, true, consumer);
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
