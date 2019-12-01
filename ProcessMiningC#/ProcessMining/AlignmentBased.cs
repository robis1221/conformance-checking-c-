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
    public static double TestAlignment(Dictionary<List<string>, int> trace_frequencies, 
        Dictionary<List<string>, int> trace_frequencies_noisy)
    {        
        var minedNet = AlphaMiner.mine(trace_frequencies);
        var completelyFits = true;

        AlignmentBased res = new AlignmentBased();
        
        foreach (KeyValuePair<List<string>, int> trace in trace_frequencies_noisy)
        {
            string alignment = "";
            res.traceCount += 1;
            bool match = true;            
            // Console.WriteLine(" \n");

            minedNet.InitializeTokens();
            minedNet.AddMarking(1);
            foreach (var step in trace.Key)
            {
                
              // Console.Write(step+"-");
                var transitionId = minedNet.TransitionNameToId(step);
                var isEnabled = minedNet.IsEnabled(transitionId);
                if (!isEnabled)
                {
                    match = false;
                 //  Console.WriteLine(step);
                    FindOptimalAlignment(trace,minedNet,transitionId,alignment);
                    completelyFits = false;
                    break;
                }
                else
                {
                    alignment += step+" - ";
                }
                minedNet.FireTransition(transitionId);
            }
            if (match == true)
            {
                res.totalFitness += 1;
            }   
        }
        
        if (completelyFits)
        {
            Console.WriteLine("Fitness: " + res.totalFitness / res.traceCount);
            return (double)res.totalFitness / res.traceCount;
        }
        else
        {
            return -1;
        }
    }
    static void FindOptimalAlignment(KeyValuePair<List<string>,int> trace,PetriNet minedNet,int transitionId,string alignment)
    {
        var optimal = minedNet.GetValidShortestPath(transitionId, minedNet.GetSinkPlace());
        for(int i = 0; i < optimal.Count; i++)
        {
          //  Console.WriteLine(minedNet.TransitionIdToName(optimal[i]));
            alignment = alignment + minedNet.TransitionIdToName(optimal[i])+" - ";
        }
        var traceString = "";
        foreach(var step in trace.Key)
        {
            traceString = traceString + step + " - ";
        }
        Console.WriteLine("Deviating trace: "+traceString.Substring(0, traceString.Length-3));
        Console.WriteLine("Optimal alignment: "+alignment.Substring(0, alignment.Length-3));
        //Console.WriteLine(trace);
        //Console.WriteLine("Cost of the optimal alignment: ");

    }
}