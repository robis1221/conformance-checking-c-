using System;
using System.Collections.Generic;
using System.Linq;

namespace ProcessMining
{
    public static class CasualFootprints
    {

        public static decimal CalculateCasualFootprintFitness(PetriNet petriNet, Dictionary<List<string>, int> trace_frequencies)
        {
            var petriNetCasualFootprintTable = CreateFootprintTable(petriNet);
            var logCasualFootprintTable = CreateFootprintTable(trace_frequencies, petriNet.TransitionsNameToId);

            return CalculateFitness(petriNetCasualFootprintTable, logCasualFootprintTable);
        }

        private static SuccessionType[,] CreateFootprintTable(PetriNet petriNet)
        {
            var transactions = petriNet.Transitions;

            var footprintTable = new SuccessionType[transactions.Count, 
                                    transactions.Count];

            for (int i = 0; i < transactions.Count; i++)
            {
                var transactionId = transactions[-(i + 1)].id;
                var childrenIds = petriNet.GetIdsOfChildTransactions(transactionId);
                var parentIds = petriNet.GetIdsOfParentTransactions(transactionId);

                for (int j = 0; j < transactions.Count; j++)
                {
                    footprintTable[i, j] = DetermineSuccessionType(childrenIds, 
                                            parentIds, -(j + 1));
                }
            }
            return footprintTable;
        }

        private static SuccessionType[,] CreateFootprintTable(Dictionary<List<string>, int> trace_frequencies,
            Dictionary<string, int> transitionsNameToId)
        {
            var directSuccessions = AlphaMiner.GetDirectSuccessions(trace_frequencies);

            List<KeyValuePair<int, int>> directSucessionList = new List<KeyValuePair<int, int>>();

            foreach (var directSuccession in directSuccessions)
            {
                var key = transitionsNameToId[directSuccession.Item1];
                var value = transitionsNameToId[directSuccession.Item2];

                directSucessionList.Add(new KeyValuePair<int, int>(key, value));
            }

            var ids = directSucessionList.Select(x => x.Key).ToList();
            ids.AddRange(directSucessionList.Select(x => x.Value).ToList());
            ids = ids.Distinct().OrderByDescending(x => x).ToList();

            var footprintTable = new SuccessionType[ids.Count, ids.Count];

            for (int i = 0; i < ids.Count; i++)
            {
                var casualityChildIdList = directSucessionList.Where(kvp => kvp.Key == -(i + 1)).Select(kvp => kvp.Value).ToList();
                var casualityParentIdList = directSucessionList.Where(kvp => kvp.Value == -(i + 1)).Select(kvp => kvp.Key).ToList();
                var parallerIdList = casualityChildIdList.Intersect(casualityParentIdList).ToList();

                for (int j = 0; j < ids.Count; j++)
                {
                    var nodeId = -(j + 1);
                    footprintTable[i, j] = DetermineSuccessionType(casualityChildIdList, casualityParentIdList,
                        parallerIdList, nodeId);                   
                }
            }

            return footprintTable;
        }

        private static SuccessionType DetermineSuccessionType(List<int> childrenIds, List<int> parentIds, int transactionId)
        {
            bool isInChildrenIds = childrenIds.Contains(transactionId);
            bool isInParentIds = parentIds.Contains(transactionId);

            if (isInChildrenIds && isInParentIds)
            {
                return SuccessionType.Parallel;
            }
            else if (isInChildrenIds)
            {
                return SuccessionType.CasualityChild;
            }
            else if (isInParentIds)
            {
                return SuccessionType.CasualityParent;
            }

            return SuccessionType.Choice;
        }

        private static SuccessionType DetermineSuccessionType(List<int> casualityChildIdList, List<int> casualityParentIdList,
            List<int> parallerIdList, int nodeId)
        {
            if (parallerIdList.Contains(nodeId))
            {
                return SuccessionType.Parallel;
            }
            if (casualityChildIdList.Contains(nodeId))
            {
                return SuccessionType.CasualityChild;
            }
            else if (casualityParentIdList.Contains(nodeId))
            {
                return SuccessionType.CasualityParent;
            }

            return SuccessionType.Choice;
        }

        private static decimal CalculateFitness(SuccessionType[,] footprintsTable1, SuccessionType[,] footprintsTable2)
        {
            if (footprintsTable1.Length != footprintsTable2.Length)
            {
                throw new Exception("Both tables has to have the same dimensions!");
            }
            int counter = 0;
            for (int i = 0; i < footprintsTable1.GetLength(0); i++)
            {
                for (int j = 0; j < footprintsTable2.GetLength(0); j++)
                {
                    if (footprintsTable1[i,j] != footprintsTable2[i,j])
                    {
                        counter++;
                    }
                }
            }
            decimal fitness = 1 - ((decimal)counter / footprintsTable1.Length);
            return fitness;
        }
    }
}
