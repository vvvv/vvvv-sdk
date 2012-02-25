using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Nodes
{
    class Program
    {
        static void Main(string[] args)
        {
            // testing change nodes
            var ftor = new ChangeDetector<string>();
            Console.WriteLine(ftor.IsChanged("hu"));
            Console.WriteLine(ftor.IsChanged("hu"));

            var ftor2 = new ChangeDetector<double>();            
            Console.WriteLine(ftor2.IsChanged(0));
            Console.WriteLine(ftor2.IsChanged(0));


            // testing toggle
            var bftor = new Toggle();
            Console.WriteLine(bftor.DoToggle(true));
            Console.WriteLine(bftor.DoToggle(true));


            // testing filternodes
            var fftor = new FilterNode();
            var goal = 10.0;
            var time = 5.0;
            
            ConsoleKey c = ConsoleKey.Spacebar;
            while (c != ConsoleKey.Escape)
            {
                Console.WriteLine(fftor.FilterAndSample(goal, time));
                
                if (Console.KeyAvailable)
                {
                    c = Console.ReadKey(false).Key;
                    goal = new Random().NextDouble() * 100;
                    time = new Random().NextDouble() * 5;
                }
            }
        }
    }
}
