using System;
using AirportHandling.Dtos;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;

namespace AirportHandling
{
    class HandlingComponent
    {
        IMessageQueueClient _mqClient;
        const string _airplaneRequestsQueueName = "airplaneRequest";
        const string _vehicleRequestsQueueName = "vehicleRequest";
        const string _handlindEndsQueueName = "handlingEnd";

        private ConcurrentQueue<AirplaneRequest> requestsQueue = new ConcurrentQueue<AirplaneRequest>();
        private AutoResetEvent oneSiteFreeHandle = new AutoResetEvent(true);
        private bool site1Empty = true;
        private bool site2Empty = true;

        public HandlingComponent(IMessageQueueClient mqClient)
        {
            _mqClient = mqClient;
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

        private void OnHandlingEndRequest(HandlingEnd dto)
        {
            if (dto.Site == 1)
                site1Empty = true;
            else if (dto.Site == 2)
                site2Empty = true;
            oneSiteFreeHandle.Set();
        }

        private void OnAirplaneRequest(AirplaneRequest request)
        {
            string requestTypeStr = Enum.GetName(typeof(RequestType), request.RequestType);
            Console.WriteLine($"{DateTime.Now}: got request from airplane #{request.Id}, {requestTypeStr}");

            int site = 0;
            if (!site1Empty && !site2Empty)
            {
                oneSiteFreeHandle.Reset();
                requestsQueue.Enqueue(request);
                oneSiteFreeHandle.WaitOne();
            }
            if (site1Empty)
            {
                site = 1;
                site1Empty = false;
            } 
            else if (site2Empty)
            {
                site = 2;
                site2Empty = false;
            }
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
            _mqClient.Subscribe<HandlingEnd>(_handlindEndsQueueName, OnHandlingEndRequest);
        }
    }
}
