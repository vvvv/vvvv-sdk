#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Generic
{

	public class SetSpread<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
        protected IIOContainer<ISpread<ISpread<T>>> FSpreadContainer;
        protected IIOContainer<ISpread<ISpread<T>>> FInputContainer;
		
		[Input("Offset", Order = 20)]
        protected ISpread<int> FOffset;

        protected IIOContainer<ISpread<ISpread<T>>> FOutputContainer;

        [Import]
        IIOFactory FFactory;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FSpreadContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new InputAttribute("Spread"));

            FInputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
               new InputAttribute("Input") { BinSize = 1, BinName = "Count", BinOrder = 1 });

            FOutputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new OutputAttribute("Output"));
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
		{
            Prepare();

            var spread = FSpreadContainer.IOObject;
            var input = FInputContainer.IOObject;
            var output = FOutputContainer.IOObject;

			output.SliceCount = SpreadUtils.SpreadMax(spread, input, FOffset);

            for (int i=0; i<input.SliceCount; i++)
			{
				output[i].AssignFrom(spread[i]);
				for (int s=0; s<input[i].SliceCount; s++)
					output[i][s+FOffset[i]]=input[i][s];
			}
		}
	}
	
}
