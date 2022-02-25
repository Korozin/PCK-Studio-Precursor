using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinecraftUSkinEditor
{
    public partial class LOCEditor : Form
    {
        public class displayId
        {
            public string id;
            public string defaultName;
        }

        public LOCEditor(LOC loc)
        {
            InitializeComponent();

            currentLoc = loc;
            tbl = new DataTable();
            tbl.Columns.Add(new DataColumn("Language") { ReadOnly = true });
            tbl.Columns.Add("Display Name");
            dataGridView1.DataSource = tbl;
        }
        DataTable tbl;
        LOC currentLoc;

        private void LOCEditor_Load(object sender, EventArgs e)
        {
            foreach(string id in currentLoc.ids.names)
                treeView1.Nodes.Add(id);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            tbl.Rows.Clear();

            foreach (LOC.Language l in currentLoc.langs)
                tbl.Rows.Add(l.name, l.names[e.Node.Index]);
        }

        private void addDisplayIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            displayId d = new displayId();
            (new DisplayIdPrompt(d)).ShowDialog();

            currentLoc.ids.names.Add(d.id);

            foreach(LOC.Language l in currentLoc.langs)
                l.names.Add(d.defaultName);

            treeView1.Nodes.Add(d.id);
        }

        private void deleteDisplayIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(treeView1.SelectedNode != null)
            {
                int index = treeView1.SelectedNode.Index;

                currentLoc.ids.names.RemoveAt(index);

                foreach (LOC.Language l in currentLoc.langs)
                    l.names.RemoveAt(index);

                treeView1.Nodes.RemoveAt(index);
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            for(int i = 0; i < tbl.Rows.Count; i++)
            {
                currentLoc.langs[i].names[treeView1.SelectedNode.Index] = (string)tbl.Rows[i][1];
            }             
        }
    }
}
