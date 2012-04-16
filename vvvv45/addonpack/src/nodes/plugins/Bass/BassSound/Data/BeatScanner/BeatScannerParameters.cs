using System;
using System.Collections.Generic;
using System.Text;

namespace BassSound.Data.BeatScanner
{
    public class BeatScannerParameters
    {
        private int index;
        private float width = 10;
        private float center = 90;
        private float release = 20;

        public int Index
        {
            get { return index; }
            set { index = value; }
        }

        public float Width
        {
            get { return width; }
            set { width = value; }
        }

        public float Center
        {
            get { return center; }
            set { center = value; }
        }

        public float Release
        {
            get { return release; }
            set { release = value; }
        }
    }
}
