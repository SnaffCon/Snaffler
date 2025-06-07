using System;
using System.Threading.Tasks;

namespace Snaffler
{
    public static class Snaffler
    {
        public static async Task Main(string[] args)
        {
            SnaffleRunner runner = new SnaffleRunner();
            await runner.RunAsync(args);
            Console.WriteLine("I snaffled 'til the snafflin was done.");
        }
    }
}