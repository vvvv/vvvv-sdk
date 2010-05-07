using System;
using System.Collections.Generic;
using VVVV.Utils.Adapter;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.NodeBrowser
{
	public class NodeListAdapterFactory: AdapterFactory
	{
		public NodeListAdapterFactory()
			:base()
		{
			Add(typeof(AlphabetModel), typeof(NodeListModelContentProvider));
			Add(typeof(CategoryModel), typeof(NodeListModelContentProvider));
			Add(typeof(CategoryEntry), typeof(CategoryEntryContentProvider));
			Add(typeof(NodeInfoDummy), typeof(NodeInfoContentProvider));
		}
	}
}