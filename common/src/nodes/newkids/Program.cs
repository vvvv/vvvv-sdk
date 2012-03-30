using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Nodes.Animation;

namespace VVVV.Nodes
{
    class Program
    {
        static void Main(string[] args)
        {
            // testing change nodes
            var ftor = new ChangedState<string>();
            Console.WriteLine(ftor.Changed("hu"));
            Console.WriteLine(ftor.Changed("hu"));

            var ftor2 = new ChangedState<double>();
            Console.WriteLine(ftor2.Changed(0));
            Console.WriteLine(ftor2.Changed(0));


            // testing toggle
            var bftor = new ToggleState();
            Console.WriteLine(bftor.Toggle(true));
            Console.WriteLine(bftor.Toggle(true));


            // testing filternodes
            var fftor = new LinearFilterState();
            var goal = 10;
            var time = 5.0;

            ConsoleKey c = ConsoleKey.Spacebar;
            while (c != ConsoleKey.Escape)
            {
                //Console.WriteLine(fftor.LinearFilter(goal, time));

                if (Console.KeyAvailable)
                {
                    c = Console.ReadKey(false).Key;
                    //goal = new Random().NextDouble() * 100;
                    time = new Random().NextDouble() * 5;
                }
            }
        }
    }
}
