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
				Credits = "https://github.com/ItayGal2/SpoutCSharp and Lynn Jarvis of http://spout.zeal.co",
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

        // TODO
        // [Input("Format")]
        // public ISpread<double> FFormat;

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
                    //if there was an existing one dispose it
					if ((i < FSpoutSender.Length) && (FSpoutSender[i] != null))
						FSpoutSender[i].Dispose();

                    //create the spoutsender

                    // Default format should be = 28 - DirectX 11 (DXGI_FORMAT_R8G8B8A8_UNORM)
                    FSpoutSender[i] = new SpoutSender(FSenderName[i], FHandle[i], FWidth[i], FHeight[i], 28, 0);

                    // TODO - create format pin and pass texture format in as integer
                    // UInt32 format = Convert.ToUInt32(FFormat[i].ToString());
                    // FSpoutSender[i] = new SpoutSender(FSenderName[i], FHandle[i], FWidth[i], FHeight[i], format, 0);

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
				Help = "Shows the list of SpoutSenderNames currently registered", 
				Credits = "https://github.com/ItayGal2/SpoutCSharp and Lynn Jarvis of http://spout.zeal.co")]
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