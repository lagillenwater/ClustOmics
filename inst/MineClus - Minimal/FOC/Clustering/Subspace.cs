using System.Collections.Generic;
using System.Linq;

namespace FOC.Clustering
{
    internal class Subspace
    {
        public Subspace()
        {
            Dimensions = new HashSet<string>();
            Centroid = new KeyValuePair<string, Dictionary<string, double>>();
        }

        public KeyValuePair<string, Dictionary<string, double>> Centroid { get; set; }
        public HashSet<string> Dimensions { get; set; }
        public List<string> OrderedDimensions => Dimensions.OrderBy(x => x).ToList();
        public int Dimensionality => Dimensions.Count;
        public int SubjectCount { get; set; }
        public Score Score => new Score {Dimensions = Dimensionality, Points = SubjectCount};
    }
}