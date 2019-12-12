using System;
using System.Collections.Generic;
using System.Diagnostics;
using FOC.Clustering;
using FOC.IO;
using FOC.Settings;

namespace FOC.Profiling
{
    // this class does a grid search given a list of configs
    internal class MiningProfiler
    {
        public static void Profile()
        {
            // create the folders for the summary file and clusterings
            var summaryPath = SettingsProfiling.SummaryPath;
            IoOperations.CreateFolder(SettingsProfiling.BasePath);
            IoOperations.ClearFolder(SettingsProfiling.BasePath);

            IoOperations.CreateFolder(SettingsProfiling.ClusteringResultFolder);
            IoOperations.ClearFolder(SettingsProfiling.ClusteringResultFolder);

            // generate configs
            var configs = Config.GenerateConfigs();
            // set the number of leading 0's in the file name according to the number of configs
            SettingsProfiling.FileNameNumberPaddingLeft = (int) Math.Ceiling(Math.Log10(configs.Count));

            // load data
            Shared.Data = Import.ImportDataset();
            SettingsDataset.SetDatasetInfo(Shared.Data.Count, Shared.Headers.Count);

            // headers for the summary file
            var summaryHeaders =
                "test_id,w,a,b,runtime(s)" +
                ",clusters,avg_cluster_size,cluster_sizes,outliers" +
                ",avg_dimensions,dimensions,features\n";

            if (SettingsProfiling.SaveSummary)
                Export.AppendToFile(summaryPath, summaryHeaders);

            // this is used to show to progress in the console
            var startingTestId = SettingsProfiling.StartingTestId - 1;

            foreach (var config in configs)
            {
                Console.WriteLine(config.GetConsoleString()); // show the current config on console
                // write the progress in debugger output
                Debug.Write(config + $", {SettingsProfiling.StartingTestId} of {configs.Count + startingTestId}, ");
                SettingsMining.SetConfig(config); // set the mining parameters to the current config
                var clusters = MineClusters();
                // write summary of the clustering to the debugger output
                Debug.WriteLine(clusters.Count + " cluster" + (clusters.Count == 1 ? "" : "s") + " found.");
                SettingsProfiling.StartingTestId++;

                // if there are less than two clusters, do not save it
                if (clusters.Count < 2)
                    continue;

                // save the results to file
                if (SettingsProfiling.SaveSummary)
                {
                    // update the summary results
                    var clustersSummary = new ClustersSummary(clusters);
                    var summaryString =
                        $"{SettingsProfiling.StartingTestId},{config.Width},{config.Alpha},{config.Beta},{Shared.Runtime}" +
                        $",{clusters.Count},{clustersSummary.AvgPoints}" +
                        $",{clustersSummary.AllPoints},{clustersSummary.Outliers}" +
                        $",{clustersSummary.AvgDimensions},{clustersSummary.AllDimensions},{clustersSummary.SubspaceFeatures}\n";

                    Export.AppendToFile(summaryPath, summaryString);
                }

                // save the actual clusterings
                if (SettingsProfiling.SaveResults)
                {
                    clusters = Cluster.ReLabel(clusters);
                    Export.ExportClustersNoValues(clusters, SettingsProfiling.ClusteringResultPath);
                }
            }
        }

        private static List<Cluster> MineClusters()
        {
            //read input
            Shared.Data = Import.ImportDataset();
            SettingsDataset.SetDatasetInfo(Shared.Data.Count, Shared.Headers.Count);
            var fpc = new FPC();
            var watch = Stopwatch.StartNew(); // start the timer
            var clusters = fpc.Mine();
            watch.Stop(); // stop the timer
            Shared.Runtime = watch.ElapsedMilliseconds / 1000.0; // convert to seconds
            return clusters;
        }
    }
}