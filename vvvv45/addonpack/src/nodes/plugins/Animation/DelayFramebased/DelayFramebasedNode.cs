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
	[PluginInfo(Name = "Delay", Category = "Animation", Version ="Framebased", Help = "Framebased delay with adjustable frame count")]
	#endregion PluginInfo
	public class DelayFramebased : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<double> FInput;
		
		[Input("Skip")]
		public ISpread<bool> FSkip;
		
		[Input("Reset")]
		public ISpread<bool> FReset;

		[Input("Delay", DefaultValue = 1.0, MinValue = 0)]
		public ISpread<int> FDelayFrames;
			
		[Output("Output")]
		public ISpread<double> FOutput;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins

		List<Queue<double>> listOfQueue = new List<Queue<double>>();
		
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
						
				if(FDelayFrames[index] != listOfQueue[index].Count){
				
					int difference = FDelayFrames[index] - listOfQueue[index].Count;
								
					if(FDelayFrames[index]>=0){
						SetDelayQueue(index, SpreadMax, difference);	
					}
				
					else{
						FOutput[index] = FInput[index];
					}
				}
			
				if(FReset[index]){
				listOfQueue[index].Clear();
				}
			
			listOfQueue[index].Enqueue(FInput[index]);
				
				if(FSkip[index]){
					FOutput[index] = FInput[index];
				}
			
				else{
				FOutput[index] = listOfQueue[index].Dequeue();
				}
			}
		}

		public void SetDelayQueue(int index, int SMax, int diff){
			
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

