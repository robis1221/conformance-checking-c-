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
    public class AlphaMinerTests
    {
        [TestMethod()]
        public void mineTest()
        {
            // Arrange
            Dictionary<List<string>, int> log = FileParser.ParseXES(
                @"C:\ProcessMining\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log.xes");

            // Act
            PetriNet minedModel = AlphaMiner.mine(log);

            // Assert
            Assert.IsNotNull(minedModel);
            Assert.AreEqual(8, minedModel.Transitions.Count);
            Assert.AreEqual(8, minedModel.TransitionsNameToId.Count);
        }
    }
}