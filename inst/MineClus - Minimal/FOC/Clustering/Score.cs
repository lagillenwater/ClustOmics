using System;
using FOC.Settings;

namespace FOC.Clustering
{
    internal class Score
    {
        public int Dimensions { get; set; }
        public int Points { get; set; }

        public bool IsHigher(Score score)
        {
            var coefficient = 1 / SettingsMining.Beta;
            var deltaDim = Dimensions - score.Dimensions;

            if (deltaDim == 0)
                return Points > score.Points;

            if (deltaDim < 0)
                return Points > score.Points * Math.Pow(coefficient, deltaDim * -1);

            return Points * Math.Pow(coefficient, deltaDim) > score.Points;
        }

        public bool IsZero()
        {
            return Dimensions == 0 || Points == 0;
        }

        public override string ToString()
        {
            return $"Dimensions: {Dimensions}, SubjectCount: {Points}";
        }
    }
}