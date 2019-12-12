using System.Collections.Generic;
using System.Linq;
using FOC.Clustering;
using FOC.Settings;
using FOC.Utilities;

namespace FOC.Evaluation.Connectedness
{
    internal class ConnectednessKnn
    {
        private static int K => SettingsEvaluation.ConnectednessK;

        public static List<KnnRecord> GetConnections(List<Cluster> clusters)
        {
            var knnRecords = new List<KnnRecord>();
            var pointClusters = new Dictionary<string, string>(); // sids and their cluster

            foreach (var cluster in clusters)
            foreach (var point in cluster.Subjects)
                pointClusters.Add(point.Key, cluster.Id);

            foreach (var cluster in clusters)
            foreach (var point in cluster.Subjects)
            {
                var closestNeighbors = FindNearestNeighbors(point.Key);
                var kResult = ComputeK(closestNeighbors, pointClusters, cluster.Id);
                var knnRecord = new KnnRecord {Sid = point.Key, K = kResult.Count, KIndecies = kResult};
                knnRecords.Add(knnRecord);
            }

            return knnRecords;
        }

        private static List<string> FindNearestNeighbors(string id)
        {
            var distances = new Dictionary<string, double>();

            foreach (var point in Shared.Data)
            {
                if (point.Key == id)
                    continue;

                var distance = Distance.GetDistance(id, point.Key);
                distances.Add(point.Key, distance);
            }

            var result = distances
                .OrderBy(x => x.Value) // order based on distance
                .Take(K) // take top K
                .Select(x => x.Key) // select the Sids
                .ToList();

            return result;
        }

        // first: list of the K's that are in the same cluster, second: count of Ks that are in the cluster
        private static List<int> ComputeK(List<string> closestNeighbors, Dictionary<string, string> pointClusters,
            string clusterId)
        {
            var inK = new List<int>();
            for (var index = 0; index < closestNeighbors.Count; index++)
            {
                var closestNeighbor = closestNeighbors[index];

                if (!pointClusters.ContainsKey(closestNeighbor))
                    continue;

                if (pointClusters[closestNeighbor] == clusterId)
                    inK.Add(index);
            }

            return inK;
        }
    }
}