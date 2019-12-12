using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FOC.Clustering;
using FOC.Settings;
using FOC.Utilities;

namespace FOC.IO
{
    internal static class Export
    {
        // appends text to file
        public static void AppendToFile(string path, string content)
        {
            File.AppendAllText(path, content);
        }

        // exports clusterings (only label and sid)
        public static void ExportClustersNoValues(List<Cluster> clusters, string resultPath,
            bool exportDimensions = false)
        {
            // add subject ids and cluster labels

            File.WriteAllText(resultPath, Cluster.ClusterSidsHeaders() + "\n"); //sid, cluster_label
            foreach (var cluster in clusters)
                AppendToFile(resultPath, cluster.SubjectsToString());
            AppendToFile(resultPath, "\n");

            // export labels 
            var headersString = new StringBuilder("sid,");
            foreach (var header in Shared.Headers)
                headersString.Append($"{SettingsDataset.DatasetName}_{header},");
            headersString.Length--;

            // export subspace
            if (exportDimensions)
            {
                headersString.AppendLine();
                AppendToFile(resultPath, headersString.ToString());
                foreach (var cluster in clusters)
                    AppendToFile(resultPath, cluster.DimensionsToString());
            }
        }

        // merges to file horizontally (for mining and evaluation summary into overall summary)
        public static void MergeColumns(string resultPath, string filePath1, string filePath2, int startColumnFile2)
        {
            File.WriteAllText(resultPath, "");
            var file1 = File.ReadAllLines(filePath1);
            var file2 = File.ReadAllLines(filePath2);

            for (var i = 0; i < file1.Length; i++)
            {
                var result = file1[i];
                var line2Values = file2[i].Split(',').Skip(startColumnFile2 - 1).ToList();
                var line2 = Converters.ListToString(line2Values, ",", false);
                result += "," + line2;
                AppendToFile(resultPath, result + "\n");
            }
        }
    }
}