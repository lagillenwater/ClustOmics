using System;
using System.Collections.Generic;
using System.Linq;
using FOC.FpGrowth;
using FOC.Settings;

namespace FOC.Clustering
{
    internal class FPC
    {
        private Dictionary<string, Dictionary<string, double>> _data; // sid => attribute => value

        public List<Cluster> Mine()
        {
            Cluster.testCount = 0;
            var clusters = new List<Cluster>();
            _data = new Dictionary<string, Dictionary<string, double>>(Shared.Data);

            var sampleSize = _data.Count;

            // output configs on console
            Console.WriteLine("Clustering Config:" +
                              $"\n  -Medoids per cluster={SettingsProfiling.Medoids}" +
                              $"\n  -Alpha={SettingsMining.Alpha}" +
                              $"\n  -Beta={SettingsMining.Beta}" +
                              $"\n  -Min SubjectCount={SettingsMining.MinPoints}" +
                              $"\n  -W={SettingsMining.Width}");

            Console.WriteLine("Data Config:" +
                              $"\n  -Samples={SettingsDataset.SampleSize}" +
                              $"\n  -Dimensions={SettingsDataset.Dimensions}");

            // reset the failed attempts
            var failedAttempts = 0;

            while (true)
            {
                var currentSize = _data.Count; // unclustered data points

                // check for termination
                if (currentSize <= SettingsMining.MinPoints || //not enough points left
                    failedAttempts >= SettingsMining.MaxAttempts) // failed to find a cluster multiple times in a row
                    //SettingsMining.Medoids >= currentSize) 
                    //(double)currentSize / sampleSize < SettingsMining.LeftPercentage) // enough points clustered
                    break;

                // pick centroids
                var medoidsCount = SettingsProfiling.Medoids >= currentSize ? currentSize : SettingsProfiling.Medoids;

                Dictionary<string, Dictionary<string, double>> medoids;
                if (SettingsProfiling.MedoidAll)
                    medoids = new Dictionary<string, Dictionary<string, double>>(_data);

                else
                    medoids = PickCentroidsRandom(medoidsCount);

                // for each centroid, build transactions 
                // get the best cluster for the current centroid
                // if its score is higher replace with the previous best

                var bestSubspace = new Subspace();

                Console.WriteLine("---------------------------------------------------" +
                                  $"\nLooking for cluster # {Cluster.testCount + 1}\n");
                var iteration = 1;
                foreach (var centroid in medoids)
                {
                    Console.WriteLine($"\nIteration {iteration} out of {medoids.Count}");
                    var transactions = BuildTransactions(centroid.Key);

                    var fpGrowth = new FpGrowthMiner();
                    var newSubspace = fpGrowth.Mine(transactions.Values.ToList());

                    var newBestScore = newSubspace.Score;

                    if (newBestScore.IsHigher(bestSubspace.Score))
                    {
                        bestSubspace = newSubspace;
                        bestSubspace.Centroid = centroid;
                        Console.WriteLine($"-A better cluster found with {newSubspace.Score}");
                    }

                    iteration++;
                } // best cluster of all centroid found

                if (bestSubspace.Score.IsZero())
                {
                    failedAttempts++;
                    continue;
                }

                failedAttempts = 0;

                // build the cluster
                var newCluster = Cluster.BuildCluster(bestSubspace, _data);
                clusters.Add(newCluster);

                // delete the assigned point from current data
                var deleteList = newCluster.Subjects.Keys;
                foreach (var deleteKey in deleteList)
                    _data.Remove(deleteKey);

                Console.WriteLine(
                    $"New cluster {clusters.Count} found" +
                    $"\n\tC:{newCluster.Subjects.Count}, " +
                    $"\n\tD:{newCluster.SubSpace.Dimensionality}");

                Console.WriteLine($"Remaining points {_data.Count} out of {sampleSize}");
            }

            Console.WriteLine();
            return clusters;
        }

        // picks random points as centroids
        private Dictionary<string, Dictionary<string, double>> PickCentroidsRandom(int count)
        {
            var centroids = new Dictionary<string, Dictionary<string, double>>();
            var keys = _data.Keys.ToList();
            var random = new Random();

            while (centroids.Count < count)
            {
                var index = random.Next(0, _data.Count);
                var key = keys[index];

                if (!centroids.ContainsKey(key))
                    centroids.Add(key, _data[key]);
            }

            return centroids;
        }

        // builds the transactions for the current centroid
        // tid: dimension, items: sids that are close to the centroid in that dimension
        private Dictionary<string, HashSet<string>> BuildTransactions(string centroidKey)
        {
            var transactions = new Dictionary<string, HashSet<string>>();
            var centroidData = Shared.Data[centroidKey];

            foreach (var point in _data)
                if (point.Key != centroidKey)
                {
                    var key = point.Key;
                    var dimensions = new HashSet<string>();


                    foreach (var value in point.Value)
                        if (Math.Abs(point.Value[value.Key] - centroidData[value.Key]) <= SettingsMining.Width)
                            dimensions.Add(value.Key);

                    if (dimensions.Count > 0)
                        transactions.Add(key, dimensions);
                }

            return transactions;
        }
    }
}