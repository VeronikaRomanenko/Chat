using ChatLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Server
    {
        ChatEntities1 chatEntities;
        Dictionary<int, IPEndPoint> keyValues;
        int port = 1025;

        public Server()
        {
            keyValues = new Dictionary<int, IPEndPoint>();
            chatEntities = new ChatEntities1();
            BeginListenTcp(new TcpListener(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1024)));
        }

        private async void BeginListenTcp(TcpListener listener)
        {
            await Task.Run(() =>
            {
                listener.Start();
                while (true)
                {
                    if (listener.Pending())
                    {                        
                        Processing(listener.AcceptTcpClient());
                    }
                }
            });
        }

        private async void Processing(TcpClient tcpC)
        {
            await Task.Run(() =>
            {
                NetworkStream ns = tcpC.GetStream();
                byte[] buffer = new byte[tcpC.ReceiveBufferSize];
                ns.Read(buffer, 0, buffer.Length);
                string str = Encoding.ASCII.GetString(buffer);
                string[] tmp = str.Split(';');
                byte[] buf = new byte[1024];
                if (tmp[0].IndexOf("login") == 0)
                {
                    User user = Login(tmp[1], tmp[2]);
                    if (user == null)
                    {
                        buffer = Encoding.ASCII.GetBytes(" ");
                    }
                    else
                    {
                        user.port = port;
                        keyValues[chatEntities.Users.Where(x => x.UserLogin == user.Login).FirstOrDefault().Id] = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
                        port++;
                        BinaryFormatter binF = new BinaryFormatter();
                        using (MemoryStream stream = new MemoryStream())
                        {
                            binF.Serialize(stream, user); stream.Flush();
                            buffer = stream.ToArray();
                        }
                    }
                }
                else if (tmp[0].IndexOf("getLogins") == 0)
                {
                    buffer = Encoding.ASCII.GetBytes(GetAllLogins());
                }
                else if (tmp[0].IndexOf("addChat") == 0)
                {
                    AddChat(tmp[1]);
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                else if (tmp[0].IndexOf("deleteChat") == 0)
                {
                    DeleteChat(tmp[2], int.Parse(tmp[1]));
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                else if (tmp[0].IndexOf("addContactUser") == 0 || tmp[0].IndexOf("editContactUser") == 0)
                {
                    EditContactUser(tmp[3], int.Parse(tmp[1]), tmp[2]);
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                else if (tmp[0].IndexOf("deleteContactUser") == 0)
                {
                    DeleteContactUser(tmp[2], int.Parse(tmp[1]));
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                else if (tmp[0].IndexOf("deleteContactChat") == 0)
                {
                    DeleteContactChat(tmp[3], int.Parse(tmp[1]), int.Parse(tmp[2]));
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                else
                {
                    object obj;
                    BinaryFormatter binF = new BinaryFormatter();
                    using (MemoryStream ms = new MemoryStream(buffer))
                    {
                        obj = binF.Deserialize(ms);
                    }
                    if (obj is Contact)
                    {
                        AddContactChat(obj as Contact);
                    }
                    else if (obj is Message)
                    {
                        SendMessage(obj as Message);
                    }
                    buffer = Encoding.ASCII.GetBytes(" ");
                }
                ns.Write(buffer, 0, buffer.Length);
            });
        }

        private User Login(string login, string password)
        {
            List<Chat> chats = new List<Chat>();
            List<Contact> contacts = new List<Contact>();
            string log = null;
            foreach (var item in chatEntities.Users)
            {
                if (item.UserLogin == login)
                {
                    if (item.UserPassword == password)
                    {
                        log = login;
                        foreach (var contact in item.Contacts1)
                        {
                            contacts.Add(new Contact(contact.ContactName, (int)contact.IdContact));
                        }
                        foreach (var chat in item.UserChat)
                        {
                            List<Contact> contacts1 = new List<Contact>();
                            List<Message> messages = new List<Message>();
                            foreach (var contact in chat.Chats.UserChat)
                            {
                                if (contact.Users.UserLogin != log)
                                {
                                    Contact contact1 = null;
                                    foreach (var item1 in contacts)
                                    {
                                        if (item1.IdUser == contact.IdUser)
                                        {
                                            contact1 = item1;
                                            break;
                                        }
                                    }
                                    if (contact1 == null)
                                    {
                                        contact1 = new Contact(contact.Users.UserLogin, (int)contact.IdUser);
                                    }
                                    contacts1.Add(contact1);
                                }
                            }
                            foreach (var message in chat.Chats.ChatMessages)
                            {                               
                                messages.Add(new Message(message.MessageText));
                            }
                            chats.Add(new Chat(contacts1, messages));
                        }
                    }
                    else
                        return null;
                }
            }
            if (log == null)
            {
                log = login;
                chatEntities.Users.Add(new Users() { UserLogin = log, UserPassword = password });
                chatEntities.SaveChanges();
            }
            return new User(log, chats, contacts);
        }

        private string GetAllLogins()
        {
            string res = "";
            foreach (var item in chatEntities.Users)
            {
                res += (item.UserLogin + ";");
            }
            return res;
        }

        private void AddChat(string login)
        {
            Chats chats = new Chats();
            chatEntities.Chats.Add(chats);
            chatEntities.UserChat.Add(new UserChat()
            {
                IdUser = chatEntities.Users.Where(x => x.UserLogin == login).FirstOrDefault().Id,
                IdChat = chats.Id
            });
            chatEntities.SaveChanges();
        }

        private void DeleteChat(string login, int chatIndex)
        {
            int userId = chatEntities.Users.Where(x => x.UserLogin == login).FirstOrDefault().Id;
            UserChat userChat = chatEntities.UserChat.Where(x => x.IdUser == userId).OrderBy(x => x.Id).Skip(chatIndex).Take(1).FirstOrDefault();
            chatEntities.UserChat.Remove(userChat);
            Chats chats = userChat.Chats;
            if (chats.UserChat.Count == 0)
            {
                chatEntities.Chats.Remove(chats);
            }
            chatEntities.SaveChanges();
        }

        private void EditContactUser(string login, int contId, string newName)
        {
            int contactId = chatEntities.Users.Where(x => x.UserLogin != login).OrderBy(x => x.Id).Skip(contId - 1).Take(1).FirstOrDefault().Id;
            int userId = chatEntities.Users.Where(x => x.UserLogin == login).FirstOrDefault().Id;
            if (chatEntities.Contacts.Where(x => x.IdContact == contactId && x.IdUser == userId).Count() > 0)
                chatEntities.Contacts.Where(x => x.IdContact == contactId && x.IdUser == userId).FirstOrDefault().ContactName = newName;
            else
                chatEntities.Contacts.Add(new Contacts { ContactName = newName, IdContact = contactId, IdUser = userId });
            chatEntities.SaveChanges();
        }

        private void DeleteContactUser(string login, int index)
        {
            int userId = chatEntities.Users.Where(x => x.UserLogin == login).FirstOrDefault().Id;
            chatEntities.Contacts.Remove(chatEntities.Contacts.Where(x => x.IdUser == userId).OrderBy(x => x.Id).Skip(index).Take(1).FirstOrDefault());
            chatEntities.SaveChanges();
        }

        private void DeleteContactChat(string login, int indexChat, int indexContact)
        {
            int userId = chatEntities.Users.Where(x => x.UserLogin == login).FirstOrDefault().Id;
            Chats chats = chatEntities.UserChat.Where(x => x.IdUser == userId).OrderBy(x => x.Id).Skip(indexChat).Take(1).FirstOrDefault().Chats;
            UserChat userChat = chats.UserChat.Where(x => x.IdUser != userId).OrderBy(x => x.Id).Skip(indexContact).Take(1).First();
            chatEntities.UserChat.Remove(userChat);
            chatEntities.SaveChanges();
        }

        private void AddContactChat(Contact c)
        {
            int userId = chatEntities.Users.Where(x => x.UserLogin == c.Login).FirstOrDefault().Id;
            int idChat = chatEntities.UserChat.Where(x => x.IdUser == userId).OrderBy(x => x.Id).Skip(c.IdChat).Take(1).FirstOrDefault().Chats.Id;
            chatEntities.UserChat.Add(new UserChat { IdChat = idChat, IdUser = c.IdUser });
            chatEntities.SaveChanges();
        }

        private void SendMessage(Message m)
        {
            int userId = chatEntities.Users.Where(x => x.UserLogin == m.Login).FirstOrDefault().Id;
            Chats chats = chatEntities.UserChat.Where(x => x.IdUser == userId).OrderBy(x => x.Id).Skip(m.ChatIndex).Take(1).FirstOrDefault().Chats;
            ChatMessages chatMessages = new ChatMessages { MessageText = m.Text };
            chats.ChatMessages.Add(chatMessages);
            chatEntities.SaveChanges();
            foreach (var item in chats.UserChat)
            {
                if (item.IdUser != userId)
                {
                    var userChats = item.Users.UserChat;
                    int i = 0;
                    foreach (var item1 in userChats)
                    {
                        if (item1.IdChat == item.IdChat)
                            break;
                        i++;
                    }
                    m.ChatIndex = i;
                    byte[] buffer;
                    UdpClient udpClient = new UdpClient();
                    BinaryFormatter binF = new BinaryFormatter();
                    using (MemoryStream stream = new MemoryStream())
                    {
                        binF.Serialize(stream, m); stream.Flush();
                        buffer = stream.ToArray();
                    }
                    udpClient.Send(buffer, buffer.Length, keyValues[(int)item.IdUser]);
                }
            }
        }
    }
}