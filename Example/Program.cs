using System;
using System.IO;
using System.Reflection;
using System.Threading;
using ConsoleUpdater;

namespace Example
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var updater = ConsoleUpdater.ConsoleUpdater.Create().
                WithDomain(@"https://yourserver.co.uk/ConsoleUpdater/").
                WithVersionFile("ver.txt").
                WithAppFile("app.zip").
                WithProgressBarInfo(new ProgressBarInfo() { DisplayPercent = true, DisplaySpeed = true, CompleteColor = ConsoleColor.Yellow });

            if (updater.CheckForUpdate(out var v))
                Console.Read();
            else
                Console.WriteLine($"No update ({v})");
        }
    }
}
