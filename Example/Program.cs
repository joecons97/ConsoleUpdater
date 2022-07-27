using System;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Example
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var updater = ConsoleUpdater.ConsoleUpdater.Create().
                WithDomain(@"C:\Test\").
                WithVersionFile("ver.txt").
                WithAppFile("net5.0-windows.zip");

            if (updater.CheckForUpdate(out var v))
                Console.Read();
            else
                Console.WriteLine($"No update ({v})");
        }
    }
}
