#region licence/info

//////project name
//Lindenmayer

//////description
//Returns spreads for 3 dimensional L-Systems.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils;

//////initial author
//vvvv group

#endregion licence/info

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
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Lindenmayer";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Spreads";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Returns spreads for 3 dimensional L-Systems. F draws a stick; + rotates +Z; - rotates -Z; / rotates +Y; \\ rotates -Y; [ opens a branch; ] closes a branch";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "Tree, Plant, Grow";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "based on findings by Aristid Lindenmayer";
					//any known problems?
					FPluginInfo.Bugs = "";
					//any known usage of the node that may cause troubles?
					FPluginInfo.Warnings = "";
					
					//leave below as is
					System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
					System.Diagnostics.StackFrame sf = st.GetFrame(0);
					System.Reflection.MethodBase method = sf.GetMethod();
					FPluginInfo.Namespace = method.DeclaringType.Namespace;
					FPluginInfo.Class = method.DeclaringType.Name;
					//leave above as is
				}
				return FPluginInfo;				
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
			
			//FHost.Log(TLogType.Debug, SpreadMax.ToString());
			
			if (FOldSpreadMax != SpreadMax)
			{
				FHost.Log(TLogType.Debug, "spreadcount changed: " + SpreadMax.ToString());
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
					FLindenmayers[i].ProductionsF.Clear();
					FProductionsF.GetString(i, out s);
					
					if (!string.IsNullOrEmpty(s))
						s = s.Trim(); //whitespaces
					
					if (!string.IsNullOrEmpty(s))
					{
						char[] split = {','};
						string[] sa = s.Split(split);
						
						for (int j=0; j<sa.Length; j++)
							FLindenmayers[i].ProductionsF.Add(sa[j]);
					}
				}
				
				revaluate = true;
			}
			
			if (FProductionsG.PinIsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FLindenmayers[i].ProductionsG.Clear();
					FProductionsG.GetString(i, out s);

					if (!string.IsNullOrEmpty(s))
						s = s.Trim(); //whitespaces
					
					if (!string.IsNullOrEmpty(s))
					{
						char[] split = {','};
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

