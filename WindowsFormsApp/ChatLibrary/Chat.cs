using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    [Serializable]
    public class Chat
    {
        public List<Contact> Contacts { set; get; }
        public List<Message> Messages { set; get; }

        public Chat(List<Contact> contacts, List<Message> messages)
        {
            Contacts = contacts;
            Messages = messages;
        }
    }
}