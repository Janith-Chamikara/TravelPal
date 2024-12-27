using System.Collections.Generic;

namespace TravelPal.DataStructures
{
    public class Node
    {
        public long Id { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public Dictionary<Node, double> Neighbors { get; set; }

        public Node(long id, double lat, double lon)
        {
            Id = id;
            Latitude = lat;
            Longitude = lon;
            Neighbors = new Dictionary<Node, double>();
        }
    }
}