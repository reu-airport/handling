using System;
using System.Collections.Generic;
using System.Text;

namespace AirportHandling.Dtos
{
    public enum RequestType
    {
        Landing, 
        Takeoff
    }

    public class AirplaneRequest
    {
        public Guid Id { get; set; }

        public RequestType RequestType { get; set; }

        public bool RefuelNeeded { get; set; }

        public bool HasVips { get; set; }

        public AirplaneRequest()
        {
        }

        public AirplaneRequest(Guid id, RequestType requestType, bool refuelNeeded, bool hasVips)
        {
            Id = id;
            RequestType = requestType;
            RefuelNeeded = refuelNeeded;
            HasVips = hasVips;
        }
    }
}
