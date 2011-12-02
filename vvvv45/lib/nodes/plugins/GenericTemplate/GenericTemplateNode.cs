#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template", Category = "Generic", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class GenericTemplateNode : IPluginEvaluate
	{
		[Input("Input")]
		ISpread<IContainer<Fruit>> FInput;

		[Output("Output")]
		ISpread<IContainer<Apple>> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = FInput[0];
		}
	}
	
	public class Fruit
	{
		
	}
	
	public class Apple : Fruit
	{
		
	}
	
	public interface IContainer<in T>
	{
		
	}
}
