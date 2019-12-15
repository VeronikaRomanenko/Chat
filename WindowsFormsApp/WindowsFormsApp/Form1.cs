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
    public partial class Form1 : Form
    {
        Client client;

        public Form1()
        {
            InitializeComponent();
            Login form = new Login();
            if (form.ShowDialog() == DialogResult.OK)
            {
                client = form.client;
            }
            foreach (Chat chat in client.user.Chats)
            {
                string str = "Чат с ";
                foreach (Contact contact in chat.Contacts)
                {
                    str += $"{contact.Name} ";
                }
                listBox1.Items.Add(str);
            }
        }

        private void btnContacts_Click(object sender, EventArgs e)
        {
            Contacts form = new Contacts(client, -1);
            form.Show();
        }

        private void listBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            Chats form = new Chats(client, listBox1.SelectedIndex);
            form.Show();
        }

        private async void btnAddChat_Click(object sender, EventArgs e)
        {
            await Task.Run(() =>
            {
                client.AddChat();
                InvokeChats(-1);
            });
        }

        private async void btnDeleteChat_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex == -1)
                return;

            await Task.Run(() =>
            {
                client.DeleteChat(listBox1.SelectedIndex);
                InvokeChats(listBox1.SelectedIndex);
            });
        }

        private void InvokeChats(int num)
        {
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new Action<int>(InvokeChats), num);
            else
            {
                if (num == -1)
                    listBox1.Items.Add("Чат с ");
                else
                    listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }
    }
}
