﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TF2_Demo_Tool
{
    public partial class Form1 : Form
    {
        string splitter;
        string path1;
        string path2;
        int Number=0;
        string[] FilePaths;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }
        
        void Import(string path)
        {
            splitter = path + @"\";
            FilePaths = Directory.GetFiles(path, "*.json", SearchOption.TopDirectoryOnly);
            flowLayoutPanel1.Controls.Clear();
            for (int i = 0; i < FilePaths.Length; i++)
            {
                string nameofFile = FilePaths[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                RadioButton rbtn = new RadioButton();
                rbtn.Text = nameofFile;
                rbtn.Tag = FilePaths[i];
                rbtn.AutoSize = true;
                rbtn.CheckedChanged += radioButton_CheckedChanged;
                if (rbtn.Size.Width < 210)
                {
                    rbtn.AutoSize = false;
                    rbtn.Size = new Size(210, 17);
                }
                flowLayoutPanel1.Controls.Add(rbtn);
            }
            richTextBox1.Text = null;
        }

        private void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rbtn = (RadioButton)sender;
            if (rbtn.Checked == true)
            {
                richTextBox1.Text = null;
                String json = File.ReadAllText(rbtn.Tag.ToString());
                try
                {
                    var JsonDeserializetion1 = JsonConvert.DeserializeObject<JsonPropreties>(json);
                    for (int i = 0; i < JsonDeserializetion1.events.Count; i++)
                    {
                        var JsonDeserializetion2 = JsonConvert.DeserializeObject<JsonPropreties>(JsonDeserializetion1.events[i].ToString());
                        richTextBox1.Text += "Name: " + JsonDeserializetion2.Name + Environment.NewLine + "Value:  " + JsonDeserializetion2.value + Environment.NewLine + "Tick:    " + JsonDeserializetion2.tick + Environment.NewLine + Environment.NewLine;
                    }
                }
                catch (Exception)
                {
                    richTextBox1.Text = json;
                }
            }
        }
       
        public class JsonPropreties
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
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Choose a folder where your demo files are";
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                path1 = fbd.SelectedPath;
                Import(path1);
                ButtonRemoveEmpty.Enabled = true;
                ButtonMoveBookmarks.Enabled = true;
            }
        }
        
        private void ButtonRemoveEmpty_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you really want to delete all demo files that do not have any event recorded on them?", "Important Question",  MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                for (int i = 0; i < FilePaths.Length; i++)
                {
                    String TextInJson = File.ReadAllText(FilePaths[i]);
                    string nameofFile = FilePaths[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                    try
                    {
                        string test = TextInJson.Split('"')[3];
                    }
                    catch (Exception)
                    {
                        Number++;
                        File.Delete(splitter + nameofFile + ".json");
                        File.Delete(splitter + nameofFile + ".dem");
                        File.Delete(splitter + nameofFile + ".tga");
                    }
                }
                MessageBox.Show(Number.ToString() + " files were successfully removed");
                Number = 0;
                Import(path1); 
            }
        }

        private void ButtonMoveBookmarks_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            fbd.Description = "Choose a folder where you want to move demo files with some events on them";
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                path2 = fbd.SelectedPath;
                for (int i = 0; i < FilePaths.Length; i++)
                {
                    String TextInJson = File.ReadAllText(FilePaths[i]);
                    string nameofFile = FilePaths[i].Split(new string[] { splitter }, StringSplitOptions.None)[1].Split('.')[0];
                    string[] splitted = TextInJson.Split('"');

                    if(splitted.Length > 3)
                    {
                        File.Move(splitter + nameofFile + ".json", path2 + @"\" + nameofFile + ".json");
                        try
                        {
                            File.Move(splitter + nameofFile + ".dem", path2 + @"\" + nameofFile + ".dem");
                        } catch (Exception){ }
                        try
                        {
                            File.Move(splitter + nameofFile + ".tga", path2 + @"\" + nameofFile + ".tga");
                        } catch (Exception) { }
                        Number++;
                    }
                }
                MessageBox.Show(Number.ToString() + " files were successfully moved");
                Import(path1);
                Number = 0;
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
