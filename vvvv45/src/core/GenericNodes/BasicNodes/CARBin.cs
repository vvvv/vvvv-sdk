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
	public class CARBin<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
        #region fields & pins
        readonly VecBinSpread<T> spread = new VecBinSpread<T>();

        #pragma warning disable 649
        protected IIOContainer<IInStream<T>> FInputContainer;
		
		[Input("Bin Size", DefaultValue = -1, CheckIfChanged = true, AutoValidate = false, Order = 10)]
		IInStream<int> FBin;

        protected IIOContainer<IOutStream<T>> FFirstContainer;
        protected IIOContainer<IOutStream<T>> FRemainderContainer;

        [Import]
        IIOFactory FFactory;
        #pragma warning restore
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<IInStream<T>>(
                new InputAttribute("Input") { CheckIfChanged = true, AutoValidate = false });

            FFirstContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("First Slice"));

            FRemainderContainer = FFactory.CreateIOContainer<IOutStream<T>>(
                new OutputAttribute("Remainder"));
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
		{
            Prepare();

            var input = FInputContainer.IOObject;
            var first = FFirstContainer.IOObject;
            var remainder = FRemainderContainer.IOObject;

			input.Sync(); 
			FBin.Sync();
			
			if (input.IsChanged || FBin.IsChanged)
			{
				spread.Sync(input,1,FBin);				
				
				first.Length = spread.Count;
				if (first.Length == spread.ItemCount || spread.ItemCount == 0)
				{
					remainder.Length=0;
					if (spread.ItemCount!=0)
							first.AssignFrom(input);
					else
						first.Length = 0;;
				}
				else
				{
					remainder.Length = spread.ItemCount-first.Length;
					using (var fWriter = first.GetWriter())
			        using (var rWriter = remainder.GetWriter())
					{
						for (int b = 0; b < spread.Count; b++)
						{
							fWriter.Write(spread[b][0]);
							rWriter.Write(spread[b], 1, spread[b].Length-1);
						}
					}
				}
			}
		}
	}
}
