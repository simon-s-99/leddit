﻿using System.Text;
using RabbitMQ.Client;
using System.Text.Json;
using post.Models;

namespace post.Services
{
    public class MessageService : IHostedService
    {
        private IConnection? connection;
        private IModel? channel;

        public void Connect()
        {
            var isRunningInDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            var connectionFactory = new ConnectionFactory
            {
                HostName = isRunningInDocker ? "host.docker.internal" : "localhost",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            connection = connectionFactory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare("add-post", ExchangeType.Fanout);
            channel.ExchangeDeclare("update-post", ExchangeType.Fanout);
        }

        public void NotifyPostChanged(string exchange, Post post)
        {
            var postJson = JsonSerializer.Serialize(post);
            var message = Encoding.UTF8.GetBytes(postJson);

            channel.BasicPublish(exchange, string.Empty, null, message);
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
