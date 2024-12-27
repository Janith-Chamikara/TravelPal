using System.Xml.Linq;
using System.Collections.Generic;
using TravelPal.DataStructures;

namespace TravelPal.Services
{
    public class OSMParser
    {
        public Dictionary<string, (double Lat, double Lon)> Nodes { get; set; } = new();
        public List<(string From, string To, double Distance)> Edges { get; set; } = new();

        public void Parse(string filePath)
        {
            var xdoc = XDocument.Load(filePath);

            // Parse nodes
            foreach (var node in xdoc.Descendants("node"))
            {
                var id = node.Attribute("id")?.Value;
                var lat = double.Parse(node.Attribute("lat")?.Value ?? "0");
                var lon = double.Parse(node.Attribute("lon")?.Value ?? "0");

                if (id != null) Nodes[id] = (lat, lon);
            }

            // Parse ways
            foreach (var way in xdoc.Descendants("way"))
            {
                var nodeRefs = new List<string>();
                foreach (var nd in way.Descendants("nd"))
                {
                    var refId = nd.Attribute("ref")?.Value;
                    if (refId != null) nodeRefs.Add(refId);
                }

                for (int i = 0; i < nodeRefs.Count - 1; i++)
                {
                    if (Nodes.ContainsKey(nodeRefs[i]) && Nodes.ContainsKey(nodeRefs[i + 1]))
                    {
                        var from = nodeRefs[i];
                        var to = nodeRefs[i + 1];
                        var distance = Haversine(Nodes[from], Nodes[to]);
                        Edges.Add((from, to, distance));
                    }
                }
            }
        }

        private double Haversine((double Lat, double Lon) point1, (double Lat, double Lon) point2)
        {
            const double R = 6371; // Earth radius in km
            var latDiff = ToRadians(point2.Lat - point1.Lat);
            var lonDiff = ToRadians(point2.Lon - point1.Lon);

            var a = Math.Sin(latDiff / 2) * Math.Sin(latDiff / 2) +
                    Math.Cos(ToRadians(point1.Lat)) * Math.Cos(ToRadians(point2.Lat)) *
                    Math.Sin(lonDiff / 2) * Math.Sin(lonDiff / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private double ToRadians(double angle) => angle * (Math.PI / 180);
    }
}
