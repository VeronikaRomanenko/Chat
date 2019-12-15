using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    [Serializable]
    public class Message
    {
        public string Text { get; }
        public int ChatIndex { set; get; }
        public string Login { set; get; }

        public Message(string text, string login, int chatIndex)
        {
            ChatIndex = chatIndex;
            Text = login + ": " + text + "          " + DateTime.Now;
        }

        public Message(string text)
        {
            Text = text;
        }
    }
}
