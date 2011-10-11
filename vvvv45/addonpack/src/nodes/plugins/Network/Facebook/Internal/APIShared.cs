using System;
using System.Collections.Generic;
using System.Text;
using Facebook.API;

namespace vvvv.Nodes.Internal
{
    public class APIShared
    {
        private static FacebookAPI api = new FacebookAPI();

        public static FacebookAPI API
        {
            get { return api; }
        }
    }
}
