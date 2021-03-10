using System;
using System.Collections.Generic;
using System.Text;

namespace AirportHandling.Dtos
{
    public enum VehicleType
    { 
        FollowMeVan,
        Stairs,
        Bus,
        BaggageLoader,
        BaggageVan,
        Refueler,
        CateringTruck,
        VipShuttle,
    }

    public class VehicleRequest
    {
        public VehicleType VehicleType { get; set; }

        public int Site { get; set; }

        public VehicleRequest()
        {
        }

        public VehicleRequest(VehicleType vehicleType, int site)
        {
            VehicleType = vehicleType;
            Site = site;
        }
    }
}
