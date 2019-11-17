using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Combinatorics.Collections;

namespace ProcessMining
{
    class AlphaMiner
    {
        private static List<HashSet<string>> getSets(Dictionary<List<string>, int> trace_frequencies)
        {
            HashSet<string> transitionSet = new HashSet<string>();
            HashSet<string> startTransitions = new HashSet<string>();
            HashSet<string> endTransitions = new HashSet<string>();
            foreach (KeyValuePair<List<string>,int> trace in trace_frequencies)
            {
                for (var i = 0; i < trace.Key.Count; i++)
                {
                    if (0 == i)
                    {
                        startTransitions.Add(trace.Key[i]);
                    }
                    else if (trace.Key.Count - 1 == i)
                    {
                        endTransitions.Add(trace.Key[i]);
                    }

                    transitionSet.Add(trace.Key[i]);
                }
            }

            return new List<HashSet<string>>(){startTransitions,transitionSet,endTransitions};
        }

        private static HashSet<Tuple<string, string>> GetDirectSuccessions(Dictionary<List<string>, int> trace_frequencies)
        {
            HashSet<Tuple<string, string>> directSuccession = new HashSet<Tuple<string, string>>();
            foreach (KeyValuePair<List<string>, int> trace in trace_frequencies)
            {
                for (var i = 0; i < trace.Key.Count - 1; i++)
                {
                    var x = trace.Key[i];
                    var y = trace.Key[i + 1];
                    directSuccession.Add(new Tuple<string, string>(x,y));
                }
            }

            return directSuccession;
        }

        private static HashSet<Tuple<string, string>> GetCausalities(Dictionary<List<string>, int> trace_frequencies,
            HashSet<Tuple<string, string>> directSucession)
        {
            HashSet<Tuple<string, string>> causalities = new HashSet<Tuple<string, string>>(directSucession);
            foreach (KeyValuePair<List<string>, int> trace in trace_frequencies)
            {
                for (var i = 0; i < trace.Key.Count - 1; i++)
                {
                    var x = trace.Key[i];
                    var y = trace.Key[i + 1];
                    if (causalities.Contains(new Tuple<string, string>(y,x)))
                    {
                        causalities.Remove(new Tuple<string, string>(x, y));
                    }
                }
            }

            return causalities;
        }

        private static HashSet<Tuple<string, string>> GetParallels(HashSet<Tuple<string, string>> directSucession)
        {
            HashSet<Tuple<string,string>> parallels = new HashSet<Tuple<string, string>>();
            foreach (var element in directSucession)
            {
                var x = element.Item1;
                var y = element.Item2;
                if (directSucession.Contains(new Tuple<string, string>(y, x)))
                {
                    parallels.Add(new Tuple<string, string>(x, y));
                }
            }

            return parallels;
        }

        private static HashSet<Tuple<string, string>> GetNotConnected(HashSet<string> transitionSet,
            HashSet<Tuple<string, string>> directSucessions)
        {
            HashSet<Tuple<string, string>> choices = new HashSet<Tuple<string, string>>();
            foreach(string x in transitionSet)
            {
                foreach (string y in transitionSet)
                {
                    if (!directSucessions.Contains(new Tuple<string, string>(x, y)) &&
                          !directSucessions.Contains(new Tuple<string, string>(y, x)))
                    {
                        choices.Add(new Tuple<string, string>(x, y));
                    }
                }
            }

            return choices;
        }

