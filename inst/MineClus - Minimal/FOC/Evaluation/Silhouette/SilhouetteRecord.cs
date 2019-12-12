using System;

namespace FOC.Evaluation.Silhouette
{
    internal class SilhouetteRecord
    {
        public string Sid { get; set; }
        public double InDistance { get; set; } // inner distance
        public double OutDistance { get; set; } // outer distance
        public double CoEfficient => (OutDistance - InDistance) / Math.Max(OutDistance, InDistance); // Silhouette
        public string ClusterId { get; set; } // cluster label
    }
}