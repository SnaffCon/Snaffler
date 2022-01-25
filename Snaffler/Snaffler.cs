using System;

namespace Snaffler
{
    public static class Snaffler
    {
        public static void Main(string[] args)
        {
            SnaffleRunner runner = new SnaffleRunner();
            runner.Run(args);
        }
    }
}