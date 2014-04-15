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
	[PluginInfo(Name = "Source", Category = "Node", Help = "Basic template with one value in/out", Tags = "", AutoEvaluate = true)]
	public class NodeSourceNode : IPluginEvaluate
	{
		[Input("Input")]
		IDiffSpread<double> FInput;
		
		[Output("Output")]
		ISpread<Foo> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsChanged)
			{
				FOutput[0] = new Foo(FInput[0]);
			}
		}
	}
	
	[PluginInfo(Name = "Sink", Category = "Node", Help = "Basic template with one value in/out", Tags = "")]
	public class NodeSinkNode : IPluginEvaluate
	{
		[Input("Input")]
		IDiffSpread<Foo> FInput;

		[Output("Output")]
		ISpread<double> FOutput;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInput.IsChanged)
			{
				FOutput[0] = FInput[0].x;
			}
		}
	}
	
	public struct Foo
	{
		public double x;
		public Foo(double x)
		{
			this.x = x;
		}
	}
}
