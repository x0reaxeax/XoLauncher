/*
  * MIT License
  * 
  * Copyright (c) 2020 x0reaxeax (https://github.com/x0reaxeax)
  * 
  * Permission is hereby granted, free of charge, to any person obtaining a copy
  * of this software and associated documentation files (the "Software"), to deal
  * in the Software without restriction, including without limitation the rights
  * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
  * copies of the Software, and to permit persons to whom the Software is
  * furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in all
  * copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
  * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
  * SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using System.Threading;
using System.IO.Compression;
using System.Diagnostics;

/*
    * * * * * * * * * * * *
    *   F L A S H   F M   *
    * * * * * * * * * * * *
    * Wooooooww! It's Flash FM. I like ya huge and Flashy.. Flashy flashy..
    * Flash flash. Hmm.. It's Flash FM.
    *
    * If nobody understands you, we do. Flash FM, music for the me generation.
    * 
    * You're out of touch, I'm out of time.. But I'm out of my head when you're not around
    * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
*/

namespace XoLauncher
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public bool waitSignal = false;

        /* Move Form (thanks StackOverflow <3 */
        private bool mouseDown;
        private Point lastLocation;

        /* appdata is actually XoLauncher folder path in APPDATA */
        string appdata;

        /* config path */
        //uint mmcLine;
        string cfgPath;
        string mmcPath;
        string localSums;
        string currentIMP;
        string currentIMPNaked;
        string instancesPath;

        uint modsInstalled = 0;

        string stripPath;
        string readCfg;

        /* first time run 0 - Nope, 1 - Yes, 2 - NULL */
        uint firstTimeRun = 2;
        UInt16 startup = 1;

        private readonly AutoResetEvent mWaitForThread = new AutoResetEvent(false);

        Installer fInst;

        public string SHA256CheckSum(string filePath)
        {
            using (FileStream stream = File.OpenRead(filePath))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(stream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            lastLocation = e.Location;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                this.Location = new Point(
                    (this.Location.X - lastLocation.X) + e.X, (this.Location.Y - lastLocation.Y) + e.Y);

                this.Update();
            }
        }
        /* </MouseMove> */


        void startDownload(string url, string dlLoc)
        {

            using (WebClient client = new WebClient())
            {
                /* old async download, im leaving this here for the possibility of dynamic progress bar in the future */
                //client.DownloadProgressChanged += /*new DownloadProgressChangedEventHandler(*/client_DownloadProgressChanged/*)*/;
                //client.DownloadFileCompleted += /*new AsyncCompletedEventHandler(*/client_DownloadFileCompleted/*)*/;
                //Clipboard.SetText(url);
                client.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                client.DownloadFile(new Uri(url), dlLoc);

            }
                //Thread thread = new Thread(async() => {
                //WebClient client = new WebClient();    
            //MessageBox.Show("URL: " + url);
            //});
            //thread.Start();
        }
        private void button3_Click(object sender, EventArgs e)
        {
            button3.Text = "Checking for Updates..";
            string exeshasum;
            string localsum;
            using (WebClient txtClient = new WebClient())
            {
                txtClient.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
                exeshasum = txtClient.DownloadString("https://raw.githubusercontent.com/x0reaxeax/XoLauncher/master/launchersum.dat");
            }


            localsum = SHA256CheckSum(System.Reflection.Assembly.GetExecutingAssembly().Location).ToLower();
            exeshasum = exeshasum.Substring(0, 64);

            if (String.Equals(exeshasum, localsum, StringComparison.InvariantCultureIgnoreCase)) {
                MessageBox.Show("XoLauncher is up-to-date!", "Strawberry Fields Forever", MessageBoxButtons.OK, MessageBoxIcon.Information);
                button3.Text = "Up-to-date!";
                return;
            }
            MessageBox.Show("Update available! Click 'OK' to update XoLauncher.", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);

            string dlLoc = String.Format("{0}\\XoLauncher_Update.exe", appdata);
            startDownload("https://github.com/x0reaxeax/XoLauncher/releases/download/rls/XoLauncher.exe", dlLoc);

            MessageBox.Show("Downloaded update to: " + dlLoc, "Update Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);

            button3.Text = "Update Complete";
            /*if (localsum != exeshasum)
            {
                //MessageBox.Show("Update available! Click 'OK' to update XoLauncher.", "Update Available", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }*/
        }

        /* async download events */
        /*void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                //button2.Text = "Downloaded " + e.BytesReceived + " of " + e.TotalBytesToReceive;
                //button2.Text = ""
                progressBar1.Value = e.ProgressPercentage; //int.Parse(Math.Truncate(percentage).ToString());
                //progressBar1.Refresh();
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            
            if (e.Error != null)
            {
                //MessageBox.Show(e.Error.ToString());
                MessageBox.Show("Error: " + e.Error.ToString(), "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
            this.BeginInvoke((MethodInvoker)delegate {
                button2.Text = "Completed";
            });
            //mWaitForThread.Set();
        }*/

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkedListBox1.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please select Minecraft instance first!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            /* Launch Minecraft */
            button2.Text = "Downloading checksums..";
            //progressBar1.Visible = true;
            string dlLoc = String.Format("{0}\\sums.txt", appdata);

            if (File.Exists(dlLoc))
            {
                File.Delete(dlLoc);
            }
            startDownload("https://raw.githubusercontent.com/x0reaxeax/XoLauncher/master/shasums.txt", dlLoc);

            button2.Text = "Synchronizing Mods..";
            button2.Refresh();

            var lineCount = File.ReadLines(dlLoc).Count();

            if (modsInstalled < lineCount)
            {
                button2.Text = "Downloading Missing Mods..";
                button2.Refresh();

            } else /*if (modsInstalled == lineCount)*/
            {
                button2.Text = "Checking for Updates..";
                button2.Refresh();
            }

            List<string> undetectedMods = new List<string>();

            foreach (var sumLine in File.ReadLines(dlLoc))
            {
                foreach (Match match in Regex.Matches(sumLine.ToString(), "shasum: \"([^\"]*)\" mod: \"([^\"]*)\""))
                {                 
                    if (!File.Exists(localSums))
                    {
                        undetectedMods.Add(match.ToString());
                    } else
                    {
                        if (!File.ReadAllText(localSums).Contains(match.ToString().Substring(9, 64)))
                        {
                            /* mod sha match found */
                            undetectedMods.Add(match.ToString());
                        }
                    }
                }
            }

            foreach (var array in undetectedMods)
            {
                foreach (Match spm in Regex.Matches(array.ToString(), "mod: \"([^\"]*)\""))
                {
                    string url = spm.ToString().Replace("mod:", "");
                    url = url.Replace("\"", "");
                    string modlLoc = String.Format("{0}\\.minecraft\\mods\\{1}", currentIMP, url.Substring(63));

                    button2.Text = "Downloading: " + url.Substring(63);
                    button2.Refresh();
                    startDownload(url, modlLoc);

                }
            }

            button2.Text = "Download Complete";

            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = String.Format("/C {0}\\MultiMC.exe -l \"{1}\"", mmcPath, currentIMPNaked);
            //MessageBox.Show(startInfo.Arguments); //dbg
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            button2.Text = "Starting Minecraft..";
            process.Start();
        }

        private void Form1_Activated(object sender, EventArgs e)
        {      
            if (firstTimeRun == 1)
            {
                this.Hide();
            }
        }

        void LoadSortInstances()
        {
            string[] instDirs = Directory.GetDirectories(instancesPath, "*", SearchOption.TopDirectoryOnly);
            foreach (var dir in instDirs)
            {
                string _tmpInst = String.Format("{0}\\{1}", dir, "instance.cfg");
                bool isInstance = File.Exists(_tmpInst);
                bool hasMCFolder = Directory.Exists(dir + "\\.minecraft");
                if (isInstance && hasMCFolder)
                {
                    checkedListBox1.Items.Add(Path.GetFileName(dir));
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            /* Startup */

            /* check fist-time startup */

            /* assemble appdata XoLauncher directory path */
            appdata = String.Format("{0}\\XoLauncher", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            //listBox1.Items.Add(string.Join(Environment.NewLine, appdata)); /* dbg */

            /* check if XoLauncher directory exists */
            bool xldExists = System.IO.Directory.Exists(appdata);

            if (!xldExists)
            {
                /* dir doesnt exist, create one */
                firstTimeRun = 1;
            }

            if (startup == 1)
            {
                cfgPath = String.Format("{0}\\XoLauncher\\xol.cfg", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                startup = 0;
            }

            if (firstTimeRun == 1)
            {
                fInst = new Installer();
                fInst.Show();
            } else
            {
                StreamReader cfg = new StreamReader(cfgPath);

                while ((readCfg = cfg.ReadLine()) != null)
                {
                    foreach (Match m in Regex.Matches(readCfg, "MultiMC='[^']*'"))
                    {
                        stripPath = m.ToString();
                        mmcPath = string.Join(" ", stripPath.Split('\'').Where((x, i) => i % 2 != 0));
                        break;
                    }
                }

                if (mmcPath == null)
                {
                    MessageBox.Show("Unable to determine MultiMC path! Try resetting XoLauncher config.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }

                mmcPathLabel.Text = mmcPath;
                instancesPath = String.Format("{0}\\instances", mmcPath);

                /* load mods */
                LoadSortInstances();
            }
        }

        private void checkedListBox1_MouseUp(object sender, MouseEventArgs e)
        {
            label4.Text = "Loading Mods..";
            label4.Refresh();

            int index = ((CheckedListBox)sender).SelectedIndex;
            for (int ix = 0; ix < ((CheckedListBox)sender).Items.Count; ++ix)
                if (index != ix) { ((CheckedListBox)sender).SetItemChecked(ix, false); }
                else ((CheckedListBox)sender).SetItemChecked(ix, true);

            listBox1.Items.Clear();
            currentIMP = "";
            /* load mods (IMP = instance mods path) */
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    currentIMP = String.Format("{0}\\{1}", instancesPath, checkedListBox1.Items[i]);
                    currentIMPNaked = checkedListBox1.Items[i].ToString();
                    break;
                }
            }

            if (currentIMP == null)
            {
                MessageBox.Show("Unable to load mods for selected instance!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int sanityName = 0;
            int sanityVersion = 0;
            string[] mods = Directory.GetFiles(currentIMP + "\\.minecraft\\mods");
            localSums = "";
            localSums = String.Format("{0}\\localsums.txt", appdata);
            if (File.Exists(localSums))
            {
                File.Delete(localSums);
            }

            foreach (var mod in mods)
            {
                bool isJar = mod.EndsWith(".jar");
                if (isJar)
                {

                    string shasum = SHA256CheckSum(mod);
                    //string entry = String.Format("Mod: \"{0}\" sha256sum: \"{1}\"", Path.GetFileName(mod), shasum);
                    listBox1.Items.Add(Path.GetFileName(mod));

                    string modNsum = String.Format("shasum: \"{0}\" mod: \"{1}\"", shasum.ToLower(), Path.GetFileName(mod));
                    using (StreamWriter writeSums = new StreamWriter(localSums, append: true))
                    {
                        writeSums.WriteLine(modNsum);
                    }
                    ++modsInstalled;
                 
                 /* 
                     * This is a digger for .jar files. it opens up .jar mod as an archive and opens 'mcmod.info'
                     * for reading mod name and version.
                     * Leaving this here, because in the future, there SHOULD be an automatic old-mod deleter
                 */

                    /*
                     * string modEntry = "";
                     * using (ZipArchive zip = ZipFile.Open(mod, ZipArchiveMode.Read))
                     * {
                     *     foreach (ZipArchiveEntry entry in zip.Entries)
                     *     {
                     *         if (entry.Name == "mcmod.info")
                     *         {
                     *             using (var stream = entry.Open())
                     *                 using (var reader = new StreamReader(stream))
                     *             {
                     *                 string line;
                     *                 while ((line = reader.ReadLine()) != null)
                     *                 {
                     *                     line = line.ToString();
                     *                     foreach (Match inm in Regex.Matches(line, "\"name\": \"[^\"]*\""))
                     *                     {
                     *                         string inline = inm.ToString();
                     *                         inline = Regex.Replace(inline, "\"name\":", "");
                     *                         inline = inline.Replace("\"", "");
                     *                         modEntry = "Mod:" + inline;
                     *                         ++sanityName;
                     *                     }
                     * 
                     *                     foreach (Match inm in Regex.Matches(line, "\"version\": \"[^']*\""))
                     *                     {
                     *                         string inline = inm.ToString();
                     *                         inline = Regex.Replace(inline, "\"version\":", "");
                     *                         inline = inline.Replace("\"", "");
                     *                         MessageBox.Show(modEntry);
                     *                         ++sanityVersion;
                     *                     }
                     *                     break;
                     *                 }
                     *             }
                     *             var memStream = entry.Open();
                     *             string readMemStream;
                     *             StreamReader readInfo = new StreamReader(memStream);
                     * 
                     *             while ((readMemStream = readInfo.ReadLine()) != null)
                     *             {
                     *                 foreach (Match m in Regex.Matches(readMemStream, "\"([^\"]*)\""))
                     *                 {
                     *                     string jarStrPath = m.ToString();
                     *                     mmcPath = string.Join(" ", jarStrPath.Split('\'').Where((x, i) => i % 2 != 0));
                     *                     break;
                     *                 }
                     *                 //MatchCollection col = Regex.Matches(readMemStream, "\\\"(.*?)\\\"");
                     *             }
                     *         }
                     *     }
                     * }
                    */            
                }
            }
            
            if (sanityVersion != sanityName)
            {
                MessageBox.Show("An error has occurred while loading mods for selected instance!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            label4.Text = "Local Mods [" + modsInstalled + "]";

            modsInstalled = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /* about button */
            MessageBox.Show("XoLauncher by x0reaxeax - Walrus Gumboot LLC 2020" + Environment.NewLine + Environment.NewLine + "github.com/x0reaxeax", "Thanks for clicking this button!", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
