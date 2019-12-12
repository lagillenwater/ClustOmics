using System.Collections.Generic;

namespace FOC.Evaluation.Connectedness
{
    internal class KnnRecord
    {
        public KnnRecord()
        {
            KIndecies = new List<int>();
        }

        public string Sid { get; set; }
        public int K { get; set; }
        public List<int> KIndecies { get; set; }
    }
}