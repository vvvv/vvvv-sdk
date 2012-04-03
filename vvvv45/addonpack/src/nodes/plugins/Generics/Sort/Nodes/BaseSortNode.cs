using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
	

	public class SliceSortHolder<T>
	{
		public T Item { get; set; }
		public int FormerSlice { get; set; }
	}

	public class SliceSortComparer<T,C> : IComparer<SliceSortHolder<T>> where C : IComparer<T>
	{
		private C comparer;

		public SliceSortComparer(C comparer)
		{
			this.comparer = comparer;
		}

		public int Compare(SliceSortHolder<T> x, SliceSortHolder<T> y)
		{
			return comparer.Compare(x.Item, y.Item);
		}
	}

	public class BaseSortNode<T> : IPluginEvaluate
	{
		[Input("Input")]
		ISpread<ISpread<T>> FInput;

		[Input("Comparer",IsSingle=true,Visibility=PinVisibility.Hidden)]
		ISpread<IComparer<T>> FComparer;

		[Output("Output")]
		ISpread<ISpread<T>> FOutput;

		[Output("Former Slice")]
		ISpread<ISpread<int>> FFormer;

		public void Evaluate(int SpreadMax)
		{
			//List<double> result = new List<double>();
			IComparer<T> comparer;// = Comparer<T>.Default;
			comparer = this.FComparer[0] == null ? Comparer<T>.Default : FComparer[0];

			SliceSortComparer<T, IComparer<T>> ssc = new SliceSortComparer<T, IComparer<T>>(comparer);

			FOutput.SliceCount = FInput.SliceCount;
			FFormer.SliceCount = FInput.SliceCount;
			int cnt = 0;
			for (int i = 0; i < FInput.SliceCount; i++)
			{
				List<SliceSortHolder<T>> sort = new List<SliceSortHolder<T>>();
				for (int j = 0; j < FInput[i].SliceCount; j++)
				{
					SliceSortHolder<T> holder = new SliceSortHolder<T>();
					holder.FormerSlice = cnt;
					holder.Item = FInput[i][j];

					sort.Add(holder);

					cnt++;
					cnt = cnt == SpreadMax ? 0 : cnt;
				}

				sort.Sort(ssc);

				var items =
					from holder in sort
					select holder.Item;

				var slice =
					from holder in sort
					select holder.FormerSlice;

				FOutput[i] = new Spread<T>(items.ToList());
				FFormer[i] = new Spread<int>(slice.ToList());
				
			}
		}
	}
}
