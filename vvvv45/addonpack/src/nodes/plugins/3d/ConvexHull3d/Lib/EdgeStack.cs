using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace ConvexHull3d.Lib
{
    public class EdgeStack
    {
        private List<Edge3d> data; // contents of the stack
        public EdgeStack()
        {
            data = new List<Edge3d>();
        }

        public bool isEmpty()
        {
            return data.Count ==0;
        }

        public Edge3d get()
        {
            Edge3d res = data[0];
            data.RemoveAt(0);
            return res;
        }

        public void put(Edge3d e)
        {
            data.Insert(0,e);
        }

        public void put(Point3d a, Point3d b)
        {
            put(new Edge3d(a, b));
        }

        
        public void putp(Edge3d e)
        {
            int ind = data.IndexOf(e);
            if (ind == -1)
            {
                data.Insert(0,e);
            }
            else
            {
                data.RemoveAt(ind);
            }
        }

        public void putp(Point3d a, Point3d b)
        {
            putp(new Edge3d(a, b));
        }
    }
}
