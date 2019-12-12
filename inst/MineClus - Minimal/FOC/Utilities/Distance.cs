using System;
using System.Collections.Generic;

namespace FOC.Utilities
{
    internal class Distance
    {
        public static double GetDistance(string point1Key, string point2Key)
        {
            return GetDistance(Shared.Data[point1Key], Shared.Data[point2Key]);
        }

        public static double GetDistance(string point1Key, Dictionary<string, double> point2)
        {
            return GetDistance(Shared.Data[point1Key], point2);
        }

        public static double GetDistance(Dictionary<string, double> point1, Dictionary<string, double> point2)
        {
            var distance = 0.0;

            foreach (var dimension in Shared.Headers)
                distance += Math.Pow(point1[dimension] - point2[dimension], 2);

            return Math.Sqrt(distance);
        }
    }
}