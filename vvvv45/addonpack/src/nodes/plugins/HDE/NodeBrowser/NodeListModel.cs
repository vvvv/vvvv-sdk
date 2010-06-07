using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notify;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of NodeListModel.
    /// </summary>
    /// 
    public class NodeListModel: Notifier
    {
        List<CategoryEntry> FCategoryList = new List<CategoryEntry>();
        public List<CategoryEntry> Categories
        {
            get{return FCategoryList;}
        }
        
        public NodeListModel()
        {
        }

        public void Add(CategoryEntry entry)
        {
            FCategoryList.Add(entry);
            FCategoryList.Sort(delegate(CategoryEntry c1, CategoryEntry c2) {return c1.Name.CompareTo(c2.Name);});
            //FireOnNotifyChanged();
        }
        
        public void Remove(CategoryEntry entry)
        {
            FCategoryList.Remove(entry);
            //FireOnNotifyChanged();
        }
        
        public bool Contains(string CategoryName)
        {
            return FCategoryList.Find(delegate(CategoryEntry ce){return ce.Name == CategoryName;}) != null;
        }
        
        public CategoryEntry GetCategoryEntry(string CategoryName)
        {
            return FCategoryList.Find(delegate(CategoryEntry ce){return ce.Name == CategoryName;});
        }
    }
}