        private static bool check_set(List<string> set, HashSet<Tuple<string, string>> notConnected)
        {
            if (null == notConnected || 0 == notConnected.Count)
            {
                return false;
            }
            else
            {
                foreach (var event1 in set)
                {
                    foreach (var event2 in set)
                    {
                        if (!notConnected.Contains(new Tuple<string, string>(event1, event2)))
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        private static bool check_outset(List<string> list, List<string> list1, HashSet<Tuple<string, string>> notConnected)
        {
            foreach (var event1 in list)
            {
                foreach (var event2 in list1)
                {
                    if (!(notConnected.Contains(new Tuple<string, string>(event1,event2))))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static HashSet<Tuple<List<string>, List<string>>> MakeXLSet(HashSet<string> transitionSet, 
            HashSet<Tuple<string, string>> directSuccession, HashSet<Tuple<string, string>> causalities, 
            HashSet<Tuple<string, string>> notConnected)
        {
            HashSet<Tuple<List<string>, List<string>>> xl = new HashSet<Tuple<List<string>, List<string>>>();
            HashSet<List<string>> subsets = new HashSet<List<string>>();

            for (int i = 1; i < transitionSet.Count; i++)
            {
                var comb = new Combinations<string>(transitionSet.ToList(), i);
                foreach (var c in comb)
                {
                    subsets.Add(c.ToList());
                }
            }

            foreach (var a in subsets)
            {
                var return_a = check_set(a, notConnected);
                foreach (var b in subsets)
                {
                    var return_b = check_set(b, notConnected);
                    if (return_a && return_b && check_outset(a, b, causalities))
                    {
                        xl.Add(new Tuple<List<string>, List<string>>(a, b));
                    }
                }
            }

            return xl;
        }

        private static HashSet<Tuple<List<string>, List<string>>> MakeYLSet(HashSet<Tuple<List<string>, List<string>>> xl)
        {
            HashSet<Tuple<List<string>, List<string>>> yl = new HashSet<Tuple<List<string>, List<string>>>(xl);
            foreach (var a in xl)
            {
                var A = a.Item1;
                var B = a.Item2;
                foreach (var b in xl)
                {
                    bool results_a = A.All(i => b.Item1.Contains(i));
                    bool results_b = B.All(i => b.Item2.Contains(i));
                    if (results_b && results_a && !(a==b))
                    {
                        yl.Remove(a);
                    }
                }
            }

            return yl;
        }


        public static PetriNet mine(Dictionary<List<string>, int> trace_frequencies)
        {
            var sets = getSets(trace_frequencies);
            var startSet = sets[0];
            var transitionsSet = sets[1];
            var endSet = sets[2];

            var directSuccessions = GetDirectSuccessions(trace_frequencies);
            var causalities = GetCausalities(trace_frequencies, directSuccessions);
            // Check not_connected should return 48
            var notConnected = GetNotConnected(transitionsSet, directSuccessions);

            var xl = MakeXLSet(transitionsSet, directSuccessions, causalities, notConnected);
            var yl = MakeYLSet(xl);

            // Fill the petri net
            var p = new PetriNet();

            int placeId = 1;
            int transitionId = -1;
            Dictionary<Tuple<List<string>, List<string>>, int> placeDictionary = new Dictionary<Tuple<List<string>, List<string>>, int>();
            
            p.AddPlace(placeId);
            placeId++;

            // adding all transitions
            foreach (var transition in transitionsSet)
            {
                p.AddTransition(transitionId,transition);
                transitionId--;
            }

            // adding all middle transitions
            foreach (var place in yl)
            {
                p.AddPlace(placeId);
                placeDictionary.Add(place,placeId);
                placeId++;
            }

            // adding the "out" place
            p.AddPlace(placeId);

            // Adding the connection between in place to all start transitions
            foreach (var start in startSet)
            {
                p.AddEdge(1,p.TransitionNameToId(start));
            }

            // Adding the connection between all ending transitions to out place
            foreach (var end in endSet)
            {
                p.AddEdge(p.TransitionNameToId(end),placeId);
            }

            foreach (var place in yl)
            {
                placeId = placeDictionary[place];
                foreach (var transition in place.Item1)
                {
                    p.AddEdge(p.TransitionNameToId(transition),placeId);
                }

                foreach (var transition in place.Item2)
                {
                    p.AddEdge(placeId,p.TransitionNameToId(transition));
                }
            }

            // should I add the marking
            p.AddMarking(1);

            return p;
        }


        private static void check_enabled(PetriNet p)
        {
            List<string> transitionList = new List<string>(){ "record issue", "inspection",
                "intervention authorization", "action not required", "work mandate",
                "no concession", "work completion", "issue completion" };

            foreach (var t in transitionList)
            {
                Console.Write(p.IsEnabled(p.TransitionNameToId(t)).ToString()+ " ");
            }
            Console.WriteLine();
        }

        public static void test_alpha_miner(string pathToLog)
        {
            var minedModel = AlphaMiner.mine(FileParser.ParseXES(pathToLog));



            List<string> trace =new List<string>(){"record issue", "inspection", "intervention authorization", "work mandate", "work completion", "issue completion"};

            var cost = minedModel.GetShortestPath(minedModel.TransitionNameToId("inspection"),
                minedModel.TransitionNameToId("issue completion"), null);

            Console.WriteLine(cost);

            foreach (var a in trace)
            {
                check_enabled(minedModel);
                minedModel.FireTransition(minedModel.TransitionNameToId(a));
            }

        }

    }
}
