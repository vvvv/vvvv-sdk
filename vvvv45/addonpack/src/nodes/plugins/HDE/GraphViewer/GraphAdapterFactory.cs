using System;
using System.Collections.Generic;
using VVVV.Utils.Adapter;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Graph.Provider
{
	public class GraphAdapterFactory: AdapterFactory
	{
		public GraphAdapterFactory()
			:base()
		{
			Add(typeof(INode), typeof(PatchNodeContentProvider));
		}
	}
}