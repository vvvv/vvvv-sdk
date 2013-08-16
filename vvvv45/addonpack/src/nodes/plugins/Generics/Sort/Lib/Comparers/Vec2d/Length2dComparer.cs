using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Node10.OOPGenerics
{
	public class Length2dComparer : IComparer<Vector2D>
	{
		private Comparer<double> comparer = Comparer<double>.Default;

		public int Compare(Vector2D vec1, Vector2D vec2)
		{
			return comparer.Compare(vec1.x*vec1.x+vec1.y*vec1.y, vec2.x*vec2.x+vec2.y*vec2.y);
		}
	}

	[PluginInfo(Name = "Comparer",
	Category = "2d",
	Version = "Length",
	Tags = ""
	)]
	public class Length2dComparerNode : AbstractSortComparerNode<Vector2D>
	{
		protected override IComparer<Vector2D> GetComparer()
		{
			return new Length2dComparer();
		}
	}
}
