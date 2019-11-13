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
        static void Main(string[] args)
        {
            string logPath =
                "C:\\Users\\Nikolay Dobrev\\source\\repos\\ProcessMining\\ProcessMining\\extension-log.xes";
 

            //AlphaMiner.test_alpha_miner(logPath);
            ConformanceChecking.TestConformanceChecking();

        }
    }
}
