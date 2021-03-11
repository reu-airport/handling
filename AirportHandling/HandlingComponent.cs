using System;
using AirportHandling.Dtos;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace AirportHandling
{
    class HandlingComponent
    {
        IMessageQueueClient _mqClient;
        const string _airplaneRequestsQueueName = "airplane_requests";
        const string _vehicleRequestsQueueName = "vehicle_requests";

        private ConcurrentQueue<int> _freeDepartureSites = new ConcurrentQueue<int>();
        private ConcurrentQueue<int> _freeArrivalSites = new ConcurrentQueue<int>();

        public HandlingComponent(IMessageQueueClient mqClient)
        {
            _mqClient = mqClient;
            _freeDepartureSites.Enqueue(0);
            _freeDepartureSites.Enqueue(1);
            _freeArrivalSites.Enqueue(2);
            _freeArrivalSites.Enqueue(3);
        }

        private IEnumerable<VehicleType> GetVehiclesOrder(AirplaneRequest request)
        {
            if (request.RequestType == RequestType.Landing)
            {
                yield return VehicleType.FollowMeVan;
                if (request.HasVips)
                    yield return VehicleType.VipShuttle;
                if (request.RefuelNeeded)
                    yield return VehicleType.Refueler;
                yield return VehicleType.BaggageLoader;
                yield return VehicleType.BaggageVan;
                yield return VehicleType.Stairs;
                yield return VehicleType.Bus;
            }
            else
            {
                yield return VehicleType.Stairs;
                yield return VehicleType.Bus;
                yield return VehicleType.BaggageLoader;
                yield return VehicleType.BaggageVan;
                if (request.RefuelNeeded)
                    yield return VehicleType.Refueler;
                yield return VehicleType.CateringTruck;
                if (request.HasVips)
                    yield return VehicleType.VipShuttle;
                yield return VehicleType.FollowMeVan;
            }
        }

        private void OnAirplaneRequest(AirplaneRequest request)
        {
            string requestTypeStr = Enum.GetName(typeof(RequestType), request.RequestType);
            Console.WriteLine($"{DateTime.Now}: got request from airplane #{request.Id}, {requestTypeStr}");

            ConcurrentQueue<int> freeSitesQueue = request.RequestType switch
            {
                RequestType.Landing => _freeArrivalSites,
                RequestType.Takeoff => _freeDepartureSites,
            };
            freeSitesQueue.TryDequeue(out int site);
            Console.WriteLine($"{DateTime.Now}: request of airplane #{request.Id} fulfilled, site #{site}");
            
            var vehicles = GetVehiclesOrder(request);
            foreach (var vehicle in vehicles)
            {
                var vehRequest = new VehicleRequest
                {
                    VehicleType = vehicle,
                    Site = site
                };
                _mqClient.PublishToQueue(_vehicleRequestsQueueName, vehRequest);
            }
        }

        public void Run()
        { 
            _mqClient.Subscribe<AirplaneRequest>(_airplaneRequestsQueueName, OnAirplaneRequest);
        }
    }
}
