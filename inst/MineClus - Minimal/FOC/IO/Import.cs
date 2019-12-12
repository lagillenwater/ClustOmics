using System.Collections.Generic;
using System.IO;
using System.Linq;
using FOC.Clustering;
using FOC.Settings;

namespace FOC.IO
{
    internal static class Import
    {
        // reads dataset from file and transforms it to the form of sid => feature => value
        public static Dictionary<string, Dictionary<string, double>> ImportDataset(string inPath = "")
        {
            var result = new Dictionary<string, Dictionary<string, double>>();

            var path = inPath == string.Empty ? SettingsDataset.DatasetPath : inPath;

            var rawData = File.ReadAllLines(path).ToList();
            var headers = rawData[0].Split(',').ToList();
            headers.RemoveAt(0);
            SettingsDataset.Dimensions = headers.Count; // excluding sid
            rawData.RemoveAt(0); // remove headers from data

            foreach (var line in rawData)
            {
                if (line.Trim() == string.Empty) // skip the empty lines
                    continue;

                var row = line.Split(',').ToList();
                var key = row[0];
                row.RemoveAt(0); // remove sid
                result.Add(key, new Dictionary<string, double>());

                for (var i = 0; i < row.Count; i++)
                    result[key].Add(headers[i], double.Parse(row[i]));
            }

            return result;
        }

        // loads clusterings from file and converts them to list of clusters
        public static List<Cluster> LoadClustersFromFile(string clustersPath, string dataFilePath = "")
        {
            List<string> headers;
            Dictionary<string, Dictionary<string, double>> dataset; // dataset

            if (dataFilePath == "")
            {
                dataset = Shared.Data;
                headers = Shared.Headers;
            }

            else
            {
                dataset = ImportDataset(dataFilePath);

                headers = dataset.First().Value
                    .Select(x => x.Key)
                    .OrderBy(x => x)
                    .ToList();
            }

            var clustersFile = File.ReadAllLines(clustersPath).ToList();

            // find the clustering assignment index
            var clusteringIndex = 1;

            // find the subspace index
            var dimensionIndex = 0;
            for (var i = clusteringIndex + 1; i < clustersFile.Count; i++)
                if (clustersFile[i].Trim() == "")
                {
                    dimensionIndex = i + 2;
                    break;
                }

            var clusters = new Dictionary<string, Cluster>(); // cluster id, cluster
            var sidLabels = new Dictionary<string, string>(); // sid, label
            // for each clustering assignment
            for (var line = 1; line < dimensionIndex - 2; line++)
            {
                var assignment = clustersFile[line].Split(",");
                var sid = assignment[0];
                var label = assignment[1];

                if (!clusters.ContainsKey(label))
                    clusters.Add(label, new Cluster {Id = label});

                clusters[label].Subjects.Add(sid, dataset[sid]);
                sidLabels.Add(sid, label);
            }

            for (var line = dimensionIndex; line < clustersFile.Count; line++)
            {
                // add dimensions
                var dimValues = clustersFile[line].Split(',').ToList();
                var sid = dimValues.First();
                var label = sidLabels[sid];

                for (var d = 1; d < dimValues.Count; d++)
                {
                    if (dimValues[d] == "0")
                        continue;

                    clusters[label].SubSpace.Dimensions.Add(headers[d - 1]);
                }
            }

            var result = clusters.Select(x => x.Value).ToList();
            return result;
        }
    }
}