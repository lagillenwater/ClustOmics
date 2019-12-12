using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FOC.Settings;
using FOC.Utilities;

namespace FOC.Clustering
{
    internal class Cluster
    {
        public static int testCount = 1; // keeps track of generated clusters for a clustering

        public Cluster()
        {
            Subjects = new Dictionary<string, Dictionary<string, double>>();
            SubSpace = new Subspace();
            Score = new Score();
        }

        public string Id { get; set; } // cluster label
        public Subspace SubSpace { get; set; } // dimensions (features) for this cluster
        public Dictionary<string, Dictionary<string, double>> Subjects { get; set; } // samples

        public List<string> SubjectSids =>
            Subjects.Select(x => x.Key).OrderBy(x => x).ToList(); // returns the sids in this cluster

        public int SubjectCount => Subjects.Count; // returns the number of samples in this cluster
        public Score Score { get; set; } // returns the score of the cluster (mu function)

        public string DimensionNames => string.Join(" | ", SubSpace.Dimensions.OrderBy(x => x));

        public Dictionary<string, double> GetCentroidSum()
        {
            var centroid = new Dictionary<string, double>();

            foreach (var dimension in Shared.Headers)
            {
                var sum = 0.0;
                foreach (var subjectId in SubjectSids)
                    sum += Shared.Data[subjectId][dimension];

                centroid.Add(dimension, sum);
            }

            return centroid;
        }

        public Dictionary<string, double> GetCentroid(Dictionary<string, double> centroidSum)
        {
            var centroid = new Dictionary<string, double>();

            foreach (var dimension in Shared.Headers)
            {
                var center = centroidSum[dimension] / SubjectCount;
                centroid.Add(dimension, center);
            }

            return centroid;
        }

        public Dictionary<string, double> GetExclusiveCentroid(Dictionary<string, double> centroidSum, Dictionary<string, double> excludedPoint)
        {
            var exclusiveCentroidSum = centroidSum.ToDictionary(x => x.Key, x => x.Value); // make a copy
            foreach (var dimension in Shared.Headers)
            {
                exclusiveCentroidSum[dimension] -= excludedPoint[dimension];
                exclusiveCentroidSum[dimension] /= SubjectCount - 1;
            }

            return exclusiveCentroidSum;
        }

        // builds the cluster given the subspace and samples that are not assigned to another clusters
        public static Cluster BuildCluster(Subspace subspace,
            Dictionary<string, Dictionary<string, double>> currentData)
        {
            var cluster = new Cluster
            {
                //Id = ((char)('A' + testCount)).ToString(), // A, B, C ...
                Id = testCount.ToString(),
                SubSpace = subspace
            };
            testCount++;
            // add the centroid first
            cluster.Subjects.Add(subspace.Centroid.Key, subspace.Centroid.Value);

            // find points
            foreach (var point in currentData)
            {
                var addPoint = true;

                // if is centroid skip, else check the distance for each subsapce
                if (point.Key == subspace.Centroid.Key)
                    continue;

                foreach (var dimension in subspace.Dimensions)
                {
                    var distance = Math.Abs(Shared.Data[subspace.Centroid.Key][dimension] - Shared.Data[point.Key][dimension]);

                    if (distance > SettingsMining.Width)
                    {
                        addPoint = false;
                        break;
                    }
                }

                if (addPoint)
                    cluster.Subjects.Add(point.Key, point.Value);
            }

            return cluster;
        }

        // returns sids
        public string SubjectsToString()
        {
            var sb = new StringBuilder();

            foreach (var key in SubjectSids)
                sb.AppendLine($"{key},{Id}");

            return sb.ToString();
        }

        public string DimensionsToString()
        {
            // first add headers
            var sb = new StringBuilder("");

            foreach (var sid in SubjectSids)
            {
                sb.Append(sid + ",");
                foreach (var dimension in Shared.Headers)
                {
                    var value = SubSpace.Dimensions.Contains(dimension) ? "1," : "0,";
                    sb.Append(value);
                }

                sb.Length--;
                sb.AppendLine("");
            }

            return sb.ToString();
        }

        public static List<Cluster> ReLabel(List<Cluster> clusters)
        {
            var orderedClusters = clusters.OrderByDescending(x => x.SubjectCount).ToList();
            for (var label = 0; label < orderedClusters.Count; label++)
                clusters[label].Id = label.ToString();

            return orderedClusters;
        }

        public static string ClusterSidsHeaders()
        {
            return "sid,cluster_label";
        }
    }
}