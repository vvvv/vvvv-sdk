using System;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.Notifier;

namespace VVVV.Nodes.NodeBrowser
{
    /// <summary>
    /// Description of NodeListModel.
    /// </summary>
    /// 
    public class NodeListModel
    {
        List<CategoryEntry> FNodeList = new List<CategoryEntry>();
        public List<CategoryEntry> NodeList
        {
            get {return FNodeList;}
        }
    }
    
    public class AlphabetModel: NodeListModel
    {
        public AlphabetModel():base()
        {
        }

        public void Add(INodeInfo nodeInfo)
        {
            var category = NodeList.Find(delegate(CategoryEntry entry) {return string.Equals(entry.Name[0], nodeInfo.Name[0]);});
		    if (category == null)
		    {
		        category = new CategoryEntry(nodeInfo.Name[0].ToString());
		        NodeList.Add(category);
		        
		        NodeList.Sort(delegate(CategoryEntry e1, CategoryEntry e2) {return e1.Name.CompareTo(e2.Name);});
		    }
		    	
		    category.Add(nodeInfo);
            //FireOnNotifyChanged();
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
            var category = NodeList.Find(delegate(CategoryEntry entry) {return string.Equals(entry.Name[0], nodeInfo.Name[0]);});
		    if (category != null)
                category.Remove(nodeInfo);
            //FireOnNotifyChanged();
        }
    }

    public class CategoryModel: NodeListModel
    {
        public CategoryModel(): base()
        {
        }

        public void Add(INodeInfo nodeInfo)
        {
            var category = NodeList.Find(delegate(CategoryEntry entry) {return string.Equals(entry.Name, nodeInfo.Category);});
		    if (category == null)
		    {
		        category = new CategoryEntry(nodeInfo.Category);
		        NodeList.Add(category);
		        
		        NodeList.Sort(delegate(CategoryEntry e1, CategoryEntry e2) {return e1.Name.CompareTo(e2.Name);});
		    }
		    	
		    category.Add(nodeInfo);
            //FireOnNotifyChanged();
        }
        
        public void Remove(INodeInfo nodeInfo)
        {
            var category = NodeList.Find(delegate(CategoryEntry entry) {return string.Equals(entry.Name, nodeInfo.Category);});
		    if (category != null)
                category.Remove(nodeInfo);
            //FireOnNotifyChanged();
        }
    }
    
}
