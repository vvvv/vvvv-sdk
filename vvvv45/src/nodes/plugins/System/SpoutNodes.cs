#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using Spout;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "SpoutSender", 
				Category = "System", 
				Help = "Adds/Removes spout sender infos to the list of SpoutSenderNames", 
				Tags = "share",
				Credits = "https://github.com/ItayGal2/SpoutCSharp",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class VVVVSpoutSenderNode: IPluginEvaluate, IDisposable
	{
		#region fields & pins
		[Input("Sender Name", DefaultString = "vvvvideo")]
		public ISpread<string> FSenderName;
		
		[Input("Width")]
		public ISpread<uint> FWidth;
		
		[Input("Height")]
		public ISpread<uint> FHeight;
		
		[Input("Handle")]
		public ISpread<uint> FHandle;
		
		[Input("Write", IsBang=true)]
		public ISpread<bool> FWrite;

		[Import()]
		public ILogger FLogger;
		
		SpoutSender[] FSpoutSender = new SpoutSender[0];
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//if sender count has changed
			if (FSpoutSender.Length != SpreadMax)
			{
				//for simplicity remove all existing senders
				CleanUp();
				FLogger.Log(LogType.Debug, "Cleaned Up SpoutSender");
				
				//and create a new array
				FSpoutSender = new SpoutSender[SpreadMax];
			}	
			
			for (int i = 0; i < SpreadMax; i++)
			{
				if (FWrite[i])
				{
					if ((i < FSpoutSender.Length) && (FSpoutSender[i] != null))
						FSpoutSender[i].Dispose();
					
					FSpoutSender[i] = new SpoutSender(FSenderName[i], FHandle[i], FWidth[i], FHeight[i], 0, 1); 
					var succ = FSpoutSender[i].Initialize();
					FLogger.Log(LogType.Debug, "Writing Spout sender " + (succ ? "succeeded!" : "failed!"));
				}
			}
		}
		
		public void Dispose()
		{
			CleanUp();
		}
		
		void CleanUp()
		{
			foreach (var s in FSpoutSender)
				if (s != null)
					s.Dispose();
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "SpoutSenderNames", 
				Category = "System", 
				Help = "Show the list of SpoutSenderNames currently registered", 
				Tags = "share",
				Credits = "https://github.com/ItayGal2/SpoutCSharp")]
	#endregion PluginInfo
	public class VVVVSpoutSenderListNode: IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Update", IsSingle=true, IsBang=true)]
		public ISpread<bool> FUpdate;
		
		[Output("Output")]
		public ISpread<string> FOutput;
		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FOutput.AssignFrom(SpoutSender.GetSenderNames());
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FUpdate[0])
				FOutput.AssignFrom(SpoutSender.GetSenderNames());
		}
	}
}