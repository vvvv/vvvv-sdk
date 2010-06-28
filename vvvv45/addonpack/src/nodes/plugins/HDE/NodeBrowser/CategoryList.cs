using System;
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
    public class CategoryList: IViewableCollection
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
        
        public event CollectionDelegate Removed;
        
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
