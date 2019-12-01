using ProcessMining;
using System;
using System.Collections.Generic;

public class AlignmentBased
{
    double totalFitness = 0;
    int traceCount = 0;
    bool completelyFits = false;
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
                    FindOptimalAlignment(trace,minedNet,transitionId);
                    completelyFits = false;
                    break;
                }
                minedNet.FireTransition(transitionId);
            }
            if (match == true)
            {
                res.totalFitness += 1;
            }   
        }
        //    Console.WriteLine(res.totalFitness / res.traceCount);
        if (completelyFits)
        {
            return (double)res.totalFitness / res.traceCount;
        }
        else
        {
            return -1;
        }
    }
    void FindOptimalAlignment(KeyValuePair<List<string>,int> trace,PetriNet minedNet,int transitionId)
    {
        Console.WriteLine("Optimal alignment: ");
        Console.WriteLine("Cost of the optimal alignment: "+minedNet.GetValidShortestPath(transitionId, minedNet.GetSinkPlace()));

    }
}