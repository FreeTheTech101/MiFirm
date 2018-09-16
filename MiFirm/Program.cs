/*
 * Program Name:    MiFirm
 * Purpose:         Automatically extract firmware from the Mi Fit application
 * Author:          Alexander Georgiadis
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.ServiceModel.Syndication;
using System.Threading;
using System.Xml;

namespace MiFirm
{
    class Program
    {
        static void Main(string[] args)
        {
            // Setup our console enviroment
            Console.Title = "MiFirm";

            // Build our list of watches, and their required firmware files
            List<WatchModels> miWatches = new List<WatchModels>()
            {
                new WatchModels() { WatchName = "Mi Band (Model 1)", FirmwareFiles = { "Mili.fw" } },
                new WatchModels() { WatchName = "Mi Band (Model 1A)", FirmwareFiles = { "Mili_1a.fw" } },
                new WatchModels() { WatchName = "Mi Band (Model 1S)", FirmwareFiles = { "Mili_hr.fw" } },
                new WatchModels() { WatchName = "Mi Band 2", FirmwareFiles = { "Mili_pro.ft.en" } },
                new WatchModels() { WatchName = "Mi Band 3", FirmwareFiles = {"Mili_wuhan.fw", "Mili_wuhan.res" } },
                new WatchModels() { WatchName = "Mi Band 3 (NFC)", FirmwareFiles = { "Mili_chongqing.fw", "Mili_chongqing.res" } },
                new WatchModels() { WatchName = "Amazfit Bip", FirmwareFiles = { "Mili_chaohu.fw", "Mili_chaohu.res", "Mili_chaohu.gps" } },
                new WatchModels() { WatchName = "Amazfit Cor", FirmwareFiles = { "Mili_tempo.fw", "Mili_tempo.res" } }
            };

            // Give the user the option to specify a pre-downloaded APK file, or to download the latest version from APKMirror manually
            char responce = new char();
            while (!(responce.Equals('1') || responce.Equals('2')))
            {
                Console.Clear();
                Console.WriteLine("Please specify one of the following options to begin the firmware extraction:");
                Console.WriteLine("1. Open APKMirror (Download)");
                Console.WriteLine("2. Specify file   (Pre-downloaded)");
                Console.WriteLine();
                responce = Console.ReadKey().KeyChar;
            }

            // If the user selected the APKMirror option, fetch the latest version URL and then open it using their default browser
            if (responce.Equals('1'))
            {
                Console.WriteLine();
                Console.WriteLine("Opening APKMirror, standby...");
                try
                {
                    string rssURL = "https://www.apkmirror.com/apk/anhui-huami-information-technology-co-ltd/mi-fit/feed/";
                    using (XmlReader xmlReader = XmlReader.Create(rssURL))
                    {
                        SyndicationFeed rssReader = SyndicationFeed.Load(xmlReader);
                        foreach (SyndicationItem item in rssReader.Items)
                        {
                            Console.WriteLine(item.Id);
                            Process.Start(item.Id);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to open APKMirror!" + Environment.NewLine + "Error: {0}", e);
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
                Thread.Sleep(3000);
            }

            // No matter what the users choice was, prompt them for the full path to the APK file
            Console.Clear();
            Console.WriteLine("Please enter the full path of the APK file you would like to extract:");
            string apkPath = Console.ReadLine();

            // Do some basic checks in order to ensure that the APK file actually exists and is a logical file size
            try
            {
                if (!File.Exists(apkPath) || new FileInfo(apkPath).Length < ushort.MaxValue || !Path.GetExtension(apkPath).ToLower().Equals(".apk"))
                {
                    Console.WriteLine("Failed to locate valid APK file!");
                    Console.WriteLine("Press any key to exit.");
                    Console.ReadKey();
                    Environment.Exit(1);
                }
            }
            catch
            {
                Console.WriteLine("Failed to locate valid APK file!");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }

            // Prompt the user for which device they wish to extract firmware for
            bool isValid = false;
            char selectedResponce = new char();
            while (!isValid)
            {
                Console.Clear();
                Console.WriteLine("Which of the following devices would you wish to extract firmware for?");
                for (int i = 0; i <= miWatches.Count - 1; i++)
                {
                    Console.WriteLine(i + 1 + ": " + miWatches[i].WatchName);
                }
                selectedResponce = Console.ReadKey().KeyChar;
                int.TryParse(selectedResponce.ToString(), out var result);
                if (result >= 1 && result <= miWatches.Count)
                {
                    isValid = true;
                }
            }

            // Finally, extract the firmware file(s) based on the previously selected device
            Console.Clear();
            Console.WriteLine("Begining to extract firmware for {0}...", miWatches[int.Parse(selectedResponce.ToString()) - 1].WatchName);
            try
            {
                Directory.CreateDirectory(Path.Combine("Firmware Files", miWatches[int.Parse(selectedResponce.ToString()) - 1].WatchName));
                using (ZipArchive archive = ZipFile.OpenRead(apkPath))
                {
                    foreach (var firmwareList in miWatches[int.Parse(selectedResponce.ToString()) - 1].FirmwareFiles)
                    {
                        string destinationPath = Path.GetFullPath(Path.Combine("Firmware Files", miWatches[int.Parse(selectedResponce.ToString()) - 1].WatchName, firmwareList));
                        archive.GetEntry("assets/" + firmwareList).ExtractToFile(destinationPath, true);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to decompress APK file!" + Environment.NewLine + "Error: {0}", e);
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(1);
            }
            Console.WriteLine("Extraction complete! Please check the 'Firmware Files' directory, and look for your device name.");
            Thread.Sleep(3000);
            Environment.Exit(0);
        }

        /// <summary>
        /// A simple class which allows for the easy addition of watches and firmware files
        /// </summary>
        public class WatchModels
        {
            public string WatchName;
            public List<string> FirmwareFiles;
            public WatchModels()
            {
                FirmwareFiles = new List<string>();
            }
        }
    }
}