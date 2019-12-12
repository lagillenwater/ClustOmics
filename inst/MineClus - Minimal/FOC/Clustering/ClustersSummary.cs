using System;
using System.Collections.Generic;
using System.Linq;
using FOC.Settings;
using FOC.Utilities;

namespace FOC.Clustering
{
    internal class ClustersSummary
    {
        public ClustersSummary(List<Cluster> clusters)
        {
            Clusters = new List<Cluster>(clusters);
        }

        public List<Cluster> Clusters { get; set; }
        public double AvgPoints => Clusters.Count == 0 ? 0 : Math.Round(Clusters.Average(x => x.Subjects.Count), 2);

        public string AllPoints => Clusters.Count == 0
            ? ""
            : Converters.ListToString(Clusters.Select(x => x.Subjects.Count).ToList(), ";", true);

        public double AvgDimensions =>
            Clusters.Count == 0 ? 0 : Math.Round(Clusters.Average(x => x.SubSpace.Dimensionality), 2);

        public string AllDimensions => Clusters.Count == 0
            ? ""
            : Converters.ListToString(Clusters.Select(x => x.SubSpace.Dimensionality).ToList(), "; ", true);

        public string SubspaceFeatures =>
            Clusters.Count == 0 ? "" : string.Join(";", Clusters.Select(x => x.DimensionNames));

        public int Outliers => SettingsDataset.SampleSize - Clusters.Sum(x => x.SubjectCount);
    }
}