using System;
using FOC.Profiling;

namespace FOC
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("1- Cluster" +
                              "\n2- Evaluate");
            //"\n4- Convert To New Format"); 
            var option = Console.ReadLine();

            switch (option)
            {
                case "1": // perform a grid search
                    MiningProfiler.Profile();
                    break;

                case "2": // evaluate the clusterings in batch
                    EvaluationProfiler.Profile();
                    break;
            }
        }
    }
}