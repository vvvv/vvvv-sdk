using System;
using System.Collections.Generic;
using System.Text;

namespace vvvv.Utils
{
    public class MoCapDataList : Dictionary<string, MoCapDataItem>
    {

    }

    public class MoCapDataItem
    {
        private MOCAP_ITEM item;
        private string name;
        private string description;

        public MOCAP_ITEM Item
        {
            get { return item; }
            set { item = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

    }
}
