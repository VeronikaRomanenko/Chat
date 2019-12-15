using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ChatLibrary
{
    public class Client
    {
        static object obj = new object();
        
        public User user { get; private set; }
        IPEndPoint endPoint; IPEndPoint udpPoint;
        TcpClient tcpClient;

        public Client(string login, string password)
        {
            endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024);
            byte[] buffer = WorkWithServer(Encoding.ASCII.GetBytes("login;" + login + ";" + password));
            if (Encoding.ASCII.GetString(buffer) == " ")
            {
                user = null;
                return;
            }
            BinaryFormatter binF = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(buffer))
            {
                ms.Position = 0;
                user = binF.Deserialize(ms) as User;
            }
            udpPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), user.port);
            WaitMessages();
        }

            private byte[] WorkWithServer(byte[] message)
        {
            lock (obj)
            {
                try
                {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(endPoint);
                    if (tcpClient.Connected)
                    {
                        NetworkStream ns = tcpClient.GetStream();
                        ns.Write(message, 0, message.Length);
                    }
                }
                catch { tcpClient.Close(); return null; }
                byte[] buffer;
                try
                {
                    while (true)
                    {
                        Thread.Sleep(1000);
                        if (tcpClient.Available != 0)
                        {
                            buffer = new byte[tcpClient.Available];
                            NetworkStream ns = tcpClient.GetStream();
                            ns.Read(buffer, 0, buffer.Length);
                            tcpClient.Close();
                            return buffer;
                        }
                    }
                }
                catch { tcpClient.Close(); return null; }
            }
        }

        public string[] GetLogins()
        {
            string tmp = Encoding.ASCII.GetString(WorkWithServer(Encoding.ASCII.GetBytes("getLogins")));
            List<string> logins = new List<string>();
            foreach (string item in tmp.Split(';'))
            {
                if (item != user.Login)
                    logins.Add(item);
            }
            return logins.ToArray();
        }

        public void AddChat()
        {
            Chat chat = new Chat(new List<Contact>(), new List<Message>());
            user.Chats.Add(chat);
            WorkWithServer(Encoding.ASCII.GetBytes("addChat;" + user.Login));
        }

        public void DeleteChat(int index)
        {
            user.Chats.RemoveAt(index);
            WorkWithServer(Encoding.ASCII.GetBytes("deleteChat;" + index + ";" + user.Login));
        }

        public void AddContactUser(int userId, string newName)
        {
            user.Contacts.Add(new Contact(newName, userId));
            WorkWithServer(Encoding.ASCII.GetBytes("addContactUser;" + (userId + 1) + ";" + newName + ";" + user.Login));
        }

        public void EditContactUser(int index, string newName)
        {
            user.Contacts[index].Name = newName;
            WorkWithServer(Encoding.ASCII.GetBytes("editContactUser;" + (index + 1) + ";" + newName + ";" + user.Login));
        }

        public void DeleteContactUser(int index)
        {
            user.Contacts.RemoveAt(index);
            WorkWithServer(Encoding.ASCII.GetBytes("deleteContactUser;" + index + ";" + user.Login));
        }

        public void AddContactChat(int index, Contact contact)
        {
            user.Chats[index].Contacts.Add(contact);
            contact.IdChat = index;
            contact.Login = user.Login;
            BinaryFormatter binF = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                binF.Serialize(stream, contact);
                WorkWithServer(stream.ToArray());
            }
        }

        public void DeleteContactChat(int index, int indexContact)
        {
            user.Chats[index].Contacts.RemoveAt(indexContact);
            WorkWithServer(Encoding.ASCII.GetBytes("deleteContactChat;" + index + ";" + indexContact + ";" + user.Login));
        }

        public void SendMessage(int index, Message message)
        {
            user.Chats[index].Messages.Add(message);
            message.Login = user.Login;
            BinaryFormatter binF = new BinaryFormatter();
            using (MemoryStream stream = new MemoryStream())
            {
                binF.Serialize(stream, message);
                WorkWithServer(stream.ToArray());
            }
        }

        public async void WaitMessages()
        {
            await Task.Run(() =>
            {
                UdpClient udpClient = new UdpClient(udpPoint);
                while (true)
                {
                    IPEndPoint ip = null;
                    byte[] buffer = udpClient.Receive(ref ip);
                    BinaryFormatter binF = new BinaryFormatter();
                    Message message;
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        message = binF.Deserialize(ms) as Message;
                    }
                    user.Chats[message.ChatIndex].Messages.Add(message);
                }
            });
        }
    }
}