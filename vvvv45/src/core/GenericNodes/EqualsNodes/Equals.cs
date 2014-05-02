using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Generic
{
	public class Equals<T> : IPluginEvaluate
	{
        [Input("Input", IsPinGroup = true)]
        public ISpread<ISpread<T>> Inputs;

        [Output("Output")]
        public ISpread<bool> Output;

        private readonly EqualityComparer<T> FComparer;

        public Equals(EqualityComparer<T> comparer = null)
        {
            FComparer = comparer ?? EqualityComparer<T>.Default;
        }
		
		public void Evaluate(int spreadMax)
		{
			Output.SliceCount = spreadMax;
			var pinCount = Inputs.SliceCount;
			for (int i = 0; i < spreadMax; i++) 
				Output[i] = EqualsSlice(i, pinCount);
		}
		
		bool EqualsSlice(int slice, int pinCount)
		{
			for (int j = 1; j < pinCount; j++) 
			{
				var a = Inputs[j-1][slice];
				var b = Inputs[j][slice];
                if (!FComparer.Equals(a, b))
                    return false;
			}
			return true;
		}
	}
}
