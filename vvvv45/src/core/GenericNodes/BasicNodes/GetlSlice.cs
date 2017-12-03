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

    public class GetSlice<T> : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        #region fields & pins
        #pragma warning disable 0649
        protected IIOContainer<ISpread<ISpread<T>>> FInputContainer;

        [Input("Index", Order = 100)]
        ISpread<int> FIndex;

        protected IIOContainer<ISpread<ISpread<T>>> FOutputContainer;

        [Import]
        IIOFactory FFactory;
        #pragma warning restore
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FInputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new InputAttribute("Input") { BinName = "Bin Size", BinSize = 1, BinOrder = 1 });
            
            FOutputContainer = FFactory.CreateIOContainer<ISpread<ISpread<T>>>(
                new OutputAttribute("Output") { BinName = "Output Bins"});
        }

        protected virtual void Prepare() { }

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            Prepare();

            FOutputContainer.IOObject.SliceCount = FIndex.SliceCount;
            var count = FInputContainer.IOObject.SliceCount;

            for (int i = 0; i < FIndex.SliceCount; i++)
            {
                FOutputContainer.IOObject[i].AssignFrom(FInputContainer.IOObject[VMath.Zmod(FIndex[i], count)]);
            }
        }
    }

}
