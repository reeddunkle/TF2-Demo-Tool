﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.BitConverter;
using static System.Globalization.CultureInfo;
using static System.Math;
using static System.Text.Encoding;

namespace TF2_Demo_Tool
{
    public partial class Form1 : Form
    {
        int version = 400;
        int Number = 0;
        String TextInJson;
        string splitter;
        string PathDemoFiles;
        string PathMoveTo;
        string PathMoveToBackup = null;
        string SaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).ToString() + @"\TF2DemoTool\";
        string[] FilePathsDem;
        bool Hermuth = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            checkForMostRecentVersion();
            System.IO.Directory.CreateDirectory(SaveFolder);
            dataGridView1.Rows.Clear();
            dataGridView1.ClearSelection();
            try
            {
                string savePath = SaveFolder + "FolderMoveTo" + ".txt";
                PathMoveToBackup = File.ReadLines(savePath).First();
                Hermuth = true;
            }
            catch (Exception) { }
            try
            {
                string savePath = SaveFolder + "FolderDemos" + ".txt";
                PathDemoFiles = File.ReadLines(savePath).First();
                splitter = PathDemoFiles;
                Import(PathDemoFiles);
                
                ButtonMoveBookmarksToNewFolder.Enabled = true;
                buttonRemoveEmpty.Enabled = true;
                ButtonMoveBookmarks.Enabled = true;
                dataGridView1.SelectAll();
                dataGridView1.ClearSelection();

                if (Hermuth == true) { ButtonMoveBookmarks.Enabled = true; }
            }
            catch (Exception) { }


        }
        
        private void Import(string path)
        {
            splitter = path + @"\";
            FilePathsDem = Directory.GetFiles(path, "*.dem", SearchOption.TopDirectoryOnly);
            dataGridView1.Rows.Clear();
            var DataGridResources = new DemoResources();
            for (int i = 0; i < FilePathsDem.Length; i++)
            {
                dataGridView1.ClearSelection();
                string nameofFile = FilePathsDem[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                DataGridResources = getDataFromDemo(FilePathsDem[i]);
                dataGridView1.Rows.Add(nameofFile, DataGridResources.MapName, DataGridResources.Ticks, DataGridResources.PlayerName, DataGridResources.ServerName, FilePathsDem[i]);
            }

            if (FilePathsDem.Count() > 0)
            {
                ButtonMoveBookmarksToNewFolder.Enabled = true;
                buttonRemoveEmpty.Enabled = true;
                ButtonMoveBookmarks.Enabled = true;

                buttonSelectAll.Enabled = true;
                buttonSelectBookmarked.Enabled = true;
                buttonSelectEmpty.Enabled = true;
                buttonSelectNone.Enabled = true;
            }
            if (FilePathsDem.Count() == 0)
            {
                ButtonMoveBookmarksToNewFolder.Enabled = false;
                buttonRemoveEmpty.Enabled = false;
                ButtonMoveBookmarks.Enabled = false;

                buttonSelectAll.Enabled = false;
                buttonSelectBookmarked.Enabled = false;
                buttonSelectEmpty.Enabled = false;
                buttonSelectNone.Enabled = false; 
            }

            richTextBox1.Text = null;
        }
        
        private class JsonPropreties
        {
            [JsonProperty("events")]
            public List<object> events { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("value")]
            public string value { get; set; }
            [JsonProperty("tick")]
            public int tick { get; set; }
            
        }

        private void ButtonChooseFolder_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "Choose folder with Valve demos";
            sfd.Title = "Choose folder with Valve demos";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                PathDemoFiles = Path.GetDirectoryName(sfd.FileName);
                Import(PathDemoFiles);
                if (Hermuth == true) { ButtonMoveBookmarks.Enabled = true; }
            }
        }

        private void ButtonRemoveEmpty_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to delete all demo files that do not have any event recorded on them?", "Important Question",  MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    string nameofFile = FilePathsDem[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                    TextInJson = "Stuff just to be sure";
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                        string test = TextInJson.Split('"')[3];
                    }
                    catch (Exception)
                    {
                        try
                        {
                            TextInJson = File.ReadAllText(splitter + nameofFile + ".txt");
                            if (TextInJson == "")
                            {
                                Number++;
                                File.Delete(splitter + nameofFile + ".txt");
                                File.Delete(splitter + nameofFile + ".dem");
                                File.Delete(splitter + nameofFile + ".jpg");
                            }
                        }
                        catch (Exception)
                        {
                            Number++;
                            File.Delete(splitter + nameofFile + ".json");
                            File.Delete(splitter + nameofFile + ".dem");
                            File.Delete(splitter + nameofFile + ".tga");
                        }
                    }
                }
                MessageBox.Show(Number.ToString() + " files were successfully removed");
                Number = 0;
                Import(PathDemoFiles); 
            }
        }

        private void ButtonMoveBookmarks_Click(object sender, EventArgs e)
        {
            if (PathMoveToBackup != null)
            {
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    string nameofFile = FilePathsDem[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                    TextInJson = "";
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                        string[] splitted = TextInJson.Split('"');

                        if (splitted.Length > 3)
                        {
                            File.Move(splitter + nameofFile + ".json", PathMoveToBackup + @"\" + nameofFile + ".json");
                            try
                            {
                                File.Move(splitter + nameofFile + ".dem", PathMoveToBackup + @"\" + nameofFile + ".dem");
                            }
                            catch (Exception) { }
                            try
                            {
                                File.Move(splitter + nameofFile + ".tga", PathMoveToBackup + @"\" + nameofFile + ".tga");
                            }
                            catch (Exception) { }
                            Number++;
                        }
                    }
                    catch (Exception)
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".txt");
                        if (TextInJson != "")
                        {
                            File.Move(splitter + nameofFile + ".txt", PathMoveToBackup + @"\" + nameofFile + ".txt");
                            try
                            {
                                File.Move(splitter + nameofFile + ".dem", PathMoveToBackup + @"\" + nameofFile + ".dem");
                            }
                            catch (Exception) { }
                            try
                            {
                                File.Move(splitter + nameofFile + ".jpg", PathMoveToBackup + @"\" + nameofFile + ".jpg");
                            }
                            catch (Exception) { }
                            Number++;
                        }
                    }
                }
                MessageBox.Show(Number.ToString() + " files were successfully moved");
                Import(PathDemoFiles);
                Number = 0;
            }
            else MessageBox.Show("You have not saved any 'move-to' folder, you can do so by clicking on 'File' and then 'Choose default 'move to' folder'");
        }

        private void ButtonMoveBookmarksToNewFolder_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "Choose a folder where you want to move demo files with some events on them";
            sfd.Title = "Choose a folder where you want to move demo files with some events on them";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                PathMoveTo = Path.GetDirectoryName(sfd.FileName) + @"\";
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    string nameofFile = FilePathsDem[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                    TextInJson = "";
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                        string[] splitted = TextInJson.Split('"');

                        if (splitted.Length > 3)
                        {
                            File.Move(splitter + nameofFile + ".json", PathMoveTo + nameofFile + ".json");
                            try
                            {
                                File.Move(splitter + nameofFile + ".dem", PathMoveTo + nameofFile + ".dem");
                            }
                            catch (Exception) { }
                            try
                            {
                                File.Move(splitter + nameofFile + ".tga", PathMoveTo + nameofFile + ".tga");
                            }
                            catch (Exception) { }
                            Number++;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                MessageBox.Show(Number.ToString() + " demo files were successfully moved");
                Import(PathDemoFiles);
                Number = 0;
            }
        }

        private void BackupFolder(string PathToFolder, string NameofFolder)
        {
            string savePath = SaveFolder + NameofFolder + ".txt";
            System.IO.File.WriteAllText(savePath, PathToFolder);
        }

        private void DemoFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "Choose default folder with Valve demos";
            sfd.Title = "Choose default folder with Valve demos";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                PathDemoFiles = Path.GetDirectoryName(sfd.FileName);
                BackupFolder(PathDemoFiles, "FolderDemos");
                Import(PathDemoFiles);
                ButtonMoveBookmarksToNewFolder.Enabled = true;
                buttonRemoveEmpty.Enabled = true;
                ButtonMoveBookmarks.Enabled = true;
                if (Hermuth == true) { ButtonMoveBookmarks.Enabled = true; }
                ButtonMoveBookmarksToNewFolder.Enabled = true;
                MessageBox.Show("Default folder with your demo files was saved and they will be automatically imported at the start");
            }
        }

        private void MNoveToStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.FileName = "Choose a default folder where you want to move demo files";
            sfd.Title = "Choose a default folder where you want to move demo file";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                PathMoveToBackup = Path.GetDirectoryName(sfd.FileName) + @"\";
                BackupFolder(PathMoveToBackup, "FolderMoveTo");
                if (dataGridView1.Rows.Count > 0)
                {
                    ButtonMoveBookmarks.Enabled = true;
                }
                Hermuth = true;
                MessageBox.Show("Default folder for moving the demo files was saved");
            }
        }

        private void resetDefaultFoldersToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            File.Delete(SaveFolder + "FolderDemos.txt");
            MessageBox.Show("Default folder with your demo files was reseted");
        }

        private void resetDefaultmoveToFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            File.Delete(SaveFolder + "FolderMoveTo.txt");
            PathMoveToBackup = null;
            ButtonMoveBookmarks.Enabled = false;
            Hermuth = false;
            MessageBox.Show("Default folder for moving the demo files was reseted");
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private class DemoResources
        {
            public string DemoName { get; set; } = "-";
            public string MapName { get; set; } = "-";
            public string Ticks { get; set; } = "-";
            public string PlayerName { get; set; } = "-";
            public string ServerName { get; set; } = "-";
        }

        private DemoResources getDataFromDemo(string pathToFile)
        {
            var result = new DemoResources();
            try
            {
                using (var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    ASCII.GetString(br.ReadBytes(16)).TrimEnd('\0');
                    result.ServerName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');
                    result.PlayerName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');
                    result.MapName = ASCII.GetString(br.ReadBytes(260)).TrimEnd('\0');
                    ASCII.GetString(br.ReadBytes(264)).TrimEnd('\0');
                    result.Ticks = (Abs(ToInt32(br.ReadBytes(4), 0))).ToString(InvariantCulture);
                }
            }
            catch (Exception)
            {
                result.ServerName = "Not readable";
                result.PlayerName = "Not readable";
                result.MapName = "Not readable";
                result.Ticks = "Not readable";
            }
        return result;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                buttonRemoveSelected.Enabled = true;
                buttonMoveSelected.Enabled = true;
                buttonMoveSelectedSavedFolder.Enabled = true;
            }
            if (dataGridView1.SelectedRows.Count == 0)
            {
                buttonRemoveSelected.Enabled = false;
                buttonMoveSelected.Enabled = false;
                buttonMoveSelectedSavedFolder.Enabled = false;
            }
            string nameofFile = "";
            try
            {
                string value2 = "";
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    value2 = row.Cells[5].Value.ToString();
                }

                richTextBox1.Text = null;
                nameofFile = value2.Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];

                TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                var JsonDeserializetion1 = JsonConvert.DeserializeObject<JsonPropreties>(TextInJson);
                for (int i = 0; i < JsonDeserializetion1.events.Count; i++)
                {
                    var JsonDeserializetion2 = JsonConvert.DeserializeObject<JsonPropreties>(JsonDeserializetion1.events[i].ToString());
                    richTextBox1.Text += "Name: " + JsonDeserializetion2.Name + Environment.NewLine + "Value:  " + JsonDeserializetion2.value + Environment.NewLine + "Tick:    " + JsonDeserializetion2.tick + Environment.NewLine + Environment.NewLine;
                }
            }
            catch (Exception)
            {
                try
                {
                    TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                    richTextBox1.Text = TextInJson;
                }
                catch (Exception)
                {
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".txt");
                        richTextBox1.Text = TextInJson;
                    }
                    catch (Exception)
                    {
                        richTextBox1.Text = "no .json or .txt file associated with selected demo found";
                    }
                }
            }
        }

        private void checkForMostRecentVersion()
        {
            try
            {
                WebClient VersionSite = new WebClient();
                int MostRecentVersion = int.Parse(VersionSite.DownloadString(@"https://raw.githubusercontent.com/stepanex/TF2-Demo-Tool/master/version.txt"));
                if (MostRecentVersion > version)
                {
                    if (MessageBox.Show("There is new version in github: https://github.com/stepanex/TF2-Demo-Tool/releases. Do you want to visit the site and download most recent version?", "Visit", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start("https://github.com/stepanex/TF2-Demo-Tool/releases");
                    }
                }
            }
            catch (Exception)
            {
            }

        }

        private void buttonSelectEmpty_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            for (int i = 0; i < FilePathsDem.Length; i++)
            {
                string nameofFile = dataGridView1.Rows[i].Cells[0].Value.ToString();
                TextInJson = "Stuff just to be sure";
                try
                {
                    TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                    string test = TextInJson.Split('"')[3];
                }
                catch (Exception)
                {
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".txt");
                        if (TextInJson == "")
                        {
                        dataGridView1.Rows[i].Selected = true;
                        }
                    }
                    catch (Exception)
                    {
                        dataGridView1.Rows[i].Selected = true;
                    }
                }
            }
        }
        List<string> selected = new List<string>();
        private void buttonRemoveSelected_Click(object sender, EventArgs e)
        {
            selected.Clear();
           DialogResult result = MessageBox.Show("Do you really want to delete all selected demo files?", "Important Question", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    if (dataGridView1.Rows[i].Selected == true)
                    {
                        string nameofFile = dataGridView1.Rows[i].Cells[0].Value.ToString();
                        selected.Add(nameofFile);
                        Number++;
                    }
                }
                do
                {
                    string nameofFile = selected.First();
                    selected.Remove(nameofFile);
                    File.Delete(splitter + nameofFile + ".dem");
                    File.Delete(splitter + nameofFile + ".txt");
                    File.Delete(splitter + nameofFile + ".json");
                    File.Delete(splitter + nameofFile + ".jpg");
                    File.Delete(splitter + nameofFile + ".tga");
                } while (selected.Count>0);
                MessageBox.Show(Number.ToString() + " files were successfully removed");
                Number = 0;
                Import(PathDemoFiles);
                dataGridView1.ClearSelection();
            }
        }

        private void buttonSelectBookmarked_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    string nameofFile = dataGridView1.Rows[i].Cells[0].Value.ToString();
                    TextInJson = "";
                    try
                    {
                        TextInJson = File.ReadAllText(splitter + nameofFile + ".json");
                        string[] splitted = TextInJson.Split('"');

                        if (splitted.Length > 3)
                        {
                            dataGridView1.Rows[i].Selected = true;
                        }
                    }
                    catch (Exception)
                    {
                    }
            }
        }

        private void buttonMoveSelected_Click(object sender, EventArgs e)
        {
            selected.Clear();
            var sfd = new SaveFileDialog();
            sfd.Title = "Choose a folder where you want to move selected demo files";
            sfd.Title = "Choose a folder where you want to move selected demo files";
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(sfd.FileName))
            {
                PathMoveTo = Path.GetDirectoryName(sfd.FileName) + @"\";
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    if (dataGridView1.Rows[i].Selected == true)
                    {
                        string nameofFile = dataGridView1.Rows[i].Cells[0].Value.ToString();

                        File.Move(splitter + nameofFile + ".dem", PathMoveTo + nameofFile + ".dem");
                        try
                        {
                            File.Move(splitter + nameofFile + ".json", PathMoveTo + nameofFile + ".json");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".txt", PathMoveTo + nameofFile + ".txt");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".tga", PathMoveTo + nameofFile + ".tga");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".jpg", PathMoveTo + nameofFile + ".jpg");
                        }
                        catch (Exception) { }
                        Number++;
                    } 
                }
            }
            MessageBox.Show(Number.ToString() + " demo files were successfully moved");
            Import(PathDemoFiles);
            Number = 0;
        }

        private void buttonSelectAll_Click(object sender, EventArgs e)
        {
            dataGridView1.SelectAll();
        }

        private void buttonSelectNone_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
        }

        private void aboutAutorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form frm = new Form();
            frm.Text = "About";
            frm.Size = new Size(350, 100);
            frm.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Label lbl = new Label();
            RichTextBox rtx = new RichTextBox();
            rtx.Text = "Author: Stepanex" + Environment.NewLine + "Email: stepanex@gmx.com" + Environment.NewLine + "GitHub: https://github.com/stepanex/TF2-Demo-Tool/releases";
            rtx.Dock = DockStyle.Fill;
            rtx.ReadOnly = true;
            rtx.TabIndex = 50;
            frm.Controls.Add(rtx);
            frm.Show();
        }

        private void buttonRefresh_Click(object sender, EventArgs e)
        {
            Import(PathDemoFiles);
            if (Hermuth == true) { ButtonMoveBookmarks.Enabled = true; }
        }

        private void buttonMoveSelectedSavedFolder_Click(object sender, EventArgs e)
        {
            if (PathMoveToBackup != null)
            {
                selected.Clear();
                for (int i = 0; i < FilePathsDem.Length; i++)
                {
                    if (dataGridView1.Rows[i].Selected == true)
                    {
                        string nameofFile = dataGridView1.Rows[i].Cells[0].Value.ToString();

                        File.Move(splitter + nameofFile + ".dem", PathMoveToBackup + nameofFile + ".dem");
                        try
                        {
                            File.Move(splitter + nameofFile + ".json", PathMoveToBackup + nameofFile + ".json");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".txt", PathMoveToBackup + nameofFile + ".txt");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".tga", PathMoveToBackup + nameofFile + ".tga");
                        }
                        catch (Exception) { }
                        try
                        {
                            File.Move(splitter + nameofFile + ".jpg", PathMoveToBackup + nameofFile + ".jpg");
                        }
                        catch (Exception) { }
                        Number++;
                    }
                }
                MessageBox.Show(Number.ToString() + " demo files were successfully moved");
                Import(PathDemoFiles);
                Number = 0;
            }
            else MessageBox.Show("You have not saved any 'move-to' folder, you can do so by clicking on 'File' and then 'Choose default 'move to' folder'");
        }
    }
 }
