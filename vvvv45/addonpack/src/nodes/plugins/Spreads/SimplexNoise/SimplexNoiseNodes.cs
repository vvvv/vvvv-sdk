#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class SimplexNoise2DNode: IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		
		//input pin declaration
		private IValueIn FPositionInput;
		private IValueIn FOctavesInput;
		private IValueIn FFrequencyInput;
		private IValueIn FPersistanceInput;
		
		//output pin declaration
		private IValueOut FOutput;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public SimplexNoise2DNode()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		~SimplexNoise2DNode()
		{
			//the nodes destructor
			//nothing to destruct
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
				IPluginInfo Info = new PluginInfo();
				Info.Name = "Simplex";							//use CamelCaps and no spaces
				Info.Category = "2d";						//try to use an existing one
				Info.Version = "";						//versions are optional. leave blank if not needed
				Info.Help = "Ken Perlins simplex noise";
				Info.Tags = "perlin, noise, spreads, 4d, random";
				Info.Author = "tonfilm";
				Info.Bugs = "";
				Info.Credits = "Noise function ported from a java code by Stefan Gustavson, Linköping University, Sweden, http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf";								
				Info.Warnings = "";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create inputs
			FHost.CreateValueInput("Position Input ", 2, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionInput);
			FPositionInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateValueInput("Octaves", 1, null, TSliceMode.Single, TPinVisibility.True, out FOctavesInput);
			FOctavesInput.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, true);
			
			FHost.CreateValueInput("Frequency", 1, null, TSliceMode.Single, TPinVisibility.True, out FFrequencyInput);
			FFrequencyInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Persistance", 1, null, TSliceMode.Single, TPinVisibility.True, out FPersistanceInput);
			FPersistanceInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			
			//create outputs
			FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
			FOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			

		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FPositionInput.PinIsChanged || FOctavesInput.PinIsChanged 
			    || FFrequencyInput.PinIsChanged || FPersistanceInput.PinIsChanged)
			{
				
				//first set slicecounts for all outputs
				//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
				FOutput.SliceCount = SpreadMax;
				
				double octaves, freq, pers;
				FOctavesInput.GetValue(0, out octaves);
				FFrequencyInput.GetValue(0, out freq);
				FPersistanceInput.GetValue(0, out pers);
				
				//the variable to fill with the input position
				Vector2D pos;
				
				//loop for all slices
				for (int i=0; i<SpreadMax; i++)
				{
					//read position from inputs
					FPositionInput.GetValue2D(i, out pos.x, out pos.y);

					//noise function per slice
					double noiseVal = 0;
					
					for (int o = 0; o <= (int)octaves; o++)
					{
						double comul = Math.Pow(freq, o);
					    noiseVal += SimplexNoise.noise(pos.x*comul, pos.y*comul) * Math.Pow(pers, o);
					}
					
					//write data to outputs
					FOutput.SetValue(i, noiseVal);
				}
			}
		}
		
		#endregion mainloop
	}
	
	public class SimplexNoise3DNode: IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		
		//input pin declaration
		private IValueIn FPositionInput;
		private IValueIn FOctavesInput;
		private IValueIn FFrequencyInput;
		private IValueIn FPersistanceInput;
		
		//output pin declaration
		private IValueOut FOutput;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public SimplexNoise3DNode()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		~SimplexNoise3DNode()
		{
			//the nodes destructor
			//nothing to destruct
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
				IPluginInfo Info = new PluginInfo();
				Info.Name = "Simplex";							//use CamelCaps and no spaces
				Info.Category = "3d";						//try to use an existing one
				Info.Version = "";						//versions are optional. leave blank if not needed
				Info.Help = "Ken Perlins simplex noise";
				Info.Tags = "perlin, noise, spreads, 4d, random";
				Info.Author = "tonfilm";
				Info.Bugs = "";
				Info.Credits = "Noise function ported from a java code by Stefan Gustavson, Linköping University, Sweden, http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf";								
				Info.Warnings = "";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create inputs
			FHost.CreateValueInput("Position Input ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionInput);
			FPositionInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateValueInput("Octaves", 1, null, TSliceMode.Single, TPinVisibility.True, out FOctavesInput);
			FOctavesInput.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, true);
			
			FHost.CreateValueInput("Frequency", 1, null, TSliceMode.Single, TPinVisibility.True, out FFrequencyInput);
			FFrequencyInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Persistance", 1, null, TSliceMode.Single, TPinVisibility.True, out FPersistanceInput);
			FPersistanceInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			
			//create outputs
			FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
			FOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			

		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FPositionInput.PinIsChanged || FOctavesInput.PinIsChanged 
			    || FFrequencyInput.PinIsChanged || FPersistanceInput.PinIsChanged)
			{
				
				//first set slicecounts for all outputs
				//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
				FOutput.SliceCount = SpreadMax;
				
				double octaves, freq, pers;
				FOctavesInput.GetValue(0, out octaves);
				FFrequencyInput.GetValue(0, out freq);
				FPersistanceInput.GetValue(0, out pers);
				
				//the variable to fill with the input position
				Vector3D pos;
				
				//loop for all slices
				for (int i=0; i<SpreadMax; i++)
				{
					//read position from inputs
					FPositionInput.GetValue3D(i, out pos.x, out pos.y, out pos.z);

					//noise function per slice
					double noiseVal = 0;
					
					for (int o = 0; o <= (int)octaves; o++)
					{
						double comul = Math.Pow(freq, o);
					    noiseVal += SimplexNoise.noise(pos.x*comul, pos.y*comul, pos.z*comul) * Math.Pow(pers, o);
					}
					
					//write data to outputs
					FOutput.SetValue(i, noiseVal);
				}
			}
		}
		
		#endregion mainloop
	}
	
	public class SimplexNoise4DNode: IPlugin
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		
		//input pin declaration
		private IValueIn FPositionInput;
		private IValueIn FOctavesInput;
		private IValueIn FFrequencyInput;
		private IValueIn FPersistanceInput;
		
		//output pin declaration
		private IValueOut FOutput;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public SimplexNoise4DNode()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		~SimplexNoise4DNode()
		{
			//the nodes destructor
			//nothing to destruct
		}
		
		#endregion constructor/destructor
		
		#region node name and infos
		
		//provide node infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
				IPluginInfo Info = new PluginInfo();
				Info.Name = "Simplex";							//use CamelCaps and no spaces
				Info.Category = "4d";						//try to use an existing one
				Info.Version = "";						//versions are optional. leave blank if not needed
				Info.Help = "Ken Perlins simplex noise";
				Info.Tags = "perlin, noise, spreads, 4d, random";
				Info.Author = "tonfilm";
				Info.Bugs = "";
				Info.Credits = "Noise function ported from a java code by Stefan Gustavson, Linköping University, Sweden, http://staffwww.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf";								
				Info.Warnings = "";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				Info.Namespace = method.DeclaringType.Namespace;
				Info.Class = method.DeclaringType.Name;
				return Info;
				//leave above as is
			}
		}
		
		public bool AutoEvaluate
		{
			//return true if this node needs to calculate every frame even if nobody asks for its output
			get {return false;}
		}
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create inputs
			FHost.CreateValueInput("Position Input ", 4, null, TSliceMode.Dynamic, TPinVisibility.True, out FPositionInput);
			FPositionInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateValueInput("Octaves", 1, null, TSliceMode.Single, TPinVisibility.True, out FOctavesInput);
			FOctavesInput.SetSubType(double.MinValue, double.MaxValue, 1.0, 0.0, false, false, true);
			
			FHost.CreateValueInput("Frequency", 1, null, TSliceMode.Single, TPinVisibility.True, out FFrequencyInput);
			FFrequencyInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Persistance", 1, null, TSliceMode.Single, TPinVisibility.True, out FPersistanceInput);
			FPersistanceInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.5, false, false, false);
			
			//create outputs
			FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
			FOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
			

		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		//here we go, thats the method called by vvvv each frame
		//all data handling should be in here
		public void Evaluate(int SpreadMax)
		{
			//if any of the inputs has changed
			//recompute the outputs
			if (FPositionInput.PinIsChanged || FOctavesInput.PinIsChanged 
			    || FFrequencyInput.PinIsChanged || FPersistanceInput.PinIsChanged)
			{
				
				//first set slicecounts for all outputs
				//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
				FOutput.SliceCount = SpreadMax;
				
				double octaves, freq, pers;
				FOctavesInput.GetValue(0, out octaves);
				FFrequencyInput.GetValue(0, out freq);
				FPersistanceInput.GetValue(0, out pers);
				
				//the variable to fill with the input position
				Vector4D pos;
				
				//loop for all slices
				for (int i=0; i<SpreadMax; i++)
				{
					//read position from inputs
					FPositionInput.GetValue4D(i, out pos.x, out pos.y, out pos.z, out pos.w);

					//noise function per slice
					double noiseVal = 0;
					
					for (int o = 0; o <= (int)octaves; o++)
					{
						double comul = Math.Pow(freq, o);
					    noiseVal += SimplexNoise.noise(pos.x*comul, pos.y*comul, pos.z*comul, pos.w*comul) * Math.Pow(pers, o);
					}
					
					//write data to outputs
					FOutput.SetValue(i, noiseVal);
				}
			}
		}
		
		#endregion mainloop
	}
	
}


