#region usings
using System;
using System.ComponentModel.Composition;

using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Delay", Category = "Animation", Version ="Framebased", Help = "Framebased delay with adjustable frame count.", Tags = "Whiplash/joshuavh")]
	#endregion PluginInfo
	public class DelayFramebased : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Skip", IsBang = false)]
		public ISpread<bool> skip;
		
		[Input("Reset", IsBang = false)]
		public ISpread<bool> reset;

		[Input("Delay", DefaultValue = 1.0, MinValue = 0)]
		public ISpread<int> delayFrames;
		
		
		[Output("Output")]
		public ISpread<double> FOutput;
		
		
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		
////////////////////////////////////////////////////////////////////////////		
	
		
		List<Queue<double>> listOfQueue = new List<Queue<double>>();
		
	
////////////////////////////////////////////////////////////////////////////		
		
		//called when data for any output pin is requested
				
		public void Evaluate(int SpreadMax)
		{
			
			
			FOutput.SliceCount = SpreadMax;
			
			
			if(SpreadMax != listOfQueue.Count){
				
				int difference = SpreadMax - listOfQueue.Count;
				
				
				
				if(difference < 0){
					
					int temp = difference * -1;
					listOfQueue.RemoveRange(SpreadMax-1,temp);
								
				}
				
				if(difference > 0){
					for(int i = 0; i<difference; i++){
						listOfQueue.Add(new Queue<double>());
					}
				}
				
				
			}
			
			
			for(int index = 0; index<SpreadMax;index++){
				
					
				if(delayFrames[index] != listOfQueue[index].Count){
				
				
					int difference = delayFrames[index] - listOfQueue[index].Count;
								
					if(delayFrames[index]>=0){
						setDelayQueue(index, SpreadMax, difference);	
					}
				
					else{
						FOutput[index] = FInput[index];
					}
			
				}
			
				if(reset[index]){
				listOfQueue[index].Clear();
				}
			
			listOfQueue[index].Enqueue(FInput[index]);
				
				if(skip[index]){
					FOutput[index] = FInput[index];
				}
			
				else{
				FOutput[index] = listOfQueue[index].Dequeue();
				}
			}

			
		}
			

////////////////////////////////////////////////////////////////////////////	
		
		
		public void setDelayQueue(int index, int SMax, int diff){
			
			
			if(diff > 0){
				for(int i = 0; i < diff; i++){
					listOfQueue[index].Enqueue(FInput[index]);
				}
			}
			
			if(diff < 0){
				
				int temp = diff * -1;
				
				for(int i = 0; i<temp ;i++){
					FOutput[index] = listOfQueue[index].Dequeue();
				}
			}	
			
		}
			
	
		
	}
}

