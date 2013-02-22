using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.Hosting
{
	[Export(typeof(IAddonFactory))]
	[ComVisible(false)]
	public class FreeFrameFactory : IAddonFactory
	{
		[ImportingConstructor]
		public FreeFrameFactory (INodeInfoFactory nodeInfoFactory)
		{
			nodeInfoFactory.NodeInfoAdded += HandleNodeInfoFactoryNodeInfoAdded;
		}
		
		public string Name
		{
		    get
		    {
		        return ToString();
		    }
		}
		
		public bool AllowCaching
        {
            get
            {
                return true;
            }
        }

		void HandleNodeInfoFactoryNodeInfoAdded (object sender, INodeInfo nodeInfo)
		{
			if (nodeInfo.Type == NodeType.Freeframe)
				nodeInfo.Factory = this;
		}

		#region IAddonFactory implementation
		public INodeInfo[] ExtractNodeInfos (string filename, string arguments)
		{
		    return new INodeInfo[0];
		}

		public bool Create (INodeInfo nodeInfo, INode host)
		{
			return true;
		}

		public bool Delete (INodeInfo nodeInfo, INode host)
		{
			return true;
		}

		public bool Clone (INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo)
		{
			newNodeInfo = null;
			return false;
		}

		public void AddDir (string dir, bool recursive)
		{
			
		}

		public void RemoveDir (string dir)
		{
			
		}

		public string JobStdSubPath {
			get {
				return "freeframes";
			}
		}

        public bool GetNodeListAttribute(INodeInfo nodeInfo, out string name, out string value)
        {
            name = string.Empty;
            value = string.Empty;
            return false;
        }

        public void ParseNodeEntry(System.Xml.XmlReader xmlReader, INodeInfo nodeInfo)
        {
            return;
        }
		#endregion
	}
}

