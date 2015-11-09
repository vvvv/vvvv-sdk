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

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes.Generic
{
	public class Occurrence<T>: IPluginEvaluate
    {	          	
    	#region fields & pins
    	[Input("Input")]
        protected IDiffSpread<ISpread<T>> FInput;
    	
    	
    	[Output("Count")]
        protected ISpread<int> FCountOut;
    	
    	[Output("First Occurrence")]
        protected ISpread<int> FFirstOcc;
    	
    	[Output("Unique")]
        protected ISpread<T> FUniques;
    	
    	[Output("Bin Size")]
        protected ISpread<int> FBinSize;
    
    	
    	[Output("Former Index")]//, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<ISpread<int>> FFormerIndex;
    
    	
    	[Output("Unique Index", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FUniIds;
    	
    	[Output("Occurrence Index", Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<int> FOccIndex;


        protected bool eval = false;
    	#endregion fields & pins

    	public virtual bool Equals(T a, T b)
    	{
    		return a.Equals(b);
    	}
    	
        public virtual void Evaluate(int SpreadMax)
        {     	
        	if (FInput.IsChanged || eval)
        	{	
        		eval = false;
        		FCountOut.SliceCount=0;
        		FFirstOcc.SliceCount=0;
        		FUniques.SliceCount=0;
        		FBinSize.SliceCount=0;
        		
        		FFormerIndex.SliceCount=0;
        		
        		FUniIds.SliceCount=0;
        		FOccIndex.SliceCount=0;
        		int oIncr = 0;
        		for (int b=0; b<FInput.SliceCount; b++)
        		{
        			for (int s=0; s<FInput[b].SliceCount; s++)
        			{
        				bool isUnique = true;
        				for (int o=oIncr; o<FUniques.SliceCount; o++)
        				{
	        				if (Equals(FInput[b][s],FUniques[o]))
	        				{
	        					isUnique=false;
	        					FUniIds.Add(o);
	        					FFormerIndex[o].Add(s);
	        					FOccIndex.Add(FCountOut[o]);
	        					
	        					FCountOut[o]++;
	        					break;
	        				}	
        				}
        				if (isUnique)
        				{
        					FUniIds.Add(FUniques.SliceCount);
	        				FOccIndex.Add(0);
	        				FFormerIndex.Add(new Spread<int>(0));
	        				FFormerIndex[FFormerIndex.SliceCount-1].Add(s);
	        					
	        				FUniques.Add(FInput[b][s]);
	        				FCountOut.Add(1);
	        				FFirstOcc.Add(s);
        				}
        			}
        			FBinSize.Add(FUniques.SliceCount-oIncr);
        			oIncr=FUniques.SliceCount;
        		}   		
        	}      	
        }
	
	}
}
