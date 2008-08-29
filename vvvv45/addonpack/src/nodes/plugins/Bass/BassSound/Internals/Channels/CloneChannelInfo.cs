using System;
using System.Collections.Generic;
using System.Text;

namespace BassSound.Internals
{
    public class CloneChannelInfo : ChannelInfo
    {
        private ChannelInfo parent;

        public ChannelInfo Parent
        {
            get { return parent; }
            set { parent = value; }
        }

        public override void Initialize(int deviceid)
        {
            //No nothing on this one
        }

        protected override void OnLoopEndUpdated()
        {
            
        }
    }
}
