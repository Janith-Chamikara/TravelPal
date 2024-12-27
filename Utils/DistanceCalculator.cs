using System;
using TravelPal.DataStructures;

namespace TravelPal.Utils
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371;

        public static double CalculateDistance(Node node1, Node node2)
        {
            double lat1 = ToRadians(node1.Latitude);
            double lon1 = ToRadians(node1.Longitude);
            double lat2 = ToRadians(node2.Latitude);
            double lon2 = ToRadians(node2.Longitude);

            double dLat = lat2 - lat1;
            double dLon = lon2 - lon1;

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                      Math.Cos(lat1) * Math.Cos(lat2) *
                      Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}