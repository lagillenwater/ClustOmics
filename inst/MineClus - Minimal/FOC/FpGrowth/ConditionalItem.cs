using System.Collections.Generic;

namespace FOC.FpGrowth
{
    internal class ConditionalItem
    {
        public ConditionalItem()
        {
            Items = new List<string>();
        }

        public List<string> Items { get; set; }
        public int Support { get; set; }
    }
}