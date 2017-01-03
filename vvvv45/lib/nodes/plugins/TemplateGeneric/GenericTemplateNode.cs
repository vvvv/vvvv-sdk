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
	[PluginInfo(Name = "Template", 
				Category = "Node", 
				Version = "Source", 
				Help = "Basic template with one value in/out", 
				Tags = "c#")]
	public class GenericTemplateSourceNode : IPluginEvaluate
	{
		public class DerivedObject
		{
			public override string ToString()
			{
				return "DerivedObject created by Template (Node Source)";
			}
		}
		
		[Output("Output")]
        public ISpread<Func<DerivedObject>> FOutput;
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput[0] = () => new DerivedObject();
		}
	}
	
	[PluginInfo(Name = "Template", 
				Category = "Node", 
				Version = "Sink", 
				Help = "Basic template with one value in/out", 
				Tags = "c#")]
	public class GenericTemplateSinkNode : IPluginEvaluate
	{
		[Input("Input")]
        public ISpread<Func<object>> FInput;
		
		[Output("Name")]
        public ISpread<string> FOutput;
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 0;
			foreach (var func in FInput)
			{
				if (func == null) continue;
				
				var obj = func();
				FOutput.Add(obj.ToString());
			}
		}
	}
}
