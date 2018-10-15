using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Node10.OOPGenerics
{
	public class Angle2dComparer : IComparer<Vector2D>
	{
		private Comparer<double> comparer = Comparer<double>.Default;

		public int Compare(Vector2D x, Vector2D y)
		{
			return comparer.Compare(Math.Atan2(x.y, x.x), Math.Atan2(y.y, y.x));
		}
	}

	[PluginInfo(Name = "Comparer",
	Category = "2d",
	Version = "Angle",
	Tags = ""
	)]
	public class Angle2dComparerNode : AbstractSortComparerNode<Vector2D>
	{
		protected override IComparer<Vector2D> GetComparer()
		{
			return new Angle2dComparer();
		}
	}
}
