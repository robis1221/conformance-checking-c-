using ProcessMining;
using System;
using System.Collections.Generic;

public class AlignmentBased
{
    public AlignmentBased()
    {
    }
    public void TestAlignment()
    {
        Dictionary<List<string>, int> trace_frequencies = FileParser.ParseXES(
              "D:\\DTU\\conformance-checking-c-\\ProcessMiningC#\\ProcessMining\\extension-log.xes");
        Dictionary<List<string>, int> trace_frequencies_noisy =
            FileParser.ParseXES(
                "D:\\DTU\\conformance-checking-c-\\ProcessMiningC#\\ProcessMining\\extension-log-noisy.xes");
        var minedNet = AlphaMiner.mine(trace_frequencies);
        foreach (KeyValuePair<List<string>, int> trace in trace_frequencies_noisy)
        {
            Console.WriteLine(" \n");
            minedNet.InitializeTokens();
            foreach (var step in trace.Key)
            {
                
                Console.Write(step+"-");
                var transitionId = minedNet.TransitionNameToId(step);
                var isEnabled = minedNet.IsEnabled(transitionId);
                if (!isEnabled)
                {
                   // Console.WriteLine(step);
                    FindOptimalAlignment(trace,minedNet);
                    break;
                }
                minedNet.FireTransition(transitionId);
            }
            
        }
    }
    void FindOptimalAlignment(KeyValuePair<List<string>,int> trace,PetriNet minedNet)
    {
        minedNet.InitializeTokens();
      

    }
    void ShortestPath(Node start, Node end)
    {
        //A* shortest path based on the standard cost function
    }
    void GetCost(KeyValuePair<List<string>,int> log_trace, KeyValuePair<List<string>,int> model_trace)
    {
        //Return cost between a log trace and a model trace
    }
}