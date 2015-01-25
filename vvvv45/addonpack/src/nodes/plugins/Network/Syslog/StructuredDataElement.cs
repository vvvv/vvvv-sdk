using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace VVVV.Nodes.Syslog
{
    public class StructuredDataElement
    {
        public string ID { get; set; }

        public NameValueCollection Properties { get; private set; }

        public StructuredDataElement()
        {
            Properties = new NameValueCollection();
        }

        public StructuredDataElement(string id)
            : this()
        {
            ID = id;
        }


        public int Count { get { return this.Properties.Count; } }
        public string this[string name]
        {
            get { return this.Properties[name]; }
            set { this.Properties[name] = value; }
        }
        public string[] GetValues(string name)
        {
            return this.Properties.GetValues(name);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            new Format.IetfSyslogFormat().GetStructuredDataString(this, sb);
            return sb.ToString();
        }
    }
}
