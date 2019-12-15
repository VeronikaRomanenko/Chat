using ChatLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Chats : Form
    {
        Client client;
        int chatIndex;

        public Chats(Client client, int index)
        {
            InitializeComponent();
            this.client = client;
            chatIndex = index;
            foreach (ChatLibrary.Message message in client.user.Chats[index].Messages)
            {
                AddMessageInListBox(message);
            }
            WaitMessages();
        }

        private void AddMessageInListBox(ChatLibrary.Message message)
        {
            if (lbxMessages.InvokeRequired)
                lbxMessages.Invoke(new Action<ChatLibrary.Message>(AddMessageInListBox), message);
            else
            {
                lbxMessages.Items.Add(message.Text);
            }
        }

        private async void WaitMessages()
        {
            await Task.Run(() =>
            {
                while (true)
                {
                    for (int i = lbxMessages.Items.Count; i < client.user.Chats[chatIndex].Messages.Count; i++)
                    {
                        AddMessageInListBox(client.user.Chats[chatIndex].Messages[i]);
                    }
                }
            });
        }

        private async void btnSend_Click(object sender, EventArgs e)
        {
            string textMessage = textBox1.Text;
            textBox1.Text = "";
            await Task.Run(() =>
            {
                ChatLibrary.Message message = new ChatLibrary.Message(textMessage, client.user.Login, chatIndex);
                AddMessageInListBox(message);
                client.SendMessage(chatIndex, message);
            });
        }

        private void contactsManagingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Contacts form = new Contacts(client, chatIndex);
            form.ShowDialog();
        }
    }
}