using ChatLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class AddContact : Form
    {
        public string ContactName { get; private set; }
        public int Index { get; private set; }
        bool isUserContacts;

        public AddContact(string[] names, bool isUserContacts)
        {
            InitializeComponent();
            listBox1.Items.AddRange(names);
            if (!isUserContacts)
            {
                label1.Visible = false;
                textBox1.Visible = false;
                this.Height -= 40;
            }
            this.isUserContacts = isUserContacts;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ContactName = isUserContacts ? textBox1.Text : listBox1.SelectedItem.ToString();
            if (listBox1.SelectedIndex != -1)
                Index = listBox1.SelectedIndex;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
