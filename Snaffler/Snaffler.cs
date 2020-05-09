using System;

namespace Snaffler
{
    internal static class Snaffler
    {
        private static void Main(string[] args)
        {
            var runner = new SnaffleRunner();
            runner.Run(args);
            Console.ReadKey();
        }
    }
}