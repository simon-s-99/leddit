using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LedditModels;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace Search.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? _connection;
        private IModel? _channel; // _channel
        private IServiceProvider? _serviceProvider;

        public MessageService(IServiceProvider? serviceProvider)
        {
            Console.WriteLine("We are here -> ctor of msgservice");
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken token)
        {
            Console.WriteLine("We are here -> StartAsync for msgservice");
            Connect();
            ListenForMessages();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            Console.WriteLine("We are here -> stopasync of msgservice");
            _channel?.Close();
            _connection?.Close();

            return Task.CompletedTask;
        }

        private void Connect()
        {
            Console.WriteLine("We are here -> connect() in msgservice");
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbit-service",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void ListenForMessages()
        {
            Console.WriteLine("We are here -> ListenForMessages()");

            List<string> postExchanges = new(["add-post", "update-post", "delete-post"]);
            List<string> commentExchanges = new(["add-comment", "edit-comment", "delete-comment"]);
            List<string> userExchanges = new(["register-user", "update-user", "delete-user"]);

            List<string> exchanges = [.. postExchanges, .. commentExchanges, .. userExchanges];

            // declare a server-name queue
            var queue = _channel.QueueDeclare().QueueName;

            BindQueuesToExchanges(exchanges, queue);

            Console.WriteLine("We are here -> after bindQs");
            
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("We are here -> in consumer.Received");

                string body = ea.Body.ToString();
                string json = JsonConvert.SerializeObject(body);

                if (postExchanges.Contains(ea.Exchange)) // if exchange type belongs to post queue 
                {
                    Console.WriteLine("We are here -> in post if branch");

                    // Get the post object
                    Post post = System.Text.Json.JsonSerializer.Deserialize<Post>(json);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var searchService = scope.ServiceProvider.GetRequiredService<SearchService>();
                        try
                        {
                            Console.WriteLine("We are here -> trying to index post");
                            searchService.IndexDocument<Post>(post);
                            Console.WriteLine("We are here -> index should have worked?");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("We are here -> exception on post index");
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else if (commentExchanges.Contains(ea.Exchange)) // if exchange type belongs to comment queue
                {
                    // Get the comment object
                    Comment comment = System.Text.Json.JsonSerializer.Deserialize<Comment>(json);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var searchService = scope.ServiceProvider.GetRequiredService<SearchService>();
                        try
                        {
                            Console.WriteLine("We are here -> trying to index COMMENT");
                            searchService.IndexDocument<Comment>(comment);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
                else if (userExchanges.Contains(ea.Exchange)) // if exchange type belongs to user queue
                {
                    // Get the user object
                    ApplicationUser user = System.Text.Json.JsonSerializer.Deserialize<ApplicationUser>(json);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var searchService = scope.ServiceProvider.GetRequiredService<SearchService>();
                        try
                        {
                            searchService.IndexDocument<ApplicationUser>(user);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }
            };

            Console.WriteLine("We are here -> 1 line above basicconsume");
            _channel.BasicConsume(queue, true, consumer);
            Console.WriteLine("We are here -> 1 line BELOW basicconsume");
        }

        /// <summary>
        /// Binds exchangeNames to specified queue.
        /// </summary>
        /// <param name="exchangeNames">Names of declared exchanges to bind to queue.</param>
        /// <param name="queueName">Name of queue that exchanges will bind to.</param>
        private void BindQueuesToExchanges(List<string> exchangeNames, string? queueName)
        {
            Console.WriteLine("We are here -> binqstoExchanges");
            foreach (string exchangeName in exchangeNames)
            {
                Console.WriteLine("We are here ->> This msg should appear 3-9 times");
                _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                _channel.QueueBind(queue:
                    _channel.QueueDeclare(queue:
                        queueName,
                        durable: true,
                        exclusive: false,
                        autoDelete: false
                    ),
                    exchange: exchangeName,
                    routingKey: string.Empty
                );
            }
        }
    }
}
