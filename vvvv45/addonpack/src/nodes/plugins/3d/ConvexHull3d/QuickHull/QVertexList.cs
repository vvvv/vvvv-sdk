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

    class VertexList
    {
        private Vertex head;
        private Vertex tail;

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
        public void add(Vertex vtx)
        {
            if (head == null)
            {
                head = vtx;
            }
            else
            {
                tail.next = vtx;
            }
            vtx.prev = tail;
            vtx.next = null;
            tail = vtx;
        }

        /**
         * Adds a chain of vertices to the end of this list.
         */
        public void addAll(Vertex vtx)
        {
            if (head == null)
            {
                head = vtx;
            }
            else
            {
                tail.next = vtx;
            }
            vtx.prev = tail;
            while (vtx.next != null)
            {
                vtx = vtx.next;
            }
            tail = vtx;
        }

        /**
         * Deletes a vertex from this list.
         */
        public void delete(Vertex vtx)
        {
            if (vtx.prev == null)
            {
                head = vtx.next;
            }
            else
            {
                vtx.prev.next = vtx.next;
            }
            if (vtx.next == null)
            {
                tail = vtx.prev;
            }
            else
            {
                vtx.next.prev = vtx.prev;
            }
        }

        /**
         * Deletes a chain of vertices from this list.
         */
        public void delete(Vertex vtx1, Vertex vtx2)
        {
            if (vtx1.prev == null)
            {
                head = vtx2.next;
            }
            else
            {
                vtx1.prev.next = vtx2.next;
            }
            if (vtx2.next == null)
            {
                tail = vtx1.prev;
            }
            else
            {
                vtx2.next.prev = vtx1.prev;
            }
        }

        /**
         * Inserts a vertex into this list before another
         * specificed vertex.
         */
        public void insertBefore(Vertex vtx, Vertex next)
        {
            vtx.prev = next.prev;
            if (next.prev == null)
            {
                head = vtx;
            }
            else
            {
                next.prev.next = vtx;
            }
            vtx.next = next;
            next.prev = vtx;
        }

        /**
         * Returns the first element in this list.
         */
        public Vertex first()
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
