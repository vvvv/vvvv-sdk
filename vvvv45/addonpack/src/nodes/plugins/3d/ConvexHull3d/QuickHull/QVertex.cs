/**
  * Copyright John E. Lloyd, 2004. All rights reserved. Permission to use,
  * copy, modify and redistribute is granted, provided that this copyright
  * notice is retained and the author is given credit whenever appropriate.
  *
  * This  software is distributed "as is", without any warranty, including 
  * any implied warranty of merchantability or fitness for a particular
  * use. The author assumes no responsibility for, and shall not be liable
  * for, any special, indirect, or consequential damages, or any damages
  * whatsoever, arising out of or in connection with the use of this
  * software.
  */

using System;
using System.Collections.Generic;
using System.Text;

namespace ConvexHull3d.QuickHull
{
    public class Vertex
    {
        /**
         * Spatial point associated with this vertex.
         */
        internal Point3d pnt;

        /**
         * Back index into an array.
         */
        internal int index;

        /**
         * List forward link.
         */
        internal Vertex prev;

        /**
         * List backward link.
         */
        internal Vertex next;

        /**
         * Current face that this vertex is outside of.
         */
        internal Face face;

        /**
         * Constructs a vertex and sets its coordinates to 0.
         */
        public Vertex()
        {
            pnt = new Point3d();
        }

        /**
         * Constructs a vertex with the specified coordinates
         * and index.
         */
        public Vertex(double x, double y, double z, int idx)
        {
            pnt = new Point3d(x, y, z);
            index = idx;
        }

    }
}
