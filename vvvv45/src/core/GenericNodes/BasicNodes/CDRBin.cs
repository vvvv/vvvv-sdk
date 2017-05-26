#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
#endregion usings

namespace VVVV.Nodes.Generic
{
	public class CDRBin<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        protected IIOContainer<IInStream<T>> FInputContainer;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false, Order = 10)]
		IInStream<int> FBin;
		
        protected IIOContainer<IOutStream<T>> FRemainderContainer;		
        protected IIOContainer<IOutStream<T>> FLastContainer;

        [Import]
        IIOFactory FFactory;
        #pragma warning restore
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<IInStream<T>>(
                new InputAttribute("Input") { CheckIfChanged = true, AutoValidate = false });

            FRemainderContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("Remainder"));

            FLastContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("Last Slice"));
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
		{
            Prepare();
            var input = FInputContainer.IOObject;
            var remainder = FRemainderContainer.IOObject;
            var last = FLastContainer.IOObject;

			input.Sync(); 
			FBin.Sync();
			
			if (input.IsChanged || FBin.IsChanged)
			{
				spread.Sync(input,1,FBin);
				
				last.Length = spread.Count;
				if (last.Length == spread.ItemCount || spread.ItemCount==0)
				{
					remainder.Length = 0;
					if (spread.ItemCount!=0)
						last.AssignFrom(input);
					else
						last.Length=0;
				}
				else
				{
					remainder.Length = spread.ItemCount-last.Length;
					using (var rWriter = remainder.GetWriter())
					using (var lWriter = last.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							int rLength = spread[b].Length-1;
							rWriter.Write(spread[b], 0, rLength);
							
							lWriter.Write(spread[b][rLength]);
						}
					}
				}
			}
		}
	}
}
