using System.Collections.Generic;
using System.Linq;
using FOC.Clustering;
using FOC.Settings;

namespace FOC.FpGrowth
{
    internal class FpGrowthMiner
    {
        private List<string> BestItemset = new List<string>();
        private Score BestScore = new Score();

        private int BestSupport;

        //List<ItemSupport> F = new List<ItemSupport>();
        public Subspace Mine(List<HashSet<string>> transactions)
        {
            // frequency count for single items
            var temp = ComputeItemSupport(transactions);
            var orderedItems = new List<ItemSupport>();
            foreach (var orderedItem in temp)
                orderedItems.Add(new ItemSupport {Item = orderedItem.Key, Support = orderedItem.Value.Support});

            orderedItems = orderedItems.OrderBy(x => x.Support).ThenByDescending(x => x.Item).ToList();
            // build re-ordered/refined transactions
            var refinedTransactions = RefineTransactions(temp, transactions);

            MineFP(refinedTransactions, orderedItems, new ConditionalItem());

            var subspace = new Subspace {Dimensions = BestItemset.ToHashSet(), SubjectCount = BestSupport};
            return subspace; //bestItemset;
        }

        // compute the support for single items
        private Dictionary<string, OrderSupport> ComputeItemSupport(List<HashSet<string>> transactions)
        {
            var result = new Dictionary<string, OrderSupport>();
            var candidates = new Dictionary<string, int>();

            foreach (var transaction in transactions)
            foreach (var item in transaction)
            {
                if (!candidates.ContainsKey(item))
                    candidates.Add(item, 0);

                candidates[item]++;
            }

            var orderedFrequentItems = candidates.Where(x => x.Value >= SettingsMining.MinPoints)
                .OrderByDescending(x => x.Value).ThenBy(x => x.Key).ToList();

            var order = 1;
            foreach (var item in orderedFrequentItems)
            {
                var itemFrequency = new OrderSupport
                {
                    Support = item.Value,
                    Order = order
                };

                order++;

                result.Add(item.Key, itemFrequency);
            }

            return result;
        }

        // deletes items that are not frequent and orders the rest based on their frequencies
        private List<List<string>> RefineTransactions(Dictionary<string, OrderSupport> orderedFrequentItems,
            List<HashSet<string>> transactions)
        {
            var result = new List<List<string>>();

            foreach (var transaction in transactions)
            {
                var unorderedTransaction = new List<ItemOrder>();
                foreach (var item in transaction)
                    if (orderedFrequentItems.ContainsKey(item)) // if this item is not frequent then skip
                        unorderedTransaction.Add(new ItemOrder // else keep it and add get its order
                        {
                            Item = item,
                            Order = orderedFrequentItems[item].Order
                        });

                if (unorderedTransaction.Count > 0)
                {
                    unorderedTransaction = unorderedTransaction.OrderBy(x => x.Order).ToList();
                    result.Add(unorderedTransaction.Select(x => x.Item).ToList());
                }
            }

            return result;
        }

        private void MineFP(
            List<List<string>> transactions
            , List<ItemSupport> orderedItems // ascending
            , ConditionalItem conditionalItem)
        {
            if (IsPath(transactions))
            {
                GenerateFrequentItemsets(orderedItems, conditionalItem);
            }

            else
            {
                var firstItem = new List<string>(conditionalItem.Items) {orderedItems.Last().Item};
                var firstSupport = orderedItems.Last().Support;
                UpdateBestItemset(firstItem, firstSupport);

                for (var i = 0; i < orderedItems.Count - 1; i++)
                {
                    var itemSupport = orderedItems[i];
                    var newConItem = new ConditionalItem
                        {Items = new List<string>(conditionalItem.Items) {itemSupport.Item}};
                    var maxScore = new Score {Dimensions = orderedItems.Count - i, Points = itemSupport.Support};
                    if (!maxScore.IsHigher(BestScore))
                        continue;

                    // compute support of newConItems
                    var conditionalPattern = GenerateConditionalPattern(transactions, newConItem);
                    if (conditionalPattern == null)
                        continue;

                    newConItem.Support = conditionalPattern.Support;

                    UpdateBestItemset(newConItem.Items, conditionalPattern.Support);

                    transactions = ProjectTransactions(transactions, itemSupport.Item);

                    if (conditionalPattern.Transactions.Any()) // call fp for this new tree
                        MineFP(conditionalPattern.Transactions, conditionalPattern.ItemList, newConItem);
                }
            }
        }

