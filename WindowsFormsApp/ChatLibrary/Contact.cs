using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatLibrary
{
    [Serializable]
    public class Contact
    {
        public string Name { set; get; }
        public int IdUser { get; }
        public int IdChat { set; get; }
        public string Login { set; get; }

        public Contact(string name, int idUser)
        {
            Name = name;
            IdUser = idUser;
        }
    }
}