#region usings
using System;
using System.ComponentModel.Composition;
using System.Text;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;

using Spout;

#endregion usings

namespace VVVV.Nodes
{
	
	public enum myEnum
	{
		Float,
		Toggle,
		Press,
		Text
	}
	#region PluginInfo
	[PluginInfo(Name = "SpoutSender", 
				Category = "System", 
				Help = "Adds/Removes spout sender infos to the list of SpoutSenderNames", 
				Credits = "https://github.com/ItayGal2/SpoutCSharp and Lynn Jarvis of http://spout.zeal.co",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public unsafe class SystemSpoutSenderNode: IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
	{

        static SystemSpoutSenderNode()
        {
            var platform = IntPtr.Size == 4 ? "x86" : "x64";
            var pathToThisAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToBinFolder = Path.Combine(pathToThisAssembly, "dependencies", platform);
            var envPath = Environment.GetEnvironmentVariable("PATH");
            envPath = string.Format("{0};{1}", envPath, pathToBinFolder);
            Environment.SetEnvironmentVariable("PATH", envPath);
        }


        #region fields & pins

        [Config("Enable Controls", DefaultValue = 0)]   //TO DO: create pins dynamically?
        protected IDiffSpread<bool> FControl;
		
		[Input("Sender Name", DefaultString = "vvvvideo")]
		public ISpread<String> FSenderName;
		
		[Input("Width")]
		public ISpread<uint> FWidth;
		
		[Input("Height")]
		public ISpread<uint> FHeight;
		
		[Input("Handle")]
		public ISpread<uint> FHandle;
		
		[Input("Write")]
		public ISpread<bool> FWrite;
		
		[Input("Control Name", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<String>> FName;
		
		[Input("Control Type", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<myEnum>> FType;
		
		[Input("Float Defaults", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<float>> FFloat;
		
		[Input("Toggle Defaults", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<bool>> FToggle;
		
//		[Input("Bang Defaults")]//not needed
//		public ISpread<ISpread<bool>> FPress;
		
		[Input("Text Defaults", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<String>> FTxt;
		
		[Output("Float", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<double>> FF;
		
		[Output("Toggle", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<bool>> FT;
		
		[Output("Press", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<bool>> FP;
		
		[Output("Text", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<string>> FS;

		[Import()]
		public ILogger FLogger;
		
		SpoutSender[] FSpoutSender = new SpoutSender[0];
		#endregion fields & pins
		
		[System.Runtime.InteropServices.DllImport("SpoutControls4vvvv.dll")]
		private static extern void InitializeControls(String sendername, int[] numControls,String[] names, int[] types, float[] floats, float[] toggles, float[] press, String[] text);
		[System.Runtime.InteropServices.DllImport("SpoutControls4vvvv.dll")]
		private static extern int CloseControls();
		[System.Runtime.InteropServices.DllImport("SpoutControls4vvvv.dll")]
		private static extern bool UpdateControls([In, Out] String[] text, [In, Out] float[] floats,  [In, Out] float[] toggles, [In, Out] float[] press, [In, Out] int[] numControls);
		[System.Runtime.InteropServices.DllImport("SpoutControls4vvvv.dll")]
		private static extern int ReleaseMemory([In, Out] float[] ptr);
		
		public void OnImportsSatisfied() 
  		{
  		}
				
		public static IntPtr NativeUtf8FromString(String managedString) {
        	int len = Encoding.UTF8.GetByteCount(managedString);
        	byte[] buffer = new byte[len + 1];
        	Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
        	IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        	Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
       	 	return nativeUtf8;
    		}
		
		public void SpreadToArray<T>(T[] I,ISpread<T> Spread,int range){
				for (int i=0; i<range; i++){
				I[i]=Spread[i];
			}
		}
		
		public void EnumSpreadToArray(String[] I,ISpread<myEnum> Spread,int range){
				for (int i=0; i<range; i++){
				I[i]=Enum.GetName(typeof(myEnum), Spread[i]);
			}
		}
		
		public void EnumSpreadToIndexArray(int[] I,ISpread<myEnum> Spread,int range){
				for (int i=0; i<range; i++){
				I[i]=(int)Spread[i];
			}
		}
		
		public int[] countControls(ISpread<myEnum> Spread){
				int[] retArr= new int[4];
				int count = Spread.SliceCount;
				for(int i=0;i<count;i++){
					if (Enum.GetName(typeof(myEnum), Spread[i]) == "Float") retArr[0]++;
					if (Enum.GetName(typeof(myEnum), Spread[i]) == "Toggle") retArr[1]++;
					if (Enum.GetName(typeof(myEnum), Spread[i]) == "Press") retArr[2]++;
					if (Enum.GetName(typeof(myEnum), Spread[i]) == "Text") retArr[3]++;
				}			
			return retArr;
		}	
		
		public int sumControls(int[] counts){
				int count = counts.Length;
				int result=0;
				for(int i=0;i<count;i++){
					result+=counts[i];
				}
			return result;
		}
		
		public bool DoubleToBool(double input){
				bool result=false;
				if (input>0){
					result=true;
				}
			return result;
		}
		
		public bool FloatToBool(float input){
				bool result=false;
				if (input>0){
					result=true;
				}
			return result;
		}
		
		public void BoolSpreadToDoubleArray(double[] I,ISpread<bool> Spread,int range){
				for (int i=0; i<range; i++){
				I[i]=System.Convert.ToDouble(Spread[i]);
			}
		}
		
		public void BoolSpreadToFloatArray(float[] I,ISpread<bool> Spread,int range){
				for (int i=0; i<range; i++){
				I[i]=System.Convert.ToSingle(Spread[i]);
			}
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = SpreadUtils.SpreadMax(FSenderName,FWidth,FHeight,FHandle,FWrite,FName,FType,FFloat,FToggle,FTxt);
				
			FF.SliceCount=SpreadMax;
			FT.SliceCount=SpreadMax;
			FP.SliceCount=SpreadMax;
			FS.SliceCount=SpreadMax;
			
			//if sender count has changed
			if (FSpoutSender.Length != SpreadMax)
			{
				//for simplicity remove all existing senders
				CleanUp();
				FLogger.Log(LogType.Debug, "Cleaned Up SpoutSender");
				CloseControls();
				
				//and create a new array
				FSpoutSender = new SpoutSender[SpreadMax];
			}	
			
			for (int i = 0; i < SpreadMax; i++)
			{			
				if (FWrite[i])
				{					
                    //if there was an existing one dispose it
					if ((i < FSpoutSender.Length) && (FSpoutSender[i] != null)){
						FSpoutSender[i].Dispose();
						CloseControls();
					}
					
                    //create the spoutsender
					FSpoutSender[i] = new SpoutSender(FSenderName[i], FHandle[i], FWidth[i], FHeight[i], 21, 0); 
					var succ = FSpoutSender[i].Initialize();
					FLogger.Log(LogType.Debug, "Writing Spout sender " + (succ ? "succeeded!" : "failed!"));
				}
				
				if (FControl[i])
				{				
					int[] controls = countControls(FType[i]);
					int all = sumControls(controls);
					
					FF[i].SliceCount=controls[0];
					FT[i].SliceCount=controls[1];
					FP[i].SliceCount=controls[2];
					FS[i].SliceCount=controls[3];
					
					//copy spreads to arrays				
					String[] names = new String[all];
					SpreadToArray(names,FName[i],all);
					
					int[] types = new int[all];
					EnumSpreadToIndexArray(types,FType[i],all);
					
					float[] floats = new float[controls[0]];
					SpreadToArray(floats,FFloat[i],controls[0]);
					
					float[] toggles = new float[controls[1]];
					BoolSpreadToFloatArray(toggles,FToggle[i],controls[1]);
					
					float[] press = new float[controls[2]];
					
					String[] text = new String[controls[3]];
					SpreadToArray(text,FTxt[i],controls[3]);
					
					//initialze return arrays
					String[] Rtext = new String[controls[3]];
					float[] Rfloats = new float[controls[0]];
					float[] Rtoggles = new float[controls[1]];
					float[] Rpress = new float[controls[2]];
				
/*					try
						{			
*/							
						//initialize controls	
						if (FWrite[i]) InitializeControls(FSenderName[i],controls,names,types,floats,toggles,press,text);
							
						//update controls
						bool changed = UpdateControls(Rtext,Rfloats,Rtoggles,Rpress,controls);
						
						if (changed){
				
							for(int j=0; j<controls[0];j++){
							FF[i][j]=Rfloats[j];
							}			
							for(int j=0; j<controls[1];j++){
							FT[i][j]=FloatToBool(Rtoggles[j]);
							}
							for(int j=0; j<controls[3];j++){
							FS[i][j]=Rtext[j];
							}
						}
						
						for(int j=0; j<controls[2];j++){
							FP[i][j]=FloatToBool(Rpress[j]);
							}
							
/*						}
								
					finally
					{

					}*/
				}
			}	
		}
		
		public void Dispose()
		{
			CleanUp();
			CloseControls();
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
	public class SystemSpoutSenderListNode: IPluginEvaluate, IPartImportsSatisfiedNotification
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

