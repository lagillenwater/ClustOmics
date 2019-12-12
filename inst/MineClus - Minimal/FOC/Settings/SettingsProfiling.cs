using System;

namespace FOC.Settings
{
    internal class SettingsProfiling
    {
        public static int StartingTestId = 1; // in order to continue from the middle of another run

        // --------------------------------------------------
        // Mining parameters
        //public static readonly double WidthMin = 1;
        //public static readonly double WidthMax = 30;
        //public static readonly double WidthStep = 0.5;
        //public static readonly int ExtraMedoiodCoef = 1;
        //public static readonly bool MedoidAll = false; // test all samples as medoids

        public static readonly double WidthMin = 5; // starting w for grid search
        public static readonly double WidthMax = 8; // max w for grid search
        public static readonly double WidthStep = 0.5; // how much change w for each run
        public static readonly int ExtraMedoiodCoef = 2; // x times more medoids than the actual algorithm uses to increase reliability
        public static readonly bool MedoidAll = true; // test all samples as medoids

        public static readonly double Alpha = 0.1;
        public static readonly double Beta = 0.25;


        // --------------------------------------------
        // IO settings
        public static readonly bool SaveSummary = true; // save summary of grid search in file
        public static readonly bool SaveResults = true; // save results (clusterings) in file

        public static int Medoids => ExtraMedoiodCoef * (int) Math.Ceiling(2 / SettingsMining.Alpha);

        //---------------------------------------------
        // don't change the followings :)
        public static string BasePath => // path to the folder that results will be saved
            $@"{SettingsDataset.DatasetParentPath}\{SettingsDataset.DatasetName}" +
            $"-w[{WidthMin},{WidthMax};{WidthStep}]" +
            $"_a={Alpha}_b={Beta}";

        public static string SummaryPath => // path to the summary file
            BasePath + @"\ProfilingSummary.csv";

        public static int
            FileNameNumberPaddingLeft { get; set; } // leading 0's in, for items to be sorted correctly in Windows

        public static string ClusteringResultFolder => BasePath + @"\ClusteringResults\"; // clusterings folder path

        public static string ClusteringResultPath => ClusteringResultFolder + // actual path for saving clusterings
                                                     $"{StartingTestId.ToString().PadLeft(FileNameNumberPaddingLeft, '0')}" +
                                                     $"_w={Math.Round(SettingsMining.Width, 3)}.csv";
    }
}