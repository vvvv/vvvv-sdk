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
    public class FaceList
    {

        private Face head;
        private Face tail;

        /**
         * Clears this list.
         */
        public void clear()
        {
            head = tail = null;
        }

        /**
         * Adds a vertex to the end of this list.
         */
        public void add(Face vtx)
        {
            if (head == null)
            {
                head = vtx;
            }
            else
            {
                tail.next = vtx;
            }
            vtx.next = null;
            tail = vtx;
        }

        public Face first()
        {
            return head;
        }

        /**
         * Returns true if this list is empty.
         */
        public bool isEmpty()
        {
            return head == null;
        }
    }
}
