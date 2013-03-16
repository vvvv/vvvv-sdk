using System;
using System.Collections.Generic;
using System.Text;

namespace FaceBookAPI.Internal
{
    public class UserLink
    {
        private string u1;
        private string u2;
        private int index1;
        private int index2;

        public string User1
        {
            get { return u1; }
            set { u1 = value; }
        }

        public string User2
        {
            get { return u2; }
            set { u2 = value; }
        }

        public int Index1
        {
            get { return index1; }
            set { index1 = value; }
        }

        public int Index2
        {
            get { return index2; }
            set { index2 = value; }
        }
    }

    public class UserLinkList : List<UserLink>
    {
        public bool Contains(string u1, string u2)
        {
            foreach (UserLink ul in this)
            {
                if ((ul.User1 == u1 && ul.User2 == u2) || (ul.User1 == u2 && ul.User2 == u1))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
