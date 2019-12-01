using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessMining
{
    class Program
    {
        public static void TestPetriNetTest1()
        {
            PetriNet p = new PetriNet();

            p.AddPlace(1);
            p.AddPlace(2);
            p.AddPlace(3);
            p.AddPlace(4);
            p.AddTransition(-1,"A");
            p.AddTransition(-2,"B");
            p.AddTransition(-3,"C");
            p.AddTransition(-4,"D");

            p.AddEdge(1, -1);
            p.AddEdge(-1, 2);
            p.AddEdge(2, -2);
            p.AddEdge(-2, 3);
            p.AddEdge(2, -3);
            p.AddEdge(-3, 3);
            p.AddEdge(3, -4);
            p.AddEdge(-4, 4);

            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.AddMarking(1);  // add one token to place id 1
            Console.WriteLine(p.IsEnabled(-1).ToString() +" "+ p.IsEnabled(-2).ToString() +" "+ p.IsEnabled(-3).ToString() +" "+ p.IsEnabled(-4).ToString());

            p.FireTransition(-1);  // fire transition A
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.FireTransition(-3);  // fire transition C
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.FireTransition(-4);  // fire transition D
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.AddMarking(2);  // add one token to place id 2
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.FireTransition(-2);  // fire transition B
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            p.FireTransition(-4);  // fire transition D
            Console.WriteLine(p.IsEnabled(-1).ToString() + " " + p.IsEnabled(-2).ToString() + " " + p.IsEnabled(-3).ToString() + " " + p.IsEnabled(-4).ToString());

            Console.WriteLine(p.GetTokens(4));

        }

        static void TestShortestPathWithVeryComplexModel()
        {
            PetriNet p = new PetriNet();

            p.AddPlace(1);
            p.AddPlace(2);
            p.AddPlace(3);
            p.AddPlace(4);
            p.AddPlace(5);
            p.AddPlace(6);
            p.AddPlace(7);
            p.AddPlace(8);
            p.AddPlace(9);
            p.AddPlace(10);
            p.AddPlace(11);
            p.AddPlace(12);
            p.AddPlace(13);
            p.AddPlace(14);
            p.AddPlace(15);

            p.AddTransition(-1, "A");
            p.AddTransition(-2, "B");
            p.AddTransition(-3, "C");
            p.AddTransition(-4, "D");
            p.AddTransition(-5, "E");
            p.AddTransition(-6, "F");
            p.AddTransition(-7, "G");
            p.AddTransition(-8, "H");
            p.AddTransition(-9, "I");
            p.AddTransition(-10, "J");
            p.AddTransition(-11, "K");
            p.AddTransition(-12, "L");
            p.AddTransition(-13, "M");

            p.AddEdge(1, -1);

            p.AddEdge(-1, 2);
            p.AddEdge(-1, 3);
            p.AddEdge(-1, 4);

            p.AddEdge(2, -2);
            p.AddEdge(2, -3);
            p.AddEdge(3, -4);
            p.AddEdge(4, -5);

            p.AddEdge(-2, 5);
            p.AddEdge(-3, 6);
            p.AddEdge(-3, 7);
            p.AddEdge(-4, 8);
            p.AddEdge(-5, 9);

            p.AddEdge(5, -6);
            p.AddEdge(6, -7);
            p.AddEdge(7, -7);
            p.AddEdge(8, -9);
            p.AddEdge(9, -12);

            p.AddEdge(-9, 13);
            p.AddEdge(-12, 14);

            p.AddEdge(13, -8);
            p.AddEdge(14, -8);

            p.AddEdge(-6, 10);
            p.AddEdge(-7, 10);
            p.AddEdge(-8, 15);

            p.AddEdge(15,-13);

            p.AddEdge(-13, 11);

            p.AddEdge(10, -10);
            p.AddEdge(11, -10);

            p.AddEdge(-10, 12);
            

            p.AddMarking(2);
            p.AddMarking(3);
            p.AddMarking(4);

            var cost = p.GetValidShortestPath(-2, 12);
            for(int i = 0; i < cost.Count; i++)
            {
                Console.WriteLine(p.TransitionIdToName(cost[i]));
            }

        }

        static void TestShortestPath()
        {
            PetriNet p = new PetriNet();

            p.AddPlace(1);
            p.AddPlace(2);
            p.AddPlace(3);
            p.AddPlace(4);
            p.AddPlace(5);
            p.AddPlace(6);
            p.AddPlace(7);
            
            p.AddTransition(-1, "A");
            p.AddTransition(-2, "B");
            p.AddTransition(-3, "C");
            p.AddTransition(-4, "D");
            p.AddTransition(-5, "E");
            p.AddTransition(-6, "G");
            p.AddTransition(-7, "H");

            p.AddEdge(1, -1);
            p.AddEdge(-1, 2);
            p.AddEdge(-1, 3);

            p.AddEdge(2, -2);
            p.AddEdge(2, -3);

            p.AddEdge(3, -4);

            p.AddEdge(-2, 4);
            p.AddEdge(-3, 4);

            p.AddEdge(-4, 5);

            p.AddEdge(4, -5);
            p.AddEdge(5, -5);

            p.AddEdge(-5, 6);

            p.AddEdge(6, -6);
            p.AddEdge(6, -7);

            p.AddEdge(-6, 7);
            p.AddEdge(-7, 7);

            //Simulate marking
            p.AddMarking(2);
            p.AddMarking(3);
            

            var cost = p.GetValidShortestPath(-2, 7);


        }

        static void Main(string[] args)
        {
            string logPath =
                @"D:\New folder\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log.xes";
            string logPath_noisy =
                @"D:\New folder\conformance-checking-c-\ProcessMiningC#\ProcessMining\extension-log-noisy.xes";

            AlphaMiner.test_alpha_miner(logPath);
          //  ConformanceChecking.TestConformanceChecking();

                //"C:\\Users\\Nikolay Dobrev\\source\\repos\\ProcessMining\\ProcessMining\\extension-log.xes";



            //TestShortestPathWithVeryComplexModel();

            //AlphaMiner.test_alpha_miner(logPath);
            // ConformanceChecking.TestConformanceChecking();
            AlignmentBased ab = new AlignmentBased();
            ab.TestAlignment(logPath,logPath_noisy);
        }
    }
}
