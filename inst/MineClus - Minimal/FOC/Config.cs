using System;
using System.Collections.Generic;
using System.Linq;
using FOC.Settings;

namespace FOC
{
    internal class Config
    {
        public double Width { get; set; }
        public double Alpha { get; set; }
        public double Beta { get; set; }


        // returns a set of configurations, each containing W, Alpha, Beta
        // uses the ranges set for each parameter in the SettingsProfiling.cs
        public static List<Config> GenerateConfigs()
        {
            var configs = new List<Config>();
            var wList = GenerateRange(SettingsProfiling.WidthMin, SettingsProfiling.WidthMax,
                SettingsProfiling.WidthStep);

            foreach (var w in wList)
                configs.Add(new Config {Width = w, Alpha = SettingsProfiling.Alpha, Beta = SettingsProfiling.Beta});

            // sorts configs based on w, then alpha, then beta
            return configs.OrderBy(x => x.Width)
                .ThenBy(x => x.Alpha)
                .ThenBy(x => x.Beta)
                .ToList();
        }

        // returns a range between min and max with. Steps determines the difference between two successive values in the range
        private static List<double> GenerateRange(double min, double max, double step)
        {
            var range = new List<double>();

            for (var value = min; value <= max; value += step)
                range.Add(value);

            return range;
        }

        // returns w, alpha, beta
        public override string ToString()
        {
            return $"w:{Math.Round(Width, 3)}, alpha:{Alpha}, beta:{Beta}";
        }

        // for showing the progress in Console output
        public string GetConsoleString()
        {
            return "----------------------------------" +
                   $"\n{this}" +
                   "\n----------------------------------\n";
        }
    }
}