        private ConditionalPattern GenerateConditionalPattern(List<List<string>> originalTransactions,
            ConditionalItem conditionalItems)
        {
            var transactions = new List<List<string>>(originalTransactions.Select(x => x.ToList()));

            var itemSupports = new Dictionary<string, int>();

            var conditionalPattern = new ConditionalPattern {ConditionalItems = conditionalItems.Items};

            var currentItem = conditionalItems.Items.Last();

            for (var t = transactions.Count - 1; t >= 0; t--)
            {
                var transaction = transactions[t];

                if (transaction.Last() == currentItem)
                {
                    transaction.RemoveAt(transaction.Count - 1); // remove the item
                    conditionalPattern.Transactions.Add(transaction);

                    foreach (var item in transaction)
                    {
                        if (!itemSupports.ContainsKey(item))
                            itemSupports.Add(item, 0);

                        itemSupports[item]++;
                    }
                }
            }

            var deleteList = new List<string>();
            foreach (var itemSupport in itemSupports)
                if (itemSupport.Value >= SettingsMining.MinPoints)
                    conditionalPattern.ItemList.Add(new ItemSupport
                    {
                        Item = itemSupport.Key,
                        Support = itemSupport.Value
                    });

                else deleteList.Add(itemSupport.Key);

            // sort from the lowest support to the largest support
            conditionalPattern.ItemList = conditionalPattern.ItemList.OrderBy(x => x.Support)
                .ThenByDescending(x => x.Item).ToList();

            // remove items with support less than min points since they cannot generate a valid cluster
            for (var i = conditionalPattern.Transactions.Count - 1; i >= 0; i--)
            {
                conditionalPattern.Transactions[i] = conditionalPattern.Transactions[i].Except(deleteList).ToList();
                if (conditionalPattern.Transactions[i].Count == 0)
                    conditionalPattern.Transactions.RemoveAt(i);
            }

            conditionalPattern.Support = conditionalPattern.Transactions.Count;

            return conditionalPattern.Support == 0 ? null : conditionalPattern;
        }

        // generates the projected tree
        private List<List<string>> ProjectTransactions(List<List<string>> originalTransactions, string item)
        {
            var transactions = new List<List<string>>(originalTransactions.Select(x => x.ToList()));
            var result = new List<List<string>>();

            for (var t = transactions.Count - 1; t >= 0; t--)
            {
                var transaction = transactions[t];
                if (transaction.Count == 0)
                    continue;

                if (transaction.Last() == item)
                    transaction.RemoveAt(transaction.Count - 1);

                if (transaction.Count > 0)
                    result.Add(transaction);
            }

            return result;
        }

        // checks if the transaction tree contains a single path or not
        private bool IsPath(List<List<string>> orgitanlTransactions)
        {
            var transactions =
                new List<List<string>>(orgitanlTransactions.Where(x => x.Count > 0).Select(x => x.ToList()));

            while (transactions.Count > 1)
            {
                var currentItem = transactions[0][0];

                for (var i = transactions.Count - 1; i >= 0; i--)
                {
                    if (transactions[i][0] != currentItem)
                        return false;

                    transactions[i].RemoveAt(0);

                    if (transactions[i].Count == 0)
                        transactions.RemoveAt(i);
                }
            }

            return true;
        }

        // generates the frequent itemsets
        // not all the combinations, only the sequence from to most frequent to least
        private void GenerateFrequentItemsets(List<ItemSupport> items, ConditionalItem conditionalItem)
        {
            items.Reverse(); // to start from the highest supprt to lowest
            //check the best possible score
            var maxSupport = conditionalItem.Support;
            var maxItems = items.Count;

            var maxScore = new Score {Dimensions = maxItems, Points = maxSupport};
            if (!maxScore.IsHigher(BestScore))
                return;

            // start from the item with highest support and add res
            var currentDims = conditionalItem.Items;
            var currentSupport = conditionalItem.Support;

            UpdateBestItemset(currentDims, currentSupport);

            // todo: add all with the same support at once
            foreach (var itemSupport in items)
            {
                currentDims.Add(itemSupport.Item);
                currentSupport = itemSupport.Support;

                UpdateBestItemset(currentDims, currentSupport);
            }
        }

        private void UpdateBestItemset(List<string> itemsets, int support)
        {
            var newScore = new Score {Dimensions = itemsets.Count, Points = support};

            if (newScore.IsHigher(BestScore))
            {
                BestScore = newScore;
                BestItemset = itemsets;
                BestSupport = support;
            }
        }
    }
}