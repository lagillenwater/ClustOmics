using FOC.Utilities;

namespace FOC.Settings
{
    internal class SettingsEvaluation
    {
        public static readonly int ConnectednessK = 10; // how many nearest neighbors for Connectedness

        // -------------------------------------------
        // Evaluation Metrics
        // -------------------------------------------
        public static readonly bool Silhouette = true;

        public static readonly bool Connectedness = true;

        // path to the folder that clusterings are saved
        public static string MiningResultFolder =>
            @"C:\Users\helmis\Desktop\met2_2019-11-23_AE08-w[5,8;0.5]_a=0.1_b=0.25\ClusteringResults";

        public static string ParentPath =>
            Converters.GetParentFolderPath(MiningResultFolder); // assuming the summary file is in the parent folder

        public static string SummaryPath =>
            ParentPath + @"\EvaluationSummary.csv"; // a summary of evaluation will be saved here
    }
}