using System;
using System.Collections.Generic;

namespace VVVV.Nodes
{
	/// <summary>
	/// Description of MapInterval.
	/// </summary>
	public class MapInterval
	{
		private List<double> inPts;
		private List<double> inDiff;
		
		private List<double> outPts;
		private List<double> outDiff;
		
		
		public MapInterval(List<double> InBpts, List<double> OutBpts)
		{
			inPts = new List<double>(InBpts);
			outPts = new List<double>(OutBpts);
			inDiff = new List<double>(GetDiff(InBpts));
			outDiff = new List<double>(GetDiff(OutBpts));
		}
		
		public double DoMap(double Input, int EnumIn)
		{
			int incr=0;
			double factor=0;
			switch (EnumIn)
			{
				case 3:
					Input=Math.Max(Input, inPts[0]);
					Input=Math.Min(Input, inPts[inPts.Count-1]);
					break;
				case 2:
					double mFactor = inPts[inPts.Count-1]-inPts[0];
					Input = (Input-inPts[0])/mFactor;
					Input = Input%2;
					if (Input>1.0)
						Input=2.0-Input;
					Input = inPts[0]+(Input*mFactor);
					break;
				case 1:
					double wFactor = inPts[inPts.Count-1]-inPts[0];
					Input = (Input-inPts[0])/wFactor;
					Input = Input%1;
					Input = inPts[0]+(Input*wFactor);
					break;
				default:
					break;
			}
			foreach (double d in inPts)
			{
				factor =(Input-d)/inDiff[incr];
				if (factor<0)
					break;
				else if (0.0<=factor && factor<1.0)
					break;
				else
					incr++;
			}
			incr = Math.Min(incr, outPts.Count-1);
			return outPts[incr]+factor*outDiff[incr];
		}
		
		private List<double> GetDiff(List<double> pts)
		{
			List<double> outList = new List<double>();
			if (pts.Count>1)
			{
			for (int i=0; i<pts.Count-1; i++)
			{
				outList.Add(pts[i+1]-pts[i]);
			}
			outList.Add(outList[outList.Count-1]);
			}
			else
				outList.Add(1.0);
			return outList;
		}
	}
}
