using System;
using System.Collections.Generic;
using System.Text;
using MrVux.Comparers;

namespace MrVux.Structures.PriorityQueues
{
    public enum ePriorityMode { eLowestFirst, eHighestFirst }

    public class PriorityQueue<T, P>
        where P : IComparable<P>
    {
        private ePriorityMode mode;

        //We consider element with lowest priority is stored in 0
        private List<PriorityObject<T, P>> list = new List<PriorityObject<T, P>>();
        protected IComparer<P> comparer;

        #region Constructors
        public PriorityQueue(ePriorityMode priorityMode)
        {
            this.mode = priorityMode;
            this.comparer = new DefaultComparer<P>();
        }

        public PriorityQueue(ePriorityMode priorityMode,IComparer<P> comparer)
        {
            this.mode = priorityMode;
            this.comparer = comparer;
        }
        #endregion

        #region Push
        public void Push(T element, P priority)
        {
            int idx = this.SearchIndex(priority);

            if (idx == -1)
            {
                list.Add(new PriorityObject<T, P>(element, priority));
            }
            else
            {
                list.Insert(idx, new PriorityObject<T, P>(element, priority));
            }
        }
        #endregion

        #region Pop
        public T Pop()
        {
            if (list.Count > 0)
            {
                T result = list[0].Element;
                //Remove element from list
                list.RemoveAt(0);
                return result;
            }
            else
            {
                throw new Exception("List is empty");
            }
        }
        #endregion

        #region Peek
        public T Peek()
        {
            if (list.Count > 0)
            {
                //We dont remove, just watch
                return list[0].Element;
            }
            else
            {
                throw new Exception("List is empty");
            }
        }
        #endregion

        #region Count
        public int Count
        {
            get { return list.Count; }
        }
        #endregion

        #region Search Index
        private int SearchIndex(P priority)
        {
            bool found = false;
            int idx = 0;

            while ((idx < this.list.Count) && !found)
            {
                int cmp = priority.CompareTo(this.list[idx].Priority);

                if (priority.CompareTo(this.list[idx].Priority) < 0)
                {
                    //Higher priority
                    found = true;
                }
                else
                {
                    idx++;
                }
            }

            if (found)
            {
                return idx;
            }
            else
            {
                return -1;
            }
        }
        #endregion
    }
}
