using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RouteNavigation.Logging
{
    public static class Logger
    {
        static string logFilePath = System.Configuration.ConfigurationManager.AppSettings["logFilePath"];
        static object logLocker = new Object();

        public static async void LogMessage(string line, string logLevel = "ERROR")
        {
            int configLogLevel = 0;
            if (Config.logLevel == "DEBUG")
            {
                configLogLevel = 3;
            }
            else if (Config.logLevel == "INFO")
            {
                configLogLevel = 2;
            }
            else if (Config.logLevel == "WARNING")
            {
                configLogLevel = 1;
            }
            else if (Config.logLevel == "ERROR")
            {
                configLogLevel = 0;
            }

            int messageLogLevel = 0;
            if (logLevel == "DEBUG")
            {
                messageLogLevel = 3;
            }
            else if (logLevel == "INFO")
            {
                messageLogLevel = 2;
            }
            else if (logLevel == "WARNING")
            {
                messageLogLevel = 1;
            }
            else if (logLevel == "ERROR")
            {
                messageLogLevel = 0;
            }

            if (messageLogLevel <= configLogLevel)
            {
                try
                {
                    string date = DateTime.Now.ToString("yyyy:MM:dd hh:mm:ss");

                    string l = date + " | " + line + "\r\n";
                    await WriteTextAsync(logFilePath, l);
                }
                catch
                {
                }
            }
        }
        private static async Task WriteTextAsync(string filePath, string text)
        {
            try
            {
                byte[] encodedText = Encoding.Unicode.GetBytes(text);

                // await task, but also lock the code around the using file stream.  This is because high concurrency log appends will otherwise block.

                await Task.Run(() =>
                {
                    lock (logLocker)
                    {
                        using (FileStream sourceStream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read, bufferSize: 4096, useAsync: true))
                        {
                            sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                        }
                    };
                });
            }
            catch
            {
            }
        }
    };
}