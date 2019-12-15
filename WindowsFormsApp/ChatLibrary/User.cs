using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    [Serializable]
    public class User
    {
        public string Login { get; }
        public List<Chat> Chats { set; get; }
        public List<Contact> Contacts { set; get; }
        public int port;

        public User(string login, List<Chat> chats, List<Contact> contacts)
        {
            Login = login;
            Chats = chats;
            Contacts = contacts;
        }
    }
}