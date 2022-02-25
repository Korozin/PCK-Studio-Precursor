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
    public partial class DisplayIdPrompt : Form
    {
        public DisplayIdPrompt(LOCEditor.displayId d)
        {
            InitializeComponent();
            disp = d;
        }

        LOCEditor.displayId disp;

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            disp.id = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            disp.defaultName = textBox2.Text;
        }
    }
}
