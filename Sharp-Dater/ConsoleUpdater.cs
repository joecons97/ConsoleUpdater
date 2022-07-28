using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
    public class ProgressBarInfo
    {
        public ConsoleColor IncompleteColor { get; set; } = ConsoleColor.DarkGray;
        public ConsoleColor CompleteColor { get; set; } = ConsoleColor.Green;
        public char IncompleteChar { get; set; } = '-';
        public char CompleteChar { get; set; } = '-';
        public bool DisplayPercent { get; set; } = false;
        public bool DisplaySpeed { get; set; } = false;

        public static readonly ProgressBarInfo Default = new ProgressBarInfo()
        {
            IncompleteChar = '-',
            IncompleteColor = ConsoleColor.DarkGray,
            CompleteChar = '-',
            CompleteColor = ConsoleColor.Green,
            DisplayPercent = false,
            DisplaySpeed = false
        };
    }

    public class ConsoleUpdater
    {
        string domain;
        string versionFilePath;
        string appFilePath;
        Version version;
        ProgressBarInfo progressbarInfo;

        public static ConsoleUpdater Create()
        {
            var sharpDater = new ConsoleUpdater();
            sharpDater.progressbarInfo = ProgressBarInfo.Default;
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

        public ConsoleUpdater WithProgressBarInfo(ProgressBarInfo progressBarInfo)
        {
            this.progressbarInfo = progressBarInfo;
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
                float speed = ((float)list.Count / (float)sw.Elapsed.TotalSeconds)/1024f/1024f;
                var perc = (list.Count / (double)response.ContentLength) * 100;
                DrawProgress((int)perc, speed);
            } while (bytesRead > 0);

            //Move to next line
            Console.WriteLine("");

            sw.Stop();
            var tmpPath = Path.Combine(Path.GetTempPath(), $"{DateTime.Now.ToFileTime()}");
            if (!Directory.Exists(tmpPath))
                Directory.CreateDirectory(tmpPath);

            Console.WriteLine($"Extracting files...");
            using (ZipArchive zip = new ZipArchive(new MemoryStream(list.ToArray())))
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

        void DrawProgress(int perc, float speed)
        {
            string extraText = "";
            if (progressbarInfo.DisplayPercent)
                extraText += " " + perc.ToString("f0") + "%";

            if (progressbarInfo.DisplaySpeed)
                extraText += " " + speed.ToString("f2") + "MB/s";

            Console.CursorVisible = false;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"[");
            float width = Console.WindowWidth - 2 - extraText.Length;
            int p = (int)((perc / 100f) * width);
            for (int j = 0; j < width; j++)
            {
                if (j >= p)
                {
                    Console.ForegroundColor = progressbarInfo.IncompleteColor;
                    Console.Write(progressbarInfo.IncompleteChar);
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = progressbarInfo.CompleteColor;
                    Console.Write(progressbarInfo.CompleteChar);
                    Console.ResetColor();
                }
            }
            Console.Write("]" + extraText);

            Console.ResetColor();
            Console.CursorVisible = true;
        }
    }
}
