using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using FOC.Evaluation.Connectedness;
using FOC.Evaluation.Silhouette;
using FOC.IO;
using FOC.Settings;

namespace FOC.Profiling
{
    internal class EvaluationProfiler
    {
        public static void Profile()
        {
            // load data
            Shared.Data = Import.ImportDataset();
            SettingsDataset.SetDatasetInfo(Shared.Data.Count, Shared.Headers.Count);

            //File.WriteAllText(SettingsEvaluation.SummaryPath, "");
            var summaryPath = SettingsEvaluation.SummaryPath;
            var summaryLine = new StringBuilder("test_id,w"); // this will become the header for the summary file

            // generate the Silhouette Configs
            if (SettingsEvaluation.Silhouette)
                summaryLine.Append(",cluster_silhouette,overall_silhouette"); // get the Silhouette for each cluster

            // generate the Connectedness configs
            if (SettingsEvaluation.Connectedness)
                summaryLine.Append($",connectedness_k={SettingsEvaluation.ConnectednessK}");

            File.WriteAllText(summaryPath, summaryLine + "\n"); // add headers to summary path

            //get list of files
            var files = IoOperations.GetFileList(SettingsEvaluation.MiningResultFolder, "Summary").ToList();
            Console.WriteLine($"\nTests: {files.Count}");

            var testCounter = 1;
            foreach (var file in files)
            {
                Console.WriteLine($"\n\nTest # {testCounter} of {files.Count}");
                testCounter++;

                summaryLine.Clear();

                var testInfo = file.Split('\\').Last().Split('_');
                var testId = testInfo.First();
                var tempW = new StringBuilder(testInfo[1].Split('=').Last().Split('c').First());
                tempW.Length--;
                var w = tempW.ToString();

                testId = int.Parse(testId).ToString(); // removing the leading 0's
                summaryLine.Append($"{testId},{w}");
                var clusters = Import.LoadClustersFromFile(file);

                if (SettingsEvaluation.Silhouette)
                {
                    Console.Write("\t computing silhouette ... ");
                    var silhouetteRecords = Silhouette.GetCoefficients(clusters);
                    var overallSilhouette = Math.Round(silhouetteRecords.Average(x => x.CoEfficient), 4);
                    var individualSilhouettes = GetSilhouettePerCluster(silhouetteRecords);
                    summaryLine.Append($",{individualSilhouettes},{overallSilhouette}");
                    Console.WriteLine("done");
                }

                if (SettingsEvaluation.Connectedness)
                {
                    Console.Write("\t computing connectedness ... ");
                    var connectednessRecords = ConnectednessKnn.GetConnections(clusters);
                    var averageConnectedness = Math.Round(connectednessRecords.Select(x => x.K).Average() / 10, 4);
                    summaryLine.Append($",{averageConnectedness}");
                    Console.WriteLine("done");
                }

                Export.AppendToFile(summaryPath, summaryLine + "\n");
            }

            Export.MergeColumns(SettingsEvaluation.ParentPath + "\\OverallSummary.csv",
                SettingsEvaluation.ParentPath + "\\ProfilingSummary.csv", SettingsEvaluation.SummaryPath, 2);
        }

        // calculates the Silhouette for each cluster
        private static string GetSilhouettePerCluster(List<SilhouetteRecord> individualSilhouettes)
        {
            var sb = new StringBuilder();

            var labels = individualSilhouettes.Select(x => x.ClusterId).Distinct().OrderBy(x => x).ToList();

            foreach (var label in labels)
            {
                var silhouetteRecordsForLabel = individualSilhouettes.Where(x => x.ClusterId == label);
                var sil = silhouetteRecordsForLabel.Select(x => x.CoEfficient).Average();
                sb.Append($"{silhouetteRecordsForLabel.Count()}: {Math.Round(sil, 4)}; ");
            }

            return sb.ToString();
        }
    }
}