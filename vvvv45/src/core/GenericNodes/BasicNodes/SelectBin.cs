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
	public class SelectBin<T>: IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		protected IIOContainer<ISpread<ISpread<T>>> FInputContainer;
	
		[Input("Select", DefaultValue = 1, MinValue = 0, Order = 100)]
		protected ISpread<int> FSelect;
		
		protected IIOContainer<ISpread<ISpread<T>>> FOutputContainer;
		
		[Output("Former Slice", Order = 100)]
		protected ISpread<int> FFormerSlice;

        [Import]
        IIOFactory FFactory;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new InputAttribute("Input") { BinSize = 1 });

            FOutputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new OutputAttribute("Output"));
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public virtual void Evaluate(int SpreadMax)
		{
            Prepare();

            var input = FInputContainer.IOObject;
            var output = FOutputContainer.IOObject;

			int sMax = SpreadUtils.SpreadMax(input, FSelect);

            output.SliceCount = 0;
			FFormerSlice.SliceCount=0;
			
			for (int i = 0; i < sMax; i++) 
			{		
				for (int s=0; s<FSelect[i]; s++)
				{
					if (s==0)
					{
                        output.SliceCount++;
                        output[output.SliceCount-1].SliceCount=0;
					}
                    output[output.SliceCount-1].AddRange(input[i]);
					
					FFormerSlice.Add(i);
				}
			}
		}
    }
}
