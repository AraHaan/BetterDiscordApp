﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using asardotnet;

namespace BetterDiscordWI.panels
{
    public partial class Panel2 : UserControl, IPanel
    {
        private String _dataPath, _tempPath;
        private Utils _utils;

        public Panel2()
        {
            InitializeComponent();
        }

        public void SetVisible()
        {
            GetParent().btnBack.Enabled = false;
            GetParent().btnNext.Enabled = false;
            GetParent().btnBack.Visible = false;
            GetParent().btnNext.Visible = false;
            GetParent().btnCancel.Enabled = false;

            _utils = new Utils();
            if (GetParent().DiscordPath.Contains("Discord\\"))
            {
                AppendLog("Killing Discord");
                foreach (var process in Process.GetProcessesByName("Discord"))
                {
                    process.Kill();
                }
            }
            if (GetParent().DiscordPath.Contains("DiscordCanary\\"))
            {
                AppendLog("Killing DiscordCanary");
                foreach (var process in Process.GetProcessesByName("DiscordCanary"))
                {
                    process.Kill();
                }
            }
            if (GetParent().DiscordPath.Contains("DiscordPTB\\"))
            {
                AppendLog("Killing DiscordPTB");
                foreach (var process in Process.GetProcessesByName("DiscordPTB"))
                {
                    process.Kill();
                }
            }

            CreateDirectories();
        }

        private void CreateDirectories()
        {
            int errors = 0;
            Thread t = new Thread(() =>
            {
                _dataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BetterDiscord";
                _tempPath = _dataPath + "\\temp";
                AppendLog("Deleting old cached files");
                try
                {
                    if (File.Exists(_dataPath + "\\emotes_bttv.json"))
                    {
                        File.Delete(_dataPath + "\\emotes_bttv.json");
                    }
                    if (File.Exists(_dataPath + "\\emotes_bttv_2.json"))
                    {
                        File.Delete(_dataPath + "\\emotes_bttv_2.json");
                    }
                    if (File.Exists(_dataPath + "\\emotes_ffz.json")) {
                        File.Delete(_dataPath + "\\emotes_ffz.json");
                    }
                    if (File.Exists(_dataPath + "\\emotes_twitch_global.json")) {
                        File.Delete(_dataPath + "\\emotes_twitch_global.json");
                    }
                    if (File.Exists(_dataPath + "\\emotes_twitch_subscriber.json")) {
                        File.Delete(_dataPath + "\\emotes_twitch_subscriber.json");
                    }
                    if (File.Exists(_dataPath + "\\user.json")) {
                        File.Delete(_dataPath + "\\user.json");
                    }
                } catch {
                    AppendLog("Failed to delete one or more cached files");
                    errors = 1;
                    Finalize(errors);
                }
                AppendLog("Downloading new cache files");
                try
                {
                    if (!File.Exists(_dataPath + "\\emotes_bttv.json"))
                    {
                        DownloadEmoteJSON("emotes_bttv.json", "https://api.betterttv.net/emotes");
                    }
                    if (!File.Exists(_dataPath + "\\emotes_bttv_2.json"))
                    {
                        DownloadEmoteJSON("emotes_bttv_2.json", "https://raw.githubusercontent.com/AraHaan/BetterDiscordApp/test/data/emotedata_bttv.json'");
                    }
                    if (!File.Exists(_dataPath + "\\emotes_ffz.json")) {
                        DownloadEmoteJSON("emotes_ffz.json", "https://raw.githubusercontent.com/AraHaan/BetterDiscordApp/test/data/emotedata_ffz.json");
                    }
                    if (!File.Exists(_dataPath + "\\emotes_twitch_global.json")) {
                        DownloadEmoteJSON("emotes_twitch_global.json", "https://twitchemotes.com/api_cache/v2/global.json");
                    }
                    if (!File.Exists(_dataPath + "\\emotes_twitch_subscriber.json")) {
                        DownloadEmoteJSON("emotes_twitch_subscriber.json", "https://twitchemotes.com/api_cache/v2/subscriber.json");
                    }
                } catch {
                    AppendLog("Failed to download one or more cached files");
                    errors = 1;
                    Finalize(errors);
                }


                if (Directory.Exists(_tempPath))
                {
                    AppendLog("Deleting temp path");
                    Directory.Delete(_tempPath, true);
                }

                while (Directory.Exists(_tempPath))
                {
                    Debug.Print("Waiting for dirdel");
                    Thread.Sleep(100);
                }

                Directory.CreateDirectory(_tempPath);

                DownloadResource("BetterDiscordBeta.zip", "https://github.com/AraHaan/BetterDiscordApp/archive/test-stable.zip");

                while (!File.Exists(_tempPath + "\\BetterDiscordBeta.zip"))
                {
                    Debug.Print("Waiting for download");
                    Thread.Sleep(100);
                }

                AppendLog("Extracting BetterDiscord v2.6 Beta");

                ZipArchive zar =
                    ZipFile.OpenRead(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                     "\\BetterDiscord\\temp\\BetterDiscordBeta.zip");
                zar.ExtractToDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                       "\\BetterDiscord\\temp\\");

                DeleteDirs();
            });
            t.Start();

        }


