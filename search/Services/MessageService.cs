using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LedditModels;
using System.Text;
using Elastic.Clients.Elasticsearch;

namespace Search.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? _connection;
        private IModel? _channel; // channel / exchange
        private IElasticsearchClientSettings _elasticsearchClientSettings;

        public MessageService(IElasticsearchClientSettings elasticsearchClientSettings)
        {
            _elasticsearchClientSettings = elasticsearchClientSettings;
        }

        public Task StartAsync(CancellationToken token)
        {
            Connect();
            ListenForMessages();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken token)
        {
            _channel?.Close();
            _connection?.Close();

            return Task.CompletedTask;
        }

        private void Connect()
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = "rabbit-svc",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        private void ListenForMessages()
        {
            List<string> postExchanges = new(["add-post", "update-post", "delete-post"]);
            List<string> commentExchanges = new(["add-comment", "edit-comment", "delete-comment"]);
            List<string> userExchanges = new(["register-user", "update-user", "delete-user"]);

            List<string> exchanges = [.. postExchanges, .. commentExchanges, .. userExchanges];

            // declare a server-name queue and get it's name as a variable
            var queue = _channel.QueueDeclare(
                        durable: true,
                        exclusive: false,
                        autoDelete: false).QueueName;

            BindQueuesToExchanges(exchanges, queue);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);

                var searchService = new SearchService(_elasticsearchClientSettings);

                if (postExchanges.Contains(ea.Exchange)) // if exchange type belongs to post queue 
                {
                    // Get the post object
                    Post post = System.Text.Json.JsonSerializer.Deserialize<Post>(json);

                    try
                    {
                        // 1 = add | 2 = update | 3 = delete
                        if (postExchanges[1] == ea.Exchange) { searchService.IndexDocument<Post>(post); }
                        else if (postExchanges[2] == ea.Exchange) { searchService.UpdateDocument<Post>(post); }
                        else if (postExchanges[3] == ea.Exchange) { searchService.DeleteDocument<Post>(post); }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (commentExchanges.Contains(ea.Exchange)) // if exchange type belongs to comment queue
                {
                    // Get the comment object
                    Comment comment = System.Text.Json.JsonSerializer.Deserialize<Comment>(json);

                    try
                    {
                        // 1 = add | 2 = update | 3 = delete
                        if (commentExchanges[1] == ea.Exchange) { searchService.IndexDocument<Comment>(comment); }
                        else if (commentExchanges[2] == ea.Exchange) { searchService.UpdateDocument<Comment>(comment); }
                        else if (commentExchanges[3] == ea.Exchange) { searchService.DeleteDocument<Comment>(comment); }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                else if (userExchanges.Contains(ea.Exchange)) // if exchange type belongs to user queue
                {
                    // Get the user object
                    ApplicationUser user = System.Text.Json.JsonSerializer.Deserialize<ApplicationUser>(json);

                    try
                    {
                        // 1 = add | 2 = update | 3 = delete
                        if (userExchanges[1] == ea.Exchange) { searchService.IndexDocument<ApplicationUser>(user); }
                        else if (userExchanges[2] == ea.Exchange) { searchService.UpdateDocument<ApplicationUser>(user); }
                        else if (userExchanges[3] == ea.Exchange) { searchService.DeleteDocument<ApplicationUser>(user); }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            };

            _channel.BasicConsume(queue, true, consumer);
        }

        /// <summary>
        /// Binds exchangeNames to specified queue.
        /// </summary>
        /// <param name="exchangeNames">Names of declared exchanges to bind to queue.</param>
        /// <param name="queueName">Name of queue that exchanges will bind to.</param>
        private void BindQueuesToExchanges(List<string> exchangeNames, string? queueName)
        {
            foreach (string exchangeName in exchangeNames)
            {
                _channel.ExchangeDeclare(exchangeName, ExchangeType.Fanout);
                _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: string.Empty);
            }
        }
    }
}
