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
    public partial class Contacts : Form
    {
        Client client;
        int chatIndex;

        public Contacts(Client client, int chatIndex)
        {
            InitializeComponent();
            this.client = client;
            List<Contact> contacts = new List<Contact>();
            if (chatIndex == -1)
                contacts = client.user.Contacts;
            else
            {
                contacts = client.user.Chats[chatIndex].Contacts;
                btnEdit.Visible = false;
            }
            foreach (Contact contact in contacts)
            {
                listBox1.Items.Add(contact.Name);
            }
            this.chatIndex = chatIndex;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string[] names;
            if (chatIndex == -1) names = client.GetLogins();
            else
            {
                names = new string[client.user.Contacts.Count];
                for (int i = 0; i < names.Length; i++)
                {
                    names[i] = client.user.Contacts[i].Name;
                }
            }
            AddContact form = new AddContact(names, (chatIndex == -1) ? true : false);
            if (form.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Add(form.ContactName);
                if (chatIndex == -1) client.AddContactUser(form.Index, form.ContactName);
                else client.AddContactChat(chatIndex, client.user.Contacts[form.Index]);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            if (chatIndex == -1) client.DeleteContactUser(listBox1.SelectedIndex);
            else client.DeleteContactChat(chatIndex, listBox1.SelectedIndex);
            listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            EditContact form = new EditContact(listBox1.SelectedItem.ToString());
            if (form.ShowDialog() == DialogResult.OK)
            {
                client.EditContactUser(listBox1.SelectedIndex, form.ContactName);
                listBox1.Items[listBox1.SelectedIndex] = form.ContactName;
            }
        }
    }
}
