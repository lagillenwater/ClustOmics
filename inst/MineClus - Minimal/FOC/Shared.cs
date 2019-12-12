using System.Collections.Generic;
using System.Linq;

namespace FOC
{
    internal class Shared
    {
        // subject ID => header name => value | 12345 => gender => male
        public static Dictionary<string, Dictionary<string, double>> Data;

        // returns the list of attributes in the dataset, sorted alphabetically
        public static List<string> Headers =>
            Data.First().Value
                .Select(x => x.Key)
                .OrderBy(x => x)
                .ToList();

        // to measure the run time of the algorithm
        public static double Runtime { get; set; }
    }
}