using System;
using System.Collections;
using System.Collections.Generic;

using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of CategoryList.
    /// </summary>
    /// 
    public class CategoryList: IEnumerable
    {
        List<CategoryEntry> FCategoryList = new List<CategoryEntry>();
        public List<CategoryEntry> Categories
        {
            get{return FCategoryList;}
        }
        
        public CategoryList()
        {
        }

        public void Add(CategoryEntry entry)
        {
            FCategoryList.Add(entry);
            FCategoryList.Sort(delegate(CategoryEntry c1, CategoryEntry c2) {return c1.Name.CompareTo(c2.Name);});
        }
        
        public void Remove(CategoryEntry entry)
        {
            FCategoryList.Remove(entry);
        }
        
        public event CollectionDelegate Added;
        
		protected virtual void OnAdded(IViewableCollection collection, object item)
		{
			if (Added != null) {
				Added(collection, item);
			}
		}
        
        public event CollectionDelegate Removed;
        
		protected virtual void OnRemoved(IViewableCollection collection, object item)
		{
			if (Removed != null) {
				Removed(collection, item);
			}
		}
        
        public int Count 
        {
            get 
            {
                return FCategoryList.Count;
            }
        }
        
        public System.Collections.IEnumerator GetEnumerator()
        {
            return FCategoryList.GetEnumerator();
        }
    }
}
