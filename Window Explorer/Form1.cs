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
using Microsoft.VisualBasic.FileIO; // for copy file and directory
namespace Window_Explorer
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();
            if (treeView1.SelectedNode != null)
            {
                srchBox.Text = "Search " + treeView1.SelectedNode.Text;
            }
        }
        static public string copyPath = "";
        static public string copyFileName = "";
        private void listView1_DoubleClick(object sender, MouseEventArgs e)
        {
            string _path = "";
            _path += listView1.FocusedItem.SubItems[3].Text;
            System.Diagnostics.Process.Start("explorer.exe", @_path);
        }
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            foreach (TreeNode node in e.Node.Nodes)
            {
                if (node != null)
                {
                    DirectoryInfo dirsPath = new DirectoryInfo(node.FullPath);
                    if ((dirsPath.Attributes & FileAttributes.System) != FileAttributes.System)
                    {
                        try
                        {
                            DirectoryInfo[] dirs = dirsPath.GetDirectories();
                            if (node.Nodes != null)
                            {
                                node.Nodes.Clear();
                                GetDirectories(dirs, node);
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }
    
        static bool IsDirectory(string sPath)
        {
            if( Directory.Exists(sPath) ) 
                return (File.GetAttributes(sPath) & FileAttributes.Directory) == FileAttributes.Directory;
            return false;
        }
        static bool IsFile(string sPath)
        {
            if (Directory.Exists(sPath))
                return ((File.GetAttributes(sPath) & FileAttributes.Archive) == FileAttributes.Archive );
            return false;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            PopulateDrives();
        }
        private void PopulateDrives()
        {
            try
            {
                TreeNode rootNode;
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                {
                    if (!drive.IsReady)
                    {
                        continue; // Drive is not ready, continue without executing
                    }
                    DirectoryInfo driveInfo = drive.RootDirectory;
                    rootNode = new TreeNode(driveInfo.Name);
                    rootNode.Name = driveInfo.Name;
                    rootNode.Tag = driveInfo;
                    GetDirectories(driveInfo.GetDirectories(), rootNode);
                    treeView1.Nodes.Add(rootNode); 
                }
            }
            catch (Exception)
            {
            }
        }
        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode = new TreeNode();
            try
            {
                foreach (DirectoryInfo subDir in subDirs)
                {
                    if (Directory.Exists(subDir.FullName) && ((subDir.Attributes & FileAttributes.System) != FileAttributes.System)) 
                    {
                        if (IsDirectory(subDir.FullName))
                        {
                            aNode = new TreeNode(subDir.Name,1,1); // Folder Image
                        }
                        else
                        {
                            aNode = new TreeNode(subDir.Name,0,0); // Drive Image
                            aNode.ImageKey = "drive";
                        }
                        aNode.Name = subDir.Name;
                        aNode.Tag = subDir;
                        nodeToAddTo.Nodes.Add(aNode);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        // End populating treeview.
        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;
            try
            {
                foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
                {
                    item = new ListViewItem(dir.Name, 1);
                    subItems = new ListViewItem.ListViewSubItem[]
                  {new ListViewItem.ListViewSubItem(item, "Directory"), 
                   new ListViewItem.ListViewSubItem(item, 
				dir.LastAccessTime.ToShortDateString()),
                  new ListViewItem.ListViewSubItem(item, dir.FullName)};
                    item.SubItems.AddRange(subItems);
                    item.ImageKey = "folder";
                    listView1.Items.Add(item);
                }
                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    item = new ListViewItem(file.Name, 2);
                    subItems = new ListViewItem.ListViewSubItem[]
                  { new ListViewItem.ListViewSubItem(item, "File"), 
                   new ListViewItem.ListViewSubItem(item, 
				file.LastAccessTime.ToShortDateString()),
                  new ListViewItem.ListViewSubItem(item, file.FullName)};
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
                // address bar change
                this.addressBarTextBox.Text = @e.Node.FullPath;
            }
            catch (UnauthorizedAccessException )
            {
            }
            catch (Exception)
            {
            }
        }
        private void moveToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            moveToToolStripMenuItem1_Click(sender, e);
        }


        Form2 newFolder = null;
        private void newFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                {
                    newFolder = new Form2(addressBarTextBox.Text);
                }
                newFolder.ShowDialog();
                if (!listView1.Items.ContainsKey(newFolder.textBox1.Text) && newFolder.textBox1.Text != "")
                {
                    UpdateNodeForView(treeView1.SelectedNode, Path.Combine(newFolder.textBox2.Text, newFolder.textBox1.Text), newFolder.textBox1.Text);
                }
            }
            catch (Exception)
            {
            }

        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            copyPath = listView1.FocusedItem.SubItems[3].Text;
            copyFileName = listView1.FocusedItem.SubItems[0].Text;
            pasteToolStripMenuItem.Enabled = true;
            pasteStripMenuItem.Enabled = true;
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("You are about to copy " + copyFileName + " to " + treeView1.SelectedNode.FullPath + ", are you sure ? ", "Are you sure ", MessageBoxButtons.YesNo);
            string descFile = Path.Combine(copyPath, copyFileName);
            if ( result == DialogResult.Yes)
            {
                string copiedFile = Path.Combine(treeView1.SelectedNode.FullPath, copyFileName);
                try
                {
                    if (!Directory.Exists(copiedFile) && !copiedFile.Contains(".")) //If it's a directory and it's not exist
                    {
                        CopyDirectory(copyPath, copiedFile,false);
                    }
                    else
                    {
                        File.Copy(copyPath, copiedFile);
                    }
                }
                catch (Exception)
                {
                }
                UpdateNodeForView(treeView1.SelectedNode, copiedFile, copyFileName);
            }
        }
        // This function update treeView and listView whenever there is change/changes
        private void UpdateNodeForView(TreeNode root, string path, string name)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            TreeNode newNode = new TreeNode(dir.Name);
            newNode.Name = dir.Name;
            newNode.Tag = dir;
            var attr = FileAttributes.Normal ;
            if (IsDirectory(path))
            {
                newNode.ImageKey = "folder";
                attr = FileAttributes.Directory;
            }
            else
            {
                newNode.ImageKey = "file";
                try
                {
                    attr = File.GetAttributes(path);

                }
                catch (Exception)
                {
                }
            }
            ListViewItem newNode_LV = new ListViewItem(newNode.Name); // new NodeListView
            newNode_LV.ImageKey = newNode.ImageKey;
            ListViewItem.ListViewSubItem[] newNodeSubItems_LV = new ListViewItem.ListViewSubItem[]
            {
                new ListViewItem.ListViewSubItem(newNode_LV,attr.ToString()),
                new ListViewItem.ListViewSubItem(newNode_LV, dir.LastAccessTime.ToShortDateString()),
                new ListViewItem.ListViewSubItem(newNode_LV, dir.FullName)
            };
            // Update Tree View
            treeView1.BeginUpdate();
            if (!treeView1.SelectedNode.Nodes.Contains(newNode))
            {
                treeView1.SelectedNode.Nodes.Add(newNode);
            }
            treeView1.EndUpdate();
            // Update List View
            listView1.BeginUpdate();
            newNode_LV.SubItems.AddRange(newNodeSubItems_LV);
            if (!listView1.Items.Contains(newNode_LV))
            {
                listView1.Items.Add(newNode_LV);
            }
            listView1.EndUpdate();
        }
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            FileInfo dir = new FileInfo(listView1.FocusedItem.SubItems[3].Text);
                DialogResult result = MessageBox.Show("You are about to delete " + listView1.FocusedItem.SubItems[0].Text + ", are you sure ? ", "Are you sure ", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
            {
                try
                {
                    if (File.Exists(dir.FullName) && dir.FullName.Contains(".")) // If it's a file and it's exist
                    {
                        File.Delete(dir.FullName);
                    }
                    else if (Directory.Exists(dir.FullName)) // same but with folder, we call the deleteDirectory function
                    {
                        deleteDirectory(dir.FullName);
                        Directory.Delete(dir.FullName);
                        //Directory.Delete(dir.FullName, true);
                    }
                    treeView1.SelectedNode.Nodes.RemoveByKey(dir.Name);
                    listView1.FocusedItem.Remove();
                }
                catch (Exception)
                {
                    //throw;
                }
            }
        }
        private void deleteDirectory(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles())
            {
                file.Delete(); // deleting all files
            }
            foreach (var folder in dir.GetDirectories())
            {
                //if (Directory.GetDirectories(path).Length == 0)
                //{
                //    Directory.Delete(path); // deleting all child folders
                //}
                deleteDirectory(folder.FullName);
                folder.Delete(); // deleting the top level folder, 
            }
        }
        Form2 rename = null;
        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ListViewItem liItem = listView1.GetItemAt(MousePosition.X, MousePosition.Y) as ListViewItem;
            rename = new Form2(treeView1.SelectedNode.FullPath); 
            rename.Text = "Rename :";
            if (listView1.FocusedItem != null)
            {
                rename.textBox1.Text = listView1.FocusedItem.SubItems[0].Text;
                rename.oldName = listView1.FocusedItem.SubItems[0].Text;
                rename.label1.Text = "Your new name here : ";
                rename.button1.Text = "Change";
                rename.renaming = true;
                rename.ShowDialog();
            }
            if (rename.textBox1.Text != "")
            {
                listView1.FocusedItem.Remove();
                UpdateNodeForView(treeView1.SelectedNode, Path.Combine(rename.textBox2.Text
                    , rename.textBox1.Text), rename.textBox1.Text);
            }
        }
        private void moveToToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog folderDia = new FolderBrowserDialog();
                folderDia.ShowDialog();
                string srcPath = listView1.FocusedItem.SubItems[3].Text;
                string fileName = listView1.FocusedItem.SubItems[0].Text;
                string desPath = FileSystem.CombinePath(folderDia.SelectedPath, fileName);
                DialogResult result = MessageBox.Show("You are about to move " + srcPath + " to " + desPath, "Moving ", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        if (srcPath.Contains(".") && !File.Exists(desPath))
                        {
                            File.Move(srcPath, desPath);
                        }
                        else if (!Directory.Exists(desPath))
                        {
                            Directory.CreateDirectory(desPath);
                            foreach (var file in new DirectoryInfo(srcPath).GetFiles())
                            {
                                File.Move(srcPath, desPath);
                                CopyDirectory(srcPath, desPath);
                            }
                        }
                        treeView1.SelectedNode.Nodes.RemoveByKey(fileName);
                        listView1.FocusedItem.Remove();
                    }
                    catch (Exception)
                    {
                        throw;
                    }

                };

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void CopyDirectory(string srcPath, string desPath,bool isMove = true)
        {
            Directory.CreateDirectory(desPath);
            foreach (var file in new DirectoryInfo(srcPath).GetFiles())
            {
                if (isMove)
                {
                    File.Move(srcPath, desPath);
                }
                else
                {
                    File.Copy(srcPath, desPath);
                }
                CopyDirectory(Path.Combine(srcPath, file.Name), Path.Combine(desPath, file.Name));
            }
        }
        private void newFileToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            try
            {
                Form2 newFile = null;
                newFile = new Form2(treeView1.SelectedNode.FullPath);
                newFile.Text = "Create new File ";
                newFile.label1.Text = "Your File name here ";
                newFile.textBox1.Text = "New File.txt";
                newFile.ShowDialog();
                if (!listView1.Items.ContainsKey(newFile.textBox1.Text) && newFile.textBox1.Text != "")
                {
                    UpdateNodeForView(treeView1.SelectedNode, Path.Combine(newFile.textBox2.Text, newFile.textBox1.Text), newFile.textBox1.Text);
                }
            }
            catch (Exception)
            {
            }
        }

        private void addressBarTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter ) // Check enter key and if path is not a directory
            {
                string path = addressBarTextBox.Text;
                if (IsDirectory(path) && !Directory.Exists(path))
                {
                    listView1.Clear();
                    TreeNodeMouseClickEventArgs nodeChose = new TreeNodeMouseClickEventArgs(new TreeNode(path),MouseButtons.Left,1,0,0);
                    treeView1_NodeMouseClick(sender, nodeChose );
                }
            }
        }
        
        #region Chọn kiểu hiển thị file trong listView
        private void detailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Details;
        }

        private void smallIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.SmallIcon;
        }

        private void largeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.LargeIcon; 
        }

        private void listToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.List; 
        }

        private void titleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.View = View.Tile; 
        }
        #endregion/
        private void srchBox_MouseClick(object sender, MouseEventArgs e)
        {
            if (String.Compare(srchBox.Text, "Search") == 1)
            {
                srchBox.Focus();
                srchBox.Text = "";
            }
        }
        private void srchBox_OnEnter(object sener, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
            {
                string[] resultFiles = Directory.GetFiles(treeView1.SelectedNode.FullPath, "*" + srchBox.Text + "*", System.IO.SearchOption.AllDirectories);
                string[] resultFolders = Directory.GetDirectories(treeView1.SelectedNode.FullPath, "*" + srchBox.Text + "*", System.IO.SearchOption.AllDirectories);
                listView1.Items.Clear();
                foreach (var file in resultFiles)
                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = new DirectoryInfo(file);
                    item.Text = Path.GetFileName(file);
                    if (file.Contains("."))
                    {
                        item.ImageKey = "file";
                    }
                    else
                    {
                        item.ImageKey = "folder";
                    }
                    listView1.Items.Add(item);
                }
                foreach (var folder in resultFolders)
                {
                    ListViewItem item = new ListViewItem();
                    item.Tag = new DirectoryInfo(folder);
                    item.Text = Path.GetFileName(folder);
                    if (folder.Contains("."))
                    {
                        item.ImageKey = "file";
                    }
                    else
                    {
                        item.ImageKey = "folder";
                    }
                    listView1.Items.Add(item);
                }
            }

        }
        #region ToolStripItem
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteToolStripMenuItem1_Click(sender, e);
        }

        private void copyStripMenuItem_Click(object sender, EventArgs e)
        {
            copyToolStripMenuItem_Click(sender, e);
            pasteStripMenuItem.Enabled = true;
            pasteToolStripMenuItem.Enabled = true;
        }

        private void pasteStripMenuItem_Click(object sender, EventArgs e)
        {
            pasteToolStripMenuItem_Click(sender, e);
        }

        private void newFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFileToolStripMenuItem1_Click(sender, e);
        }

        #endregion
    }
}
