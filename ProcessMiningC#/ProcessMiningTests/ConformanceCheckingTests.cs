using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProcessMining;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining.Tests
{
    [TestClass()]
    public class ConformanceCheckingTests
    {
        private static Dictionary<List<string>, int> log = FileParser.ParseXES(
                @"C:\ProcessMining\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log.xes");
        private static Dictionary<List<string>, int> log_noisy =
                FileParser.ParseXES(
                    @"C:\ProcessMining\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log-noisy.xes");
        [TestMethod()]
        public void CalculateCasualFootprintFitnessWithSameLogTest()
        {
            // Arrange 
            PetriNet minedModel = AlphaMiner.mine(log);

            // Act
            var casualFitness = CasualFootprints.CalculateCasualFootprintFitness(minedModel, log);

            // Assert
            Assert.AreEqual(casualFitness, 1);
        }

        [TestMethod()]
        public void CalculateCasualFootprintFitnessWithNoisyLogTest()
        {
            // Arrange 
            PetriNet minedModel = AlphaMiner.mine(log);

            // Act
            var casualFitness = CasualFootprints.CalculateCasualFootprintFitness(minedModel, log_noisy);

            // Assert
            Assert.AreNotEqual(casualFitness, 1);
        }

        [TestMethod()]
        public void CalculateTokenReplayFitnessWithSameLogTest()
        {
            // Arrange 
            PetriNet minedModel = AlphaMiner.mine(log);

            // Act
            var casualFitness = ConformanceChecking.TokenReplayFitness(minedModel, log);

            // Assert
            Assert.AreEqual(casualFitness, 1);
        }
        
        [TestMethod()]
        public void CalculateTokenReplayFitnessWithNoisyLogTest()
        {
            // Arrange 
            PetriNet minedModel = AlphaMiner.mine(log);

            // Act
            var casualFitness = ConformanceChecking.TokenReplayFitness(minedModel, log);

            // Assert
            Assert.AreEqual(casualFitness, 1);
        }

        [TestMethod()]
        public void CalculateAligningObservedFitnessWithSameLogTest()
        {
            // Arrange 

            // Act
            var fitness = AlignmentBased.TestAlignment(log, log);

            // Assert
            Assert.AreEqual(1, fitness);
        }

        [TestMethod()]
        public void CalculateAligningObservedFitnessWithNoisyLogTest()
        {
            // Arrange 

            // Act
            var fitness = AlignmentBased.TestAlignment(log, log_noisy);

            // Assert
            Assert.AreEqual(-1, fitness);
        }
    }
}