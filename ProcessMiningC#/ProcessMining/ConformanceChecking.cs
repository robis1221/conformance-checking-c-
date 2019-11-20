using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining
{
    class ConformanceChecking
    {
        private class TokenReplayInfo
        {
            public int produced { get; set; }
            public int consumed { get; set; }
            public int missed { get; set; }
            public int remained { get; set; }

            public TokenReplayInfo(int produced = 0, int consumed = 0, int missed = 0, int remained = 0)
            {
                this.produced = produced;
                this.consumed = consumed;
                this.missed = missed;
                this.remained = remained;
            }

            public void MultiplyBy(int n)
            {
                this.produced *= n;
                this.consumed *= n;
                this.missed *= n;
                this.remained *= n;
            }

            public void SumUp(TokenReplayInfo other)
            {
                this.produced += other.produced;
                this.consumed += other.consumed;
                this.missed += other.missed;
                this.remained += other.remained;
            }

            public double ComputeFitness()
            {
                return (double)(0.5 * (1 - ((double)this.missed / (double)this.consumed)) + 0.5 * (1 - ((double)this.remained / (double)this.produced)));
            }
        }

        public static double TokenReplayFitness(Dictionary<List<string>, int> trace_frequencies, PetriNet minedNet)
        {
            var result = new TokenReplayInfo();
            foreach (KeyValuePair<List<string>, int> trace in trace_frequencies)
            {
                minedNet.InitializeTokens();
                var sequenceTokenInfo = new TokenReplayInfo(produced: 1);
                foreach (var step in trace.Key)
                {
                    var transitionId = minedNet.TransitionNameToId(step);
                    var isEnabled = minedNet.IsEnabled(transitionId);

                    if (!isEnabled)
                    {
                        //  produce the correct amount of missing tokens
                        var placesBefore = minedNet.GetIdsOfParentPlaces(transitionId);
                        foreach (var placeId in placesBefore)
                        {
                            if (0 == minedNet.GetTokens(placeId))
                            {
                                minedNet.AddMarking(placeId);
                                sequenceTokenInfo.missed++;
                            }
                        }
                    }

                    sequenceTokenInfo.consumed += minedNet.GetNumberOfParents(transitionId);
                    sequenceTokenInfo.produced += minedNet.GetNumberOfChildren(transitionId);
                    minedNet.FireTransition(transitionId);
                }

                var lastTransitionId = minedNet.TransitionNameToId(trace.Key[trace.Key.Count - 1]);
                if (1 != minedNet.GetNumberOfChildren(lastTransitionId))
                {
                    Console.WriteLine("Error, the last transition should have only one out place");
                    return -1;
                }

                // consume the tokens from the out place
                sequenceTokenInfo.consumed += minedNet.GetTokens(minedNet.GetIdsOfChildPlaces(lastTransitionId)[0]);

                // check if there are any tokens left behind in the net
                foreach (var step in trace.Key)
                {
                    var transitionId = minedNet.TransitionNameToId(step);

                    sequenceTokenInfo.remained += minedNet.GetIdsOfParentPlaces(transitionId)
                        .Select(x => minedNet.GetTokens(x)).ToList().Sum();
                }
                sequenceTokenInfo.MultiplyBy(trace.Value);
                result.SumUp(sequenceTokenInfo);

            }
            return result.ComputeFitness();
        }

        public static void TestConformanceChecking()
        {
            var log = FileParser.ParseXES(
                "C:\\Users\\Nikolay Dobrev\\source\\repos\\ProcessMining\\ProcessMining\\extension-log.xes");
            var log_noisy =
                FileParser.ParseXES(
                    "C:\\Users\\Nikolay Dobrev\\source\\repos\\ProcessMining\\ProcessMining\\extension-log-noisy.xes");

            var minedModel = AlphaMiner.mine(log);
            Console.WriteLine(ConformanceChecking.TokenReplayFitness(log,minedModel));
            Console.WriteLine(ConformanceChecking.TokenReplayFitness(log_noisy, minedModel));
            
        }
    }
}
