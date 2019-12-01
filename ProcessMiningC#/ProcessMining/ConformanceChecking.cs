using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Token replay technique
        /// </summary>
        /// <param name="minedNet">Mined model</param>
        /// <param name="trace_frequencies">Dictionary holding the frequencies from the trance</param>
        /// <returns>Fitness E [0;1]</returns>
        public static double TokenReplayFitness(PetriNet minedNet, Dictionary<List<string>, int> trace_frequencies)
        {
            var result = new TokenReplayInfo();
            foreach (KeyValuePair<List<string>, int> trace in trace_frequencies)
            {
                minedNet.InitializeTokens();
                minedNet.AddMarking(1);
                var sequenceTokenInfo = new TokenReplayInfo(produced: 1);
                foreach (var step in trace.Key)
                {
                    var transitionId = minedNet.TransitionNameToId(step);
                    var isEnabled = minedNet.IsEnabled(transitionId);

                    if (!isEnabled)
                    {
                        // produce the correct amount of missing tokens
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

        /// <summary>
        /// Token replay technique
        /// </summary>
        /// <param name="minedNet">Mined model</param>
        /// <param name="pathToLogFile">path to the log file</param>
        /// <returns>Fitness E [0;1]</returns>
        public static double TokenReplayFitness(PetriNet minedNet, string pathToLogFile)
        {
            Dictionary<List<string>, int> log = FileParser.ParseXES(pathToLogFile);
            return TokenReplayFitness(minedNet, log);
        }

        /// <summary>
        /// Token replay technique
        /// </summary>
        /// <param name="pathToLogToBeMined"></param>
        /// <param name="pathToLogFile">path to the log file</param>
        /// <returns>Fitness E [0;1]</returns>
        public static double TokenReplayFitness(string pathToLogToBeMined, string pathToLogFile)
        {
            Dictionary<List<string>, int> log = FileParser.ParseXES(pathToLogToBeMined);
            Dictionary<List<string>, int> logNoisy = FileParser.ParseXES(pathToLogFile);

            PetriNet minedModel = AlphaMiner.mine(log);

            return (double)TokenReplayFitness(minedModel, logNoisy);
        }

        /// <summary>
        /// Interface for calculating Casual Footprint fitness
        /// </summary>
        /// <param name="net">Petri net representing the model</param>
        /// <param name="logDict">Sequence dictionary of the log</param>
        /// <returns>fitness E [0,1]</returns>
        public static double CalculateCasualFootprintFitness(PetriNet net, Dictionary<List<string>, int> logDict)
        {
            return (double)CasualFootprints.CalculateCasualFootprintFitness(net, logDict);
        }

        /// <summary>
        /// Interface for calculating Casual Footprint fitness
        /// </summary>
        /// <param name="net">Petri net representing the model</param>
        /// <param name="pathToLogFile"></param>
        /// <returns>fitness E [0,1]</returns>
        public static double CalculateCasualFootprintFitness(PetriNet net, string pathToLogFile)
        {
            Dictionary<List<string>, int> log = FileParser.ParseXES(pathToLogFile);
            return (double)CasualFootprints.CalculateCasualFootprintFitness(net, log);
        }

        /// <summary>
        /// Interface for calculating Casual Footprint fitness
        /// </summary>
        /// <param name="pathToLogToBeMined">Path to the log file, from which we will mine the model using Petri net</param>
        /// <param name="pathToLogFile"></param>
        /// <returns>fitness E [0,1]</returns>
        public static double CalculateCasualFootprintFitness(string pathToLogToBeMined, string pathToLogFile)
        {
            Dictionary<List<string>, int> log = FileParser.ParseXES(pathToLogToBeMined);
            Dictionary<List<string>, int> logNoisy = FileParser.ParseXES(pathToLogFile);

            PetriNet minedModel = AlphaMiner.mine(log);

            return (double)CasualFootprints.CalculateCasualFootprintFitness(minedModel, logNoisy);
        }


        /// <summary>
        /// Test
        /// </summary>
        public static void TestConformanceChecking()
        {
            Dictionary<List<string>, int> log = FileParser.ParseXES(
                @"D:\New folder\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log.xes");
            Dictionary<List<string>, int> log_noisy =
                FileParser.ParseXES(
                    @"D:\New folder\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log-noisy.xes");

            PetriNet minedModel = AlphaMiner.mine(log);
            
            var casualFitness = CasualFootprints.CalculateCasualFootprintFitness(minedModel, log);
            var casualFitnessNoisy = CasualFootprints.CalculateCasualFootprintFitness(minedModel, log_noisy);

            Console.WriteLine($"CasualFootprint fitness of the same logs and model: {casualFitness}");
            Console.WriteLine($"CasualFootprint fitness of the model and noisy logs: {casualFitnessNoisy}");

            Console.WriteLine(ConformanceChecking.TokenReplayFitness(minedModel,log));
            Console.WriteLine(ConformanceChecking.TokenReplayFitness(minedModel,log_noisy));

            Console.ReadLine();
            
        }
    }
}
