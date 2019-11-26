using ProcessMining;
using System;
using System.Collections.Generic;

public class AlignmentBased
{
    double totalFitness = 0;
    int traceCount = 0;
    public AlignmentBased()
    {
    }
    public double TestAlignment()
    {
        Dictionary<List<string>, int> trace_frequencies = FileParser.ParseXES(
              "D:\\DTU\\conformance-checking-c-\\ProcessMiningC#\\ProcessMining\\extension-log.xes");
        Dictionary<List<string>, int> trace_frequencies_noisy =
            FileParser.ParseXES(
                "D:\\DTU\\conformance-checking-c-\\ProcessMiningC#\\ProcessMining\\extension-log-noisy.xes");
        var minedNet = AlphaMiner.mine(trace_frequencies);

        AlignmentBased res = new AlignmentBased();
        foreach (KeyValuePair<List<string>, int> trace in trace_frequencies)
        {
        res.traceCount += 1;
            bool match = true;
            Console.WriteLine(" \n");
            minedNet.InitializeTokens();
            minedNet.AddMarking(1);
            foreach (var step in trace.Key)
            {
                
                Console.Write(step+"-");
                var transitionId = minedNet.TransitionNameToId(step);
                var isEnabled = minedNet.IsEnabled(transitionId);
                if (!isEnabled)
                {
                    match = false;
                   // Console.WriteLine(step);
                    FindOptimalAlignment(trace,minedNet);
                    break;
                }
                minedNet.FireTransition(transitionId);
            }
            if (match == true)
            {
                res.totalFitness += 1;
            }   
        }
        Console.WriteLine(res.totalFitness / res.traceCount);
        return (double) res.totalFitness / res.traceCount;
    }
    void FindOptimalAlignment(KeyValuePair<List<string>,int> trace,PetriNet minedNet)
    {
        minedNet.InitializeTokens();
        minedNet.AddMarking(1);


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