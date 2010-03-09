using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.src.Controllers
{
    public class Touch
    {
        public int Id;
        public double X, Y;
        public bool IsNew;
    }

    public class TouchDictionnary : Dictionary<int, Touch> { }
    public class TouchList : List<Touch> 
    {
        public bool ContainsId(int id)
        {
            foreach (Touch t in this)
            {
                if (t.Id == id)
                {
                    return true;
                }
            }
            return false;
        }

        public Touch GetById(int id)
        {
            foreach (Touch t in this)
            {
                if (t.Id == id)
                {
                    return t;
                }
            }
            return null;
        }

        public TouchList NewTouches
        {
            get
            {
                TouchList result = new TouchList();
                foreach (Touch t in this)
                {
                    if (t.IsNew)
                    {
                        result.Add(t);
                    }
                }
                return result;
            }
        }
    }
}
