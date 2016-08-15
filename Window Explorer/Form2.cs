using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.VisualBasic.FileIO;
namespace Window_Explorer
{
    public partial class Form2 : Form
    {
        public string oldName = "";
        public bool renaming = false;
        public Form2(string textBox = "")
        {
            InitializeComponent();
            textBox1.Text = "New folder";
            textBox2.Text = textBox;
            //oldName = textBox;
        }
        static bool IsDirectory(string sPath)
        {
            if (Directory.Exists(sPath))
                return (File.GetAttributes(sPath) & FileAttributes.Directory) == FileAttributes.Directory;
            return false;
        }
        static bool IsFile(string sPath)
        {
            if (Directory.Exists(sPath))
                return ((File.GetAttributes(sPath) & FileAttributes.Archive) == FileAttributes.Archive);
            return false;
        }
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(textBox2.Text,textBox1.Text);
            string newName = "";
            if (!renaming)
            {
                if (DialogResult.Yes == MessageBox.Show("Do you want to create " + textBox1.Text
            + " in " + textBox2.Text + " ? ","Warning", MessageBoxButtons.YesNo))
                {
                    if (!path.Contains(".") )  
                    {
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                            newName = new DirectoryInfo(path).Name;
                        }
                        else
                        {
                            int num = 1;
                            while (Directory.Exists(path)) // Check if Dir is exist and increase the number of its name
                            {
                                if (path.Contains("("))
                                {
                                    path = path.Remove(path.Length - 3);
                                }
                                string newPath = path + "(" + num + ")";
                                path = newPath;
                                num++;
                            }
                            Directory.CreateDirectory(path);
                            newName = new DirectoryInfo(path).Name;
                        }
                    }
                    else if (path.Contains(".") ) 
                    {
                        if (!File.Exists(path))
                        {
                            File.Create(path);
                            newName = new DirectoryInfo(path).Name;
                        }
                        else
                        {
                            int num = 1;
                            while (File.Exists(path)) // Same as above but we have to separate the extension from the file name
                            {
                                string EXTension = "";
                                EXTension = path.Substring(path.LastIndexOf("."));
                                path = path.Remove(path.LastIndexOf("."));
                                int diff = path.Length - path.LastIndexOf("(");
                                if (diff == 3)
                                {
                                   path = path.Remove(path.Length - 3);
                                }
                                string newPath = path + "(" + num + ")" + EXTension;
                                path = newPath; 
                                num++;
                            }
                            File.Create(path);
                            newName = new DirectoryInfo(path).Name;
                        }
                    }
                    textBox1.Text = newName;
                    this.Close();
                } 
            }
            else if(renaming)
            {
                if (DialogResult.Yes == MessageBox.Show("Do you want to change " + oldName + " to " + textBox1.Text,"Warning ", MessageBoxButtons.YesNo))
                {
                    oldName = Path.Combine(textBox2.Text, oldName);
                    if (!path.Contains(".")) 
                    {
                        Directory.Move(oldName,path);
                    }
                    else if (path.Contains(".")) 
                    {
                        File.Move(oldName,path);
                    }
                    this.Close();
                }
                else if (this.textBox1.Text == "")
                {
                    this.Close();
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                button1_Click(sender, e);
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
