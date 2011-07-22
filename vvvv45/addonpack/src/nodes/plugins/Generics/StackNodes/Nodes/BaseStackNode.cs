using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    public class BaseStackNode<T> : IPluginEvaluate
    {
        #region Fields
        [Input("Input")]
        ISpread<T> FPinInput;

        [Input("Push",IsBang=true,IsSingle=true)]
        ISpread<bool> FPinInPush;

        [Input("Pop", IsBang = true, IsSingle = true)]
        ISpread<bool> FPinInPop;

		[Input("Stack Size", DefaultValue = -1, IsSingle = true)]
		IDiffSpread<int> FPinInStackSize;

        [Input("Default")]
        ISpread<T> FPinDefault;

        [Input("Reset", IsBang = true, IsSingle = true)]
        ISpread<bool> FPinInReset;

        [Output("Output")]
        ISpread<T> FPinOutput;

		[Output("Peek")]
		ISpread<T> FPinOutPeek;

		[Output("Stack Size")]
		ISpread<int> FPinOutStackSize;

        private Stack<List<T>> FStack;
		private int FStackSize = -1;
        #endregion

        public BaseStackNode()
        {
            FStack = new Stack<List<T>>();
        }

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {

			//Update Stack size
			if (FPinInStackSize.IsChanged)
			{
				FStackSize = FPinInStackSize[0];
			}

            if (this.FPinInReset[0])
            {
                FStack.Clear();
                if (FPinDefault.SliceCount > 0)
                {
                    FStack.Push(FPinDefault.ToList());
                }
            }
            else
            {

                //Push spread in the stack if required
                if (FPinInPush[0] && FPinInput.SliceCount > 0)
                {
                    FStack.Push(FPinInput.ToList());
                }

                //Pop as many items as required to reduce buffer size
                if (FStackSize >= 0)
                {
                    while (this.FStack.Count > FStackSize) { FStack.Pop(); }
                }

                //Pop an element off the stack and output it
                if (FPinInPop[0] && this.FStack.Count > 0)
                {
                    List<T> output = FStack.Pop();
                    FPinOutput.SliceCount = output.Count;
                    for (int i = 0; i < output.Count; i++)
                    {
                        FPinOutput[i] = output[i];
                    }
                }
                else
                {
                    FPinOutput.SliceCount = 0;
                }

                //Output stack size
                this.FPinOutStackSize.SliceCount = 1;
                this.FPinOutStackSize[0] = this.FStack.Count;

                //Show top level spread
                if (this.FStack.Count > 0)
                {
                    List<T> peek = FStack.Peek();
                    FPinOutPeek.SliceCount = peek.Count;
                    for (int i = 0; i < peek.Count; i++)
                    {
                        FPinOutPeek[i] = peek[i];
                    }
                }
                else
                {
                    this.FPinOutPeek.SliceCount = 0;
                }
            }
        }
        #endregion
    }


}
