using System.Linq;
using FOC.Utilities;

namespace FOC.Settings
{
    internal class SettingsDataset
    {
        // path to dataset
        public static string DatasetPath => @"C:\Users\helmis\Desktop\met2_2019-11-23_AE08.csv";

        // returns parent folder of the dataset
        public static string DatasetParentPath => Converters.GetParentFolderPath(DatasetPath);

        // returns the dataset name without the path and extension
        public static string DatasetName => DatasetPath.Split('\\').Last().Split(".").First();

        public static int SampleSize { get; set; } // number of samples in the dataset
        public static int Dimensions { get; set; } // number of dimensions (features in the dataset)

        public static void SetDatasetInfo(int sampleSize, int dimensions)
        {
            SampleSize = sampleSize;
            Dimensions = dimensions;
        }
    }
}