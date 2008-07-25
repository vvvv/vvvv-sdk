using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.Nodes.Lindenmayer;

namespace VVVV.Nodes
{
	public class LindenmayerPlugin: IPlugin
	{
		#region field declaration
		private IPluginHost FHost;

		private IValueIn FDepth;
		private IValueIn FBranchLength;
		private IValueIn FAngle;
		private IValueIn FAngleDeviation;
		private IValueIn FBranchLengthDeviation;
		private IValueIn FSeed;
		private IStringIn FProductionsF;
		private IStringIn FProductionsG;
		private IStringIn FAxiom;
		
		private IValueOut FLevel;
		private IValueOut FGAtFSlice;
		private ITransformOut FTransform;
		private ITransformOut FTransformG;
		private IValueOut FBinSizes;
		private IValueOut FBinSizesG;
		
		private List<TLindenmayer> FLindenmayers = new List<TLindenmayer>();
		private int FOldSpreadMax = 0;
		
		#endregion field declaration
		
		#region constructor/destructor
		public LindenmayerPlugin()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		~LindenmayerPlugin()
		{
			//the nodes destructor
			//nothing to destruct
		}
		#endregion constructor/destructor

		#region node name and infos
		public static IPluginInfo PluginInfo
		{
			get
			{
				//fill out nodes info
				IPluginInfo Info = new PluginInfo();
				Info.Name = "Lindenmayer";
				Info.Category = "Spreads";
				Info.Version = "";
				Info.Help = "Returns spreads for 3 dimensional L-Systems. F draws a stick; + rotates +Z; - rotates -Z; / rotates +Y; \\ rotates -Y; [ opens a branch; ] closes a branch;";
				Info.Bugs = "";
				Info.Credits = "based on findings by Aristid Lindenmayer";
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
		public void SetPluginHost(IPluginHost Host)
		{
			FHost = Host;

			//create inputs
			FHost.CreateStringInput("Axiom", TSliceMode.Dynamic, TPinVisibility.True, out FAxiom);
			FAxiom.SetSubType("F", false);
			
			FHost.CreateStringInput("Productions F", TSliceMode.Dynamic, TPinVisibility.True, out FProductionsF);
			FProductionsF.SetSubType("F+F", false);
			
			FHost.CreateStringInput("Productions G", TSliceMode.Dynamic, TPinVisibility.True, out FProductionsG);
			FProductionsG.SetSubType("", false);
			
			FHost.CreateValueInput("Depth", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDepth);
			FDepth.SetSubType(0, 10, 1, 1, false, false, true);
			
			FHost.CreateValueInput("Seed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSeed);
			FSeed.SetSubType(0, int.MaxValue, 1, 1, false, false, true);
			
			FHost.CreateValueInput("Branch Length", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBranchLength);
			FBranchLength.SetSubType(0, double.MaxValue, 0.01, 0.2, false, false, false);
			
			FHost.CreateValueInput("Length Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBranchLengthDeviation);
			FBranchLengthDeviation.SetSubType(0, double.MaxValue, 0.01, 0.5, false, false, false);
			
			FHost.CreateValueInput("Angle", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAngle);
			FAngle.SetSubType(0, 1, 0.01, 0.1, false, false, false);
			
			FHost.CreateValueInput("Angle Deviation", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FAngleDeviation);
			FAngleDeviation.SetSubType(0, 1, 0.01, 0, false, false, false);
			
			//create outputs
			FHost.CreateValueOutput("Level", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FLevel);
			FLevel.SetSubType(0, 1, 0.1, 0, false, false, false);
			
			FHost.CreateValueOutput("G at F Slice", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FGAtFSlice);
			FGAtFSlice.SetSubType(0, 1, 0.1, 0, false, false, false);
			
			FHost.CreateValueOutput("Bin Sizes", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSizes);
			FBinSizes.SetSubType(0, int.MaxValue, 1, 0, false, false, true);
			
			FHost.CreateValueOutput("Bin Sizes G", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBinSizesG);
			FBinSizesG.SetSubType(0, int.MaxValue, 1, 0, false, false, true);
			
			FHost.CreateTransformOutput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransform);
			FHost.CreateTransformOutput("Transform G", TSliceMode.Dynamic, TPinVisibility.True, out FTransformG);
		}
		
		#endregion pin creation
		
		#region mainloop
		
		public void Configurate(IPluginConfig Input)
		{
			//nothing to configure in this plugin
			//only used in conjunction with inputs of type cmpdConfigurate
		}
		
		public void Evaluate(int SpreadMax)
		{
			string s;
			double val;
			bool revaluate = false;
			
			if (FOldSpreadMax != SpreadMax)
			{
				FLindenmayers.Clear();
				
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers.Add(new TLindenmayer());
				
				FOldSpreadMax = SpreadMax;
				revaluate = true;
			}
			
			if (FAxiom.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FAxiom.GetString(i, out s);
					FLindenmayers[i].Axiom = s;
				}
				
				revaluate = true;
			}

			if (FProductionsF.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FProductionsF.GetString(i, out s);
					FLindenmayers[i].ProductionsF.Clear(); 
					
					char[] split = {','};
					//s = s.Trim(split);
					//s = s.Trim(); //whitespaces
					string[] sa = s.Split(split);
					for (int j=0; j<sa.Length; j++)
						FLindenmayers[i].ProductionsF.Add(sa[j]);
				}
				
				revaluate = true;
			}
		
			if (FProductionsG.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FProductionsG.GetString(i, out s);
					FLindenmayers[i].ProductionsG.Clear(); 
					
					System.Diagnostics.Debug.WriteLine("s: " + s);
					char[] split = {','};
					//s = s.Trim(split);
					//s = s.Trim(); //whitespaces
					if (s != " ")
					{
						System.Diagnostics.Debug.WriteLine("sss: " + s);
						string[] sa = s.Split(split);
						
						for (int j=0; j<sa.Length; j++)
							FLindenmayers[i].ProductionsG.Add(sa[j]);
					}
				}
				
				revaluate = true;
			}
			
			if (FDepth.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FDepth.GetValue(i, out val);
					FLindenmayers[i].Depth = (int) val;
				}
				
				revaluate = true;
			}
			
			if (FSeed.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FSeed.GetValue(i, out val);
					FLindenmayers[i].Seed = (int) val;
				}
				
				revaluate = true;
			}

			if (FBranchLength.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FBranchLength.GetValue(i, out val);
					FLindenmayers[i].BranchLength = val;
				}
				
				revaluate = true;
			}
			
			if (FBranchLengthDeviation.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FBranchLengthDeviation.GetValue(i, out val);
					FLindenmayers[i].BranchLengthDeviation = val;
				}
				
				revaluate = true;
			}
			
			if (FAngle.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FAngle.GetValue(i, out val);
					FLindenmayers[i].Angle = val;
				}
				
				revaluate = true;
			}
			
			if (FAngleDeviation.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FAngleDeviation.GetValue(i, out val);
					FLindenmayers[i].AngleDeviation = val;
				}
				
				revaluate = true;
			}
			
			
			if (revaluate)
			{
				int c = 0;
				int gs = 0;
				foreach (TLindenmayer lm in FLindenmayers)
				{
					lm.Evaluate();
					c += lm.BranchCount;
					gs += lm.TransformsG.Count;
				}

				FTransform.SliceCount = c;
				FTransformG.SliceCount = gs;
				FGAtFSlice.SliceCount = gs;
				FLevel.SliceCount = c;
				FBinSizes.SliceCount = FLindenmayers.Count;
				FBinSizesG.SliceCount = FLindenmayers.Count;

				int sliceF = 0;
				int sliceG = 0;
				for (int i=0; i<SpreadMax; i++)
				{
					FBinSizes.SetValue(i, FLindenmayers[i].BranchCount);

					for (int j=0; j<FLindenmayers[i].BranchCount; j++)
					{
						FTransform.SetMatrix(sliceF, (Matrix4x4) FLindenmayers[i].Transforms[j]);
						FLevel.SetValue(sliceF, (int) FLindenmayers[i].Level[j]);
						sliceF++;
					}
					
					FBinSizesG.SetValue(i, FLindenmayers[i].TransformsG.Count);
					for (int j=0; j<FLindenmayers[i].TransformsG.Count; j++)
					{
						FTransformG.SetMatrix(sliceG, (Matrix4x4) FLindenmayers[i].TransformsG[j]);
						FGAtFSlice.SetValue(sliceG, (int) FLindenmayers[i].GAtFSlice[j]);
						sliceG++;
					}
				}
			}
		}
	}
	#endregion mainloop
}

