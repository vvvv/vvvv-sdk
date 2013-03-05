#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template", Category = "Value", Version = "DynamicPins", Help = "Basic template with a dynamic amount of in- and outputs", Tags = "")]
	#endregion PluginInfo
	public class DynamicPinsValueTemplateNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		// A spread which contains our inputs
		Spread<IIOContainer<ISpread<double>>> FInputs = new Spread<IIOContainer<ISpread<double>>>();
		
		// A spread which contains our outputs
		Spread<IIOContainer<ISpread<double>>> FOutputs = new Spread<IIOContainer<ISpread<double>>>();
		
		[Config("Input Count", DefaultValue = 1, MinValue = 0)]
		IDiffSpread<int> FInputCountIn;
		
		[Config("Output Count", DefaultValue = 1, MinValue = 0)]
		IDiffSpread<int> FOutputCountIn;
		
		[Import]
		IIOFactory FIOFactory;
		#endregion fields & pins
		
		#region pin management
		public void OnImportsSatisfied()
		{
			FInputCountIn.Changed += HandleInputCountChanged;
			FOutputCountIn.Changed += HandleOutputCountChanged;
		}
		
		private void HandlePinCountChanged<T>(ISpread<int> countSpread, Spread<IIOContainer<T>> pinSpread, Func<int, IOAttribute> ioAttributeFactory) where T : class
		{
			pinSpread.ResizeAndDispose(
				countSpread[0], 
				(i) => 
				{
					var ioAttribute = ioAttributeFactory(i + 1);
					return FIOFactory.CreateIOContainer<T>(ioAttribute);
				}
			);
		}
		
		private void HandleInputCountChanged(IDiffSpread<int> sender)
		{
			HandlePinCountChanged(sender, FInputs, (i) => new InputAttribute(string.Format("Input {0}", i)));
		}
		
		private void HandleOutputCountChanged(IDiffSpread<int> sender)
		{
			HandlePinCountChanged(sender, FOutputs, (i) => new OutputAttribute(string.Format("Output {0}", i)));
		}
		#endregion
		
		// Called when data for any output pin is requested.
		public void Evaluate(int SpreadMax)
		{
			for (int i = 0; i < FInputs.CombineWith(FOutputs); i++)
			{
				var inputSpread = FInputs[i].IOObject;
				var outputSpread = FOutputs[i].IOObject;
				outputSpread.AssignFrom(inputSpread);
			}
		}
	}
}
