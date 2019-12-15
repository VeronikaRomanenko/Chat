using ChatLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class Login : Form
    {
        public Client client { get; private set; }

        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("Логин и пароль не могут быть пустыми!");
                return;
            }
            if (textBox1.Text.Contains(';') || textBox2.Text.Contains(';') || textBox1.Text.Contains(',') || textBox2.Text.Contains(','))
            {
                MessageBox.Show("Логин и пароль не могут содержать символ ';'");
                return;
            }
            Task task = new Task(() =>
            {
                client = new Client(textBox1.Text, textBox2.Text);
            });
            task.Start();
            task.Wait();
            if (client.user == null || client == null)
            {
                MessageBox.Show("Ошибка входа или регистрации");
                return;
            }
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}