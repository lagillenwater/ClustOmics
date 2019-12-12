using System.Collections.Generic;
using System.Linq;

namespace FOC.FpGrowth
{
    internal class ConditionalPattern
    {
        public ConditionalPattern()
        {
            ConditionalItems = new List<string>();
            ItemList = new List<ItemSupport>();
            Transactions = new List<List<string>>();
        }

        public int Support { get; set; }
        public List<string> ConditionalItems { get; set; }
        public List<ItemSupport> ItemList { get; set; } // items in this pattern with their support
        public List<List<string>> Transactions { get; set; } // transaction

        public override string ToString()
        {
            return $"Item:{ConditionalItems.Last()}, Support:{Support}, Dims:{ItemList.Count}";
        }
    }
}