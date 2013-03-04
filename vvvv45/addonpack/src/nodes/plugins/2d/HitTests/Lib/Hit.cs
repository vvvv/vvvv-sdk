using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Lib
{
    public class Hit
    {
        public Hit(int point, int obj)
        {
            this.PointId = point;
            this.ObjectId = obj;
        }

        public int ObjectId;
        public int PointId;
    }
}
