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
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;

/*
    * * * * * * * * * * * *
    *   F L A S H   F M   *
    * * * * * * * * * * * *
    *  That was hot like leather seats on a 100 degree day. 
    *  That's fahrenheight, not Celsius. But it is BOILING IN HERE! I'm taking
    *  it off to this next track. It is hot baby.
    *  You know what it takes to make a great record and so does Laura Branigan.
    *  
    *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *
    *   N O W   P L A Y I N G:   L A U R A   B R A N I G A N   -   S E L F    C O N T R O L   *
    *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *  *
    *  
    *  It's a little self control, you show me yours I'll show you mine, It's Flash FM.
    * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
*/

namespace XoLauncher
{
    public partial class Installer : Form
    {   
        public Installer()
        {
            InitializeComponent();
        }

        string mmcPath;
        public UInt16 instStatus = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            CommonOpenFileDialog xlfDialog = new CommonOpenFileDialog();


            string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            xlfDialog.InitialDirectory = appdata;
            xlfDialog.IsFolderPicker = true;
            CommonFileDialogResult fRes = xlfDialog.ShowDialog();
            if (fRes == CommonFileDialogResult.Ok)
            {
                textBox1.Text = xlfDialog.FileName;
                mmcPath = xlfDialog.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*  check if MultiMC.exe and instances folder exist */
            string mmcExe = String.Format("{0}\\MultiMC.exe", textBox1.Text);
            string dirInst = String.Format("{0}\\instances", textBox1.Text);

            bool exeStat = File.Exists(mmcExe);
            bool insStat = Directory.Exists(dirInst);

            if (!exeStat || !insStat)
            {
                MessageBox.Show("The selected path is not a valid MultiMC directory!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string xoldir = String.Format("{0}\\XoLauncher", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            
            bool xldExists = System.IO.Directory.Exists(xoldir);

            if (!xldExists)
            {
                /* dir doesnt exist, create one */
                System.IO.Directory.CreateDirectory(xoldir);
            }

            string cfgPath = String.Format("{0}\\xol.cfg", xoldir);

            /* check if cfg exists */
            /* god, this language is stupid af.. */
            bool xlCfgExists = System.IO.File.Exists(xoldir);

            if (!xlCfgExists)
            {
                /* cfg doesn't exist, create one.. */
                var cfg = File.Create(cfgPath);
                cfg.Close();
               
            }


            string writePath = String.Format("MultiMC='{0}'", mmcPath);
            File.WriteAllText(cfgPath, writePath);

            instStatus = 1;
            Form1 mainForm = new Form1();
            mainForm.Show();
            mainForm.waitSignal = true;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
