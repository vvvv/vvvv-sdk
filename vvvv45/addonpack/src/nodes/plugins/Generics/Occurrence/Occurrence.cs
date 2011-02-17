#region licence/info

//////project name
//OccurrenceValue

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
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	public class Occurrence<T>: IPluginEvaluate
    {	          	
    	#region fields & pins
    	[Input("Input")]
    	IDiffSpread<T> FInput;
    	
    	
    	[Output("Count")]
    	ISpread<int> FCountOut;
    	
    	[Output("First Occurrence")]
    	ISpread<int> FFirstOcc;
    	
    	[Output("Unique")]
    	ISpread<T> FUniques;
    	
    	[Output("Unique Index", Visibility = PinVisibility.OnlyInspector)]
    	ISpread<int> FUniIds;
    	
    	[Output("Occurrence Index", Visibility = PinVisibility.OnlyInspector)]
    	ISpread<int> FOccIndex;
    	
    	public bool eval = false;
    	#endregion fields & pins

    	public virtual bool Equals<T>(T a, T b)
    	{
    		return a.Equals(b);
    	}
    	
        public virtual void Evaluate(int SpreadMax)
        {     	
        	if (FInput.IsChanged || eval)
        	{	
        		eval = false;
        		FUniIds.SliceCount=FInput.SliceCount;
        		FOccIndex.SliceCount=FInput.SliceCount;

       			List<UniqueObj<T>> uniqueList = new List<UniqueObj<T>>();
	        	for (int i=0; i<FInput.SliceCount; i++)
	        	{
	        		bool isUnique = true;
	        		 	for (int j=0; j<uniqueList.Count; j++)
	        		 	{
	        		 		if (Equals(FInput[i], uniqueList[j].Unique))
	        		 		{
	        		 			isUnique=false;
	        		 			FUniIds[i] = j;
	        		 			FOccIndex[i] = uniqueList[j].AddMember(i);
	        		 			break;
	        		 		}
	        		 	}
	        		 	if (isUnique)
	        		 	{
	        		 		uniqueList.Add(new UniqueObj<T>(FInput[i], i));
	        		 		FUniIds[i] = uniqueList.Count-1;
	        		 		FOccIndex[i] = 0;
	        		 	}
	        	}
        		
        		
        		FCountOut.SliceCount=uniqueList.Count;
        		FFirstOcc.SliceCount=uniqueList.Count;
        		FUniques.SliceCount=uniqueList.Count;
        		for (int i=0; i<uniqueList.Count; i++)
        		{
        			FCountOut[i] = uniqueList[i].Count;
        			FFirstOcc[i] = uniqueList[i].FirstOccurred;
        			FUniques[i] = uniqueList[i].Unique;
        		}
        	}      	
        }
	
	}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Value",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceValue: Occurrence<double>
	{
		[Input("Epsilon", IsSingle = true, Order = 1)]
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
	public class OccurrenceEnum: Occurrence<EnumEntry>
	{
	}
	
	[PluginInfo(Name = "Occurrence", 
	            Category = "Transform",
	            Help = "counts the occurrence of equal slices",
	            Tags = "count, occurrence, spectral, spread",
	           	Author = "woei")]
	public class OccurrenceTransform: Occurrence<Matrix4x4>
	{
	}
}
