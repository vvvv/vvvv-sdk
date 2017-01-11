using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes
{
	[PluginInfo(Name = "Sort",
		Category = "Spreads",
		Version = "Advanced",
		Tags = "order", 
        Author="vux, elias"
		)]
	public class ValueSortNode : BaseSortNode<double>//, IPluginEvaluate
	{
	}


	[PluginInfo(Name = "Sort",
	Category = "String",
	Version = "Advanced",
	Tags = "order", 
    Author = "vux, elias"
	)]
	public class StringSortNode : BaseSortNode<string>//, IPluginEvaluate
	{
	}
}