        private void DeleteDirs()
        {
            int errors = 0;
            Thread t = new Thread(() =>
            {
                String dir = GetParent().DiscordPath + "\\resources\\app";

                if (Directory.Exists(dir))
                {
                    try
                    {
                        AppendLog("Deleting " + dir);
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                        AppendLog("Error Failed to Delete the '" + dir + "\\resources\\app' Directory.");
                        errors = 1;
                        Finalize(errors);
                    }
                }

                while (Directory.Exists(dir))
                {
                    Debug.Print("Waiting for dirdel");
                    Thread.Sleep(100);
                }

                dir = GetParent().DiscordPath + "\\resources\\node_modules\\BetterDiscord";


                if (Directory.Exists(dir))
                {
                    AppendLog("Deleting " + dir);
                    Directory.Delete(dir, true);
                }

                while (Directory.Exists(dir))
                {
                    Debug.Print("Waiting for dirdel");
                    Thread.Sleep(100);
                }

                AppendLog("Moving BetterDiscord to resources\\node_modules\\");

                Directory.Move(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BetterDiscord\\temp\\BetterDiscordApp-test-stable", GetParent().DiscordPath + "\\resources\\node_modules\\BetterDiscord");

                try
                {
                    if(File.Exists(GetParent().DiscordPath + "\\resources\\app.asar"))
                    {
                        AppendLog("Extracting app.asar");
                        AsarArchive archive = new AsarArchive(GetParent().DiscordPath + "\\resources\\app.asar");

                        AsarExtractor extractor = new AsarExtractor();

                        extractor.ExtractAll(archive, GetParent().DiscordPath + "\\resources\\app\\");
                        Splice();
                    } else {
                        AppendLog("Error: app.asar does not exist. Installation cannot Continue.");
                        errors = 1;
                        Finalize(errors);
                    }
                }
                catch
                {
                    if (File.Exists(Application.StartupPath + "\\Newtonsoft.Json.dll"))
                    {
                        if (File.Exists(Application.StartupPath + "\\asardotnet.dll"))
                        {
                            AppendLog("Error Extracting app.asar: Unknown Error. Installation cannot Continue. Maybe JsonReaderException? or Something else like AccessDeniedException? Is Discord Running as Admin or something?");
                            errors = 1;
                            Finalize(errors);
                        }
                        else
                        {
                            AppendLog("Error Extracting app.asar: asardotnet.dll might not be present in the Installer Folder. Installation cannot Continue.");
                            errors = 1;
                            Finalize(errors);
                        }
                    }
                    else
                    {
                        if (File.Exists(Application.StartupPath + "\\asardotnet.dll"))
                        {
                            AppendLog("Error Extracting app.asar: Newtonsoft.Json.dll might not be present in the Installer Folder. Installation cannot Continue.");
                            errors = 1;
                            Finalize(errors);
                        }
                        else
                        {
                            AppendLog("Error Extracting app.asar: Newtonsoft.Json.dll might not be present in the Installer Folder. Installation cannot Continue.");
                            AppendLog("Error Extracting app.asar: asardotnet.dll might not be present in the Installer Folder. Installation cannot Continue.");
                            errors = 2;
                            Finalize(errors);
                        }
                    }
                    if (File.Exists(Application.StartupPath + "\\asardotnet.dll"))
                    {
                        //Do nothing because the file exists.
                    }
                    else
                    {
                        AppendLog("Error Extracting app.asar: asardotnet.dll might not be present in the Installer Folder. Installation cannot Continue.");
                        errors = 1;
                        Finalize(errors);
                    }
                }
            });
            
            
            t.Start();
        }


        private void DownloadResource(String resource, String url)
        {
            AppendLog("Downloading Resource: " + resource);

            WebClient webClient = new WebClient();
            webClient.Headers["User-Agent"] = "Mozilla/5.0";
            
            webClient.DownloadFile(new Uri(url), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BetterDiscord\\temp\\" + resource);
        }

        private void DownloadEmoteJSON(String jsonfile, String url)
        {
            AppendLog("Downloading JSON: " + jsonfile);

            WebClient webClient = new WebClient();
            webClient.Headers["User-Agent"] = "Mozilla/5.0";
            
            webClient.DownloadFile(new Uri(url), Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BetterDiscord\\" + jsonfile);
        }

        private void Splice()
        {
            int errors = 0;
            String indexloc = GetParent().DiscordPath + "\\resources\\app\\app\\index.js";

            Thread t = new Thread(() =>
            {
                List<String> lines = new List<string>();
                AppendLog("Spicing index");
                using (FileStream fs = new FileStream(indexloc, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        String line = "";
                        while((line = reader.ReadLine()) != null)
                        {
                            if (GetParent().DiscordPath.Contains("Discord\\"))
                            {
                                if (line.Contains("var _discord_overlay2"))
                                {
                                    lines.Add(line);
                                    lines.Add("var _betterDiscord = require('betterdiscord');");
                                    if (line.Contains("mainWindow = new _electron.BrowserWindow"))
                                    {
                                        lines.Add(line);
                                        lines.Add(File.ReadAllText("splice"));
                                    }
                                    else
                                    {
                                        lines.Add(line);
                                    }
                                }
//                                else
//                                {
//                                    AppendLog("Discord must have updated and might not work now. Please Ask the Installer Dev to fix this.");
//                                    errors = 1;
//                                    Finalize(errors);
//                                }
                            }
                            if (GetParent().DiscordPath.Contains("DiscordCanary\\"))
                            {
                                if (line.Contains("var _discord_overlay2"))
                                {
                                    lines.Add(line);
                                    lines.Add("var _betterDiscord = require('betterdiscord');");
                                    if (line.Contains("mainWindow = new _electron.BrowserWindow"))
                                    {
                                        lines.Add(line);
                                        lines.Add(File.ReadAllText("splice"));
                                    }
                                    else
                                    {
                                        lines.Add(line);
                                    }
                                }
//                                else
//                                {
//                                    AppendLog("DiscordCanary must have updated and might not work now. Please Ask the Installer Dev to fix this.");
//                                    errors = 1;
//                                    Finalize(errors);
//                                }
                            }
                            if (GetParent().DiscordPath.Contains("DiscordPTB\\"))
                            {
                                if (line.Contains("var _singleInstance2"))
                                {
                                    lines.Add(line);
                                    lines.Add("var _betterDiscord = require('betterdiscord');");
                                    //"mainWindow = new _browserWindow2" was changed in DiscordPTB v0.0.6
                                    if (line.Contains("mainWindow = new _electron.BrowserWindow"))
                                    {
                                        lines.Add(line);
                                        lines.Add(File.ReadAllText("splice"));
                                    }
                                    else
                                    {
                                        lines.Add(line);
                                    }
                                }
//                                else
//                                {
//                                    AppendLog("DiscordPTB must have updated and might not work now. Please Ask the Installer Dev to fix this.");
//                                    errors = 1;
//                                    Finalize(errors);
//                                }
                            }
                        }
                    }
                }

                AppendLog("Writing index");

                File.WriteAllLines(indexloc, lines.ToArray());

                
                AppendLog("Finished installation, verifying installation...");


                String curPath = GetParent().DiscordPath + "\\resources\\app\\app\\index.js";
                
                if (!File.Exists(curPath))
                {
                    AppendLog("ERROR: FILE: " + curPath + " DOES NOT EXIST!");
                    errors++;
                }

                curPath = GetParent().DiscordPath + "\\resources\\node_modules\\BetterDiscord";

                if (!Directory.Exists(curPath))
                {
                    AppendLog("ERROR: DIRECTORY: " + curPath + " DOES NOT EXIST");
                    errors++;
                }


                String basePath = GetParent().DiscordPath + "\\resources\\node_modules\\BetterDiscord";
                String[] bdFiles = {"\\package.json", "\\betterdiscord.js", "\\lib\\BetterDiscord.js", "\\lib\\config.json", "\\lib\\Utils.js"};

                foreach (string s in bdFiles.Where(s => !File.Exists(basePath + s)))
                {
                    AppendLog("ERROR: FILE: " + basePath + s + " DOES NOT EXIST");
                    errors++;
                }


                Finalize(errors);
            });

            t.Start();
        }

        private void Finalize(int errors)
        {
            AppendLog("Finished installing BetterDiscord with " + errors + " errors");


            Invoke((MethodInvoker) delegate
            {
                GetParent().finished = true;
                GetParent().btnCancel.Text = "OK";
                GetParent().btnCancel.Enabled = true;
            });

            if (GetParent().RestartDiscord)
            {
                if (GetParent().DiscordPath.Contains("\\Discord\\"))
                {
                    Process.Start(GetParent().DiscordPath + "\\Discord.exe");
                }
                if (GetParent().DiscordPath.Contains("\\DiscordCanary\\"))
                {
                    Process.Start(GetParent().DiscordPath + "\\DiscordCanary.exe");
                }
                if (GetParent().DiscordPath.Contains("\\DiscordPTB\\"))
                {
                    Process.Start(GetParent().DiscordPath + "\\DiscordPTB.exe");
                }
            }
        }

        public FormMain GetParent()
        {
            return (FormMain)ParentForm;
        }

        public void BtnNext()
        {
            throw new NotImplementedException();
        }

        public void BtnPrev()
        {
            throw new NotImplementedException();
        }

        private void AppendLog(String message)
        {
            Invoke((MethodInvoker) delegate
            {
                rtLog.AppendText(message + "\n");
                rtLog.SelectionStart = rtLog.Text.Length;
                rtLog.ScrollToCaret();
            });
            
        }
    }
}
