using System;
using System.Collections.Generic;
using System.Text;
using NTrees.Lib.Base;

namespace NTrees.Lib
{
    public abstract class NTreeNode<N, B, E>
        where N : NTreeNode<N, B, E>
        where B : IBounds<E>,ISpliteable<B>
        where E : IElement<E>
    {
        private N[] children;

        private B bounds;

        private List<E> elements;
        private int maxElements;

        private bool identical;

        #region Constructor
        public NTreeNode(B bounds, int maxElements)
        {
            this.bounds = bounds;
            this.elements = new List<E>();
            this.maxElements = maxElements;
        }
        #endregion

        #region Child Count And setup children
        protected abstract int ChildCount { get; }
        protected abstract N CreateChild(B bounds, int maxElements);

        protected void SetupChildren()
        {
            this.children = new N[this.ChildCount];
        }
        #endregion

        #region Properties
        public N[] Children
        {
            get { return children; }
        }

        public B Bounds
        {
            get { return bounds; }
        }
        #endregion

        #region FindNode
        public NTreeNode<N, B, E> FindNode(E element)
        {
            if (this.bounds.IsInside(element))
            {
                if (this.children != null)
                {
                    foreach (N child in this.children)
                    {
                        if (child.Bounds.IsInside(element))
                        {
                            return child.FindNode(element);
                        }
                    }
                }
                else
                {
                    return this;
                }
            }

            return null;
        }
        #endregion

        #region Add an Element
        public bool AddElement(E element)
        {
            if (this.children == null)
            {
                this.elements.Add(element);

                if (this.elements.Count == 1)
                {
                    this.identical = true;
                    return true;
                }
                else
                {
                    if (!this.elements[0].IsEquals(element))
                    {
                        this.identical = false;
                    }

                    //We split in this case (check for degenerate case when all points identical
                    if (!this.identical && this.elements.Count > this.maxElements)
                    {
                        this.Split();
                    }

                    return true;
                }
            }
            else
            {
                //If we have children, no processing to do
                NTreeNode<N, B, E> child = this.FindNode(element);
                if (child != null)
                {
                    return child.AddElement(element);
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region Split the node
        public void Split()
        {
            //Get a split from the bounds
            this.SetupChildren();

            B[] childbounds = this.bounds.Split();

            for (int i = 0; i < childbounds.Length; i++)
            {
                this.children[i] = this.CreateChild(childbounds[i], this.maxElements);
                //Send child bouns, and propagate max points
                //this.children[i] = new NTreeNode<N, B, E>((N)this, childbounds[i], this.maxElements);
                //this.children[i] = new NTreeNode<N, B, E>(this as N,childbounds[i],this.maxElements);
            }

            foreach (E elem in this.elements)
            {
                //Copy the point to avoid removal
                this.AddElement(elem.Clone());
            }

            this.elements.Clear();
        }
        #endregion

        #region Get All Bounds
        public void GetAllBounds(List<B> result)
        {
            if (this.children == null)
            {
                result.Add(this.bounds);
            }
            else
            {
                foreach (N child in this.children)
                {
                    child.GetAllBounds(result);
                }
            }
        }
        #endregion
    }
}
