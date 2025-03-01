using System;
using TravelPal.DataStructures;

namespace TravelPal.Utils
{
    public static class DistanceCalculator
    {
        private const double EarthRadiusKm = 6371;

        public static double CalculateDistance(double lati1, double longi1, double lati2, double longi2)
        {
            double lat1 = ToRadians(lati1);
            double lon1 = ToRadians(longi1);
            double lat2 = ToRadians(lati2);
            double lon2 = ToRadians(longi2);

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