#region licence/info

//////project name
//Occurrence

//////description
//counts the occurrence of equal slices

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.PluginInterfaces.V2
//VVVV.Utlis.VColor;
//VVVV.Utils.VMath;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.ComponentModel.Composition;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	[PluginInfo(Name = "Occurrence", 
	            Category = "Value",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceValue: Occurrence<double>
	{
		[Input("Epsilon", IsSingle = true, MinValue=0, Order = 1)]
    	IDiffSpread<double> FEps;
		
		public override bool Equals<T>(T a, T b)
		{
			double _a = (double)(a as object);
			double _b = (double)(b as object);
			return (_a>=_b-FEps[0] && _a<=_b+FEps[0]);
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FEps.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "String",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceString: Occurrence<string>
	{
		[Input("Case Sensitive", IsSingle = true, Order = 1)]
		IDiffSpread<bool> FCase;
		
		public override bool Equals<T>(T a, T b)
		{
			string _a = (string)(a as object);
			string _b = (string)(b as object);
			if (!FCase[0])
			{
				_a = _a.ToLower();
				_b = _b.ToLower();
			}
			return _a==_b;
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FCase.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}
	
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Color",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceColor: Occurrence<RGBAColor>
	{
		[Input("Epsilon", IsSingle = true, Order = 1)]
    	IDiffSpread<Vector4D> FEps;
    	
		public override bool Equals<T>(T a, T b)
		{
			RGBAColor _a = (RGBAColor)(a as object);
			RGBAColor _b = (RGBAColor)(b as object);
			return 	(_a.R>=_b.R-FEps[0].x && _a.R<=_b.R+FEps[0].x) &&
					(_a.G>=_b.G-FEps[0].x && _a.G<=_b.G+FEps[0].y) &&
					(_a.B>=_b.B-FEps[0].z && _a.B<=_b.B+FEps[0].z) &&
					(_a.A>=_b.A-FEps[0].w && _a.A<=_b.A+FEps[0].w);
		}
		
		public override void Evaluate(int SpreadMax)
		{
			if(FEps.IsChanged)
				eval = true;
			base.Evaluate(SpreadMax);
		} 
	}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Enumerations",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceEnum: Occurrence<EnumEntry> {}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Transform",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceTransform: Occurrence<Matrix4x4> {}
}
