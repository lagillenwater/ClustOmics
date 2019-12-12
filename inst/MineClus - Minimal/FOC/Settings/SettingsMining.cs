using System;
using FOC.Utilities;

namespace FOC.Settings
{
    internal class SettingsMining
    {
        public static double Width = 2.5; // max allowed distance from medoid

        public static double Alpha = 0.5; // minimum points in one cluster

        public static double Beta = 0.25; // balancing factor

        // the number of times that MineClus fails to find a new clusters in a row before termination
        public static readonly int MaxAttempts = 2;

        // IO settings for saving results
        public static readonly bool SaveResult = true; // save the clustering in
        public static string ParentPath = Converters.GetParentFolderPath(SettingsDataset.DatasetPath);

        public static int MinPoints =>
            (int) Math.Ceiling(Alpha * SettingsDataset.SampleSize); // Min points in a cluster

        public static string ResultPath => ParentPath + $@"\Mining\w={Width}, a={Alpha}, b={Beta}.csv";

        public static void SetConfig(Config config)
        {
            Width = config.Width;
            Alpha = config.Alpha;
            Beta = config.Beta;
        }
    }
}