using AirportHandling.Dtos;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Moq;

namespace AirportHandling
{
    public abstract class TestMqClient : IMessageQueueClient
    {
        public Action<AirplaneRequest> airplaneRequestHandler;

        public abstract void Dispose();
        public abstract void PublishToQueue<TDto>(string queue, TDto dto);
        public abstract void Subscribe<TDto>(string queue, Action<TDto> onMessageCallback);
    }

    class Program
    {
        static void Main(string[] args)
        {
            const string airplaneRequestsQueueName = "airplane_requests";
            const string vehicleRequestsQueueName = "vehicle_requests";

            var mqClientMock = new Mock<TestMqClient>();
            mqClientMock
                .Setup(mqClient =>
                    mqClient.PublishToQueue(airplaneRequestsQueueName, It.IsAny<AirplaneRequest>())
                )
                .Callback<string, AirplaneRequest>((queue, req) =>
                {
                    Console.WriteLine(
                        $"AirplaneRequest published to {queue}: " +
                        $"Id={req.Id}, RequestType={req.RequestType}, RefuelNeeded={req.RefuelNeeded}, HasVips={req.HasVips}");
                    mqClientMock.Object.airplaneRequestHandler?.Invoke(req);
                });

            mqClientMock
                .Setup(mqClient =>
                   mqClient.Subscribe(airplaneRequestsQueueName, It.IsAny<Action<AirplaneRequest>>())
                )
                .Callback<string, Action<AirplaneRequest>>((queue, action) =>
                {
                    Console.WriteLine($"{DateTime.Now} Subscribed to {queue}");
                    mqClientMock.Object.airplaneRequestHandler = action;
                });
            mqClientMock
                .Setup(mqClient =>
                   mqClient.PublishToQueue(vehicleRequestsQueueName, It.IsAny<VehicleRequest>())
                )
                .Callback<string, VehicleRequest>((_, vehRequest) =>
                    Console.WriteLine(
                        $"{DateTime.Now} Vehicle request sent: VehicleType={vehRequest.VehicleType}, site={vehRequest.Site}"
                    )
                );

            var mq = mqClientMock.Object;
            new HandlingComponent(mq).Run();

            mq.PublishToQueue(
                airplaneRequestsQueueName,
                new AirplaneRequest(Guid.NewGuid(), RequestType.Landing, true, true)
                );
            mq.PublishToQueue(
                airplaneRequestsQueueName,
                new AirplaneRequest(Guid.NewGuid(), RequestType.Takeoff, true, false)
                );

            Console.ReadLine();
        }
    }
}
