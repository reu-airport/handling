using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Linq;
using System.Text.Json.Serialization;

namespace AirportHandling
{
    class RabbitMqWrapper : IMessageQueueClient
    {
        IConnection _connection;
        IModel _channel;
        EventingBasicConsumer _consumer;
        JsonSerializerOptions _serOptions = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };

        public RabbitMqWrapper(string hostname, string username, string password, string vhost)
        {
            var connFactory = new ConnectionFactory()
            {
                HostName = hostname,
                UserName = username,
                Password = password,
                VirtualHost = vhost
            };
            _connection = connFactory.CreateConnection();
            _channel = _connection.CreateModel();
            _consumer = new EventingBasicConsumer(_channel);
        }

        public RabbitMqWrapper() : this("localhost", "guest", "guest", "/")
        {

        }

        public void Subscribe<TDto>(string queue, Action<TDto> onMessageCallback)
        {
            _channel.QueueDeclare(queue, exclusive: false);
            _consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine($"{DateTime.Now} INFO: got message in queue '{queue}'");
                Console.WriteLine(message);

                
                var dto = JsonSerializer.Deserialize<TDto>(message, _serOptions);
                onMessageCallback(dto);
            };
            _channel.BasicConsume(queue, true, _consumer);
        }

        public void PublishToQueue<TDto>(string queue, TDto dto)
        {  
            _channel.QueueDeclare(queue, exclusive: false);
            var message = JsonSerializer.Serialize(dto, _serOptions);
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(string.Empty, queue, body: body);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}
