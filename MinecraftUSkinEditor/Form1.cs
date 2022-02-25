using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MinecraftUSkinEditor
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        PCK currentPCK;

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.CheckFileExists = true;
                ofd.Filter = "PCK (Minecraft Wii U Package)|*.pck";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    treeView1.Nodes.Clear();
                    PCK pck = new PCK(ofd.FileName);
                    currentPCK = pck;

                    foreach (PCK.MineFile mf in pck.mineFiles)
                    {
                        treeView1.Nodes.Add(new TreeNode(mf.name) { Tag = mf });
                    }
                } 
            }
        }

        private void selectNode(object sender, TreeViewEventArgs e)
        {
            if(e.Node.Tag is string)
                label1.Text = (string)e.Node.Tag;
            else if(e.Node.Tag is PCK.MineFile)
            {
                PCK.MineFile mf = (PCK.MineFile)e.Node.Tag;
                label1.Text = ""+mf.type+" "+mf.filesize+"\n";
                foreach(object[] entry in mf.entries)
                    label1.Text += "" + entry[0] + " " + entry[1] + "\n";

                if(Path.GetExtension(mf.name) == ".png")
                {
                    pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    pictureBox1.InterpolationMode = InterpolationMode.NearestNeighbor;
                    MemoryStream png = new MemoryStream(mf.data);
                    pictureBox1.Image = Image.FromStream(png);
                }
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode.Tag is PCK.MineFile)
            {
                string appPath = Application.StartupPath;
                string extractPath = Path.Combine(appPath, ((PCK.MineFile)treeView1.SelectedNode.Tag).name);

                if (!String.IsNullOrWhiteSpace(Path.GetDirectoryName(extractPath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(extractPath));
                File.WriteAllBytes(extractPath, ((PCK.MineFile)treeView1.SelectedNode.Tag).data);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < treeView1.Nodes.Count; i++)
                currentPCK.mineFiles[i].name = treeView1.Nodes[i].Text;

            using (var ofd = new SaveFileDialog())
            {
                ofd.Filter = "PCK (Minecraft Wii U Package)|*.pck";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllBytes(ofd.FileName, currentPCK.Rebuild());
                }
            }
        }

        private void replaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is PCK.MineFile)
            {
                PCK.MineFile mf = (PCK.MineFile)treeView1.SelectedNode.Tag;
                using (var ofd = new OpenFileDialog())
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        mf.data = File.ReadAllBytes(ofd.FileName);
                        mf.filesize = mf.data.Length;
                    }
                }
            }
        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            //Does not work as intended. Renaming moved to save function
        }

        private void deleteFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is PCK.MineFile)
            {
                PCK.MineFile mf = (PCK.MineFile)treeView1.SelectedNode.Tag;
                treeView1.Nodes.Remove(treeView1.SelectedNode);
                currentPCK.mineFiles.Remove(mf);
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    PCK.MineFile mf = new PCK.MineFile();
                    mf.data = File.ReadAllBytes(ofd.FileName);
                    mf.filesize = mf.data.Length;
                    mf.name = Path.GetFileName(ofd.FileName);
                    mf.type = 0;
                    currentPCK.mineFiles.Add(mf);
                    treeView1.Nodes.Add(new TreeNode(mf.name) { Tag = mf });
                }
            }
        }

        private void editEntryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is PCK.MineFile)
            {
                PCK.MineFile mf = (PCK.MineFile)treeView1.SelectedNode.Tag;
                (new EntryEditor(currentPCK.types, mf)).ShowDialog();
                treeView1.SelectedNode = treeView1.SelectedNode; //Jank refresh code 
            }
        }

        private void editAsLocToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is PCK.MineFile)
            {
                LOC l;
                PCK.MineFile mf = (PCK.MineFile)treeView1.SelectedNode.Tag;

                //l = new LOC(mf.data);

                try
                {
                    l = new LOC(mf.data);
                }
                catch
                {
                    MessageBox.Show("No localization data found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                (new LOCEditor(l)).ShowDialog();
                mf.data = l.Rebuild();
            }
        }
    }
}
