using FOC.Clustering;
using FOC.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace FOC.Evaluation.Silhouette
{
    class Silhouette
    {
        public static List<SilhouetteRecord> GetCoefficients(List<Cluster> clusters)
        { 

            var silhouetteRecords = new List<SilhouetteRecord>();

            // for each cluster
            foreach (var cluster in clusters)
            {
                var otherClusters = clusters.Where(x => x.Id != cluster.Id).ToList();

                // for each point the current cluster
                foreach (var point in cluster.Subjects)
                {
                    var silhouetteRecord = new SilhouetteRecord { Sid = point.Key, ClusterId = cluster.Id };
                    // inDistance <- compute average distance to member-points
                    silhouetteRecord.InDistance = ComputeAverageInnerDistance(point.Key, cluster.Subjects
                        .Where(x => x.Key != point.Key)
                        .Select(x => x.Key)
                        .ToList());

                    // outDistance <- find the average distance to the closest cluster
                    silhouetteRecord.OutDistance = ComputeOuterDistance(point.Key, otherClusters);

                    // compute Silhouette score
                    silhouetteRecords.Add(silhouetteRecord);
                }
            }

            return silhouetteRecords;
        }

        private static double ComputeAverageInnerDistance(
            string pointKey,
            List<string> pointKeys)
        {
            var distance = 0.0;

            foreach (var point in pointKeys)
                distance += Distance.GetDistance(pointKey, point);

            return distance / pointKeys.Count;
        }

        // computes the distance to the nearest cluster
        private static double ComputeOuterDistance(
             string pointKey,
            List<Cluster> otherClusters)
        {
            // compute avg distances to other clusters
            var minDistance = ComputeAverageClusterDistance(pointKey, otherClusters[0]);

            for (int i = 1; i < otherClusters.Count; i++)
            {
                var distance = ComputeAverageClusterDistance(pointKey, otherClusters[i]);

                if (distance < minDistance)
                    minDistance = distance;
            }

            return minDistance;
        }

        private static double ComputeAverageClusterDistance(string pointKey
            , Cluster cluster)
        {
            var distance = 0.0;

            foreach (var point in cluster.Subjects.Keys)
                distance += Distance.GetDistance(pointKey, point);

            return distance / cluster.Subjects.Count;
        }
    }
}