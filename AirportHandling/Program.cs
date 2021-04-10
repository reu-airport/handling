using AirportHandling.Dtos;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq;
using RabbitMQ.Client;

namespace AirportHandling
{
    class Program
    {
        static void Main(string[] args)
        {
            var mq = new RabbitMqWrapper("206.189.60.128", "guest", "guest", "/");
            new HandlingComponent(mq).Run();
            Console.ReadLine();
        }
    }
}
