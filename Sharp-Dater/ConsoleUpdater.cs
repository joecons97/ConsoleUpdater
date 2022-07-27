using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ConsoleUpdater
{
    public class ConsoleUpdater
    {
        public string domain;
        public string versionFilePath;
        public string appFilePath;
        public Version version;

        public static ConsoleUpdater Create()
        {
            var sharpDater = new ConsoleUpdater();
            sharpDater.version = Assembly.GetEntryAssembly().GetName().Version;
            return sharpDater;
        }

        public ConsoleUpdater WithDomain(string domain)
        {
            this.domain = domain;
            return this;
        }

        public ConsoleUpdater WithVersionFile(string fullPath)
        {
            this.versionFilePath = fullPath;
            return this;
        }

        public ConsoleUpdater WithAppFile(string fullPath)
        {
            this.appFilePath = fullPath;
            return this;
        }

        public ConsoleUpdater WithCustomVersion(string version)
        {
            this.version = new Version(version);
            return this;
        }

        public bool CheckForUpdate(out Version newestVersion)
        {
            WebRequest request = WebRequest.Create(domain + versionFilePath);
            string responseStr = "";
            using (Stream stream = request.GetResponse().GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                responseStr = reader.ReadToEnd();
            }

            Version responseVersion = new Version(responseStr);

            bool update = responseVersion > version;
            if (update)
            {
                newestVersion = responseVersion;

                Console.WriteLine($"Update found... downloading... ({newestVersion})");
                DownloadAppFile();
            }
            else
            {
                newestVersion = version;
            }

            return update;
        }

        public async void DownloadAppFile()
        {
            WebRequest request = WebRequest.Create(domain + appFilePath);

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            int size = (int)(response.ContentLength / 100);
            Stopwatch sw = new Stopwatch();

            var list = new List<byte>();
            var bytes = new byte[size];
            var stream = response.GetResponseStream();

            sw.Start();
            int bytesRead = 0;
            do
            {
                bytesRead = await stream.ReadAsync(bytes, 0, size).ConfigureAwait(false);
                list.AddRange(bytes.Take(bytesRead));
                var perc = (list.Count / (double)response.ContentLength) * 100;
                DrawProgress((int)perc);
            } while (bytesRead > 0);

            sw.Stop();
            var tmpPath = Path.Combine(Path.GetTempPath(), $"{DateTime.Now.ToFileTime()}");
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            using(ZipArchive zip = new ZipArchive(new MemoryStream(list.ToArray())))
            {
                zip.ExtractToDirectory(tmpPath);
            }

            var installPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));
            response.Close();

            string cmd = $"/C timeout /t 1 & MOVE /Y {tmpPath}\\* {installPath}\\ & rmdir /Q/S {tmpPath}";

            ProcessStartInfo info = new ProcessStartInfo("cmd.exe", cmd);
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.UseShellExecute = true;

            Process.Start(info);

            Environment.Exit(-1);
        }

        void DrawProgress(int perc)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"[");
            float width = Console.WindowWidth - 5;
            int p = (int)((perc / 100f) * width);
            for (int j = 0; j < width; j++)
            {
                if (j >= p)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("─");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("─");
                    Console.ResetColor();
                }
            }
            Console.Write("]");

            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
