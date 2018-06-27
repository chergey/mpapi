using System;
using MPAPI;

namespace DistributedPrimeCalculatorApp
{
    class Program
    {
        static void Main(string[] args)
        {
            using (Node node = new Node())
            {
                node.OpenLocal<MainWorker>();

                /* Since the node spawns new workers in separate threads, and as
                 * background threads, we have to prevent the main thread from
                 * terminating here. */
                Console.ReadLine();
            }
        }
    }
}