//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Nodes.Lindenmayer;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Lindenmayer",
                Category = "Spreads",
                Author = "vvvv group",
                Help = "Returns spreads for 3 dimensional L-Systems. F draws a stick; + rotates +Z; - rotates -Z; / rotates +Y; \\ rotates -Y; [ opens a branch; ] closes a branch",
                Tags = "tree, plant, grow",
                Credits = "based on findings by Aristid Lindenmayer")]
    #endregion PluginInfo
	public class LindenmayerPlugin: IPluginEvaluate
	{
		#region fields & pins
		[Input("Axiom", DefaultString = "F")]
        protected IDiffSpread<string> FAxiom;
        
        [Input("Productions F", DefaultString = "F+F")]
        protected IDiffSpread<string> FProductionsF;
        
        [Input("Productions G", DefaultString = "")]
        protected IDiffSpread<string> FProductionsG;
        
        [Input("Depth", DefaultValue = 1, MinValue = 0, MaxValue = 10)]
        protected IDiffSpread<int> FDepth;
        
        [Input("Seed", DefaultValue = 1, MinValue = 0)]
        protected IDiffSpread<int> FSeed;
        
        [Input("Branch Length", DefaultValue = 0.2, MinValue = 0)]
        protected IDiffSpread<double> FBranchLength;
        
        [Input("Length Deviation", DefaultValue = 0.5, MinValue = 0)]
        protected IDiffSpread<double> FBranchLengthDeviation;
        
        [Input("Angle", DefaultValue = 0.1, MinValue = 0, MaxValue = 1)]
        protected IDiffSpread<double> FAngle;
        
        [Input("Angle Deviation", DefaultValue = 0, MinValue = 0, MaxValue = 1)]
        protected IDiffSpread<double> FAngleDeviation;
        
        [Output("Level")]
        protected ISpread<int> FLevel;
        
        [Output("G at F Slice")]
        protected ISpread<int> FGAtFSlice;
        
        [Output("Bin Sizes")]
        protected ISpread<int> FBinSizes;
        
        [Output("Bin Sizes G")]
        protected ISpread<int> FBinSizesG;
        
        [Output("Transform")]
        protected ISpread<Matrix4x4> FTransform;
        
        [Output("Transform G")]
        protected ISpread<Matrix4x4> FTransformG;

		private List<TLindenmayer> FLindenmayers = new List<TLindenmayer>();
		private int FOldSpreadMax = 0;
		
		#endregion field declaration
		
		public void Evaluate(int SpreadMax)
		{
			bool revaluate = false;
			
			if (FOldSpreadMax != SpreadMax)
			{
				//FHost.Log(TLogType.Debug, "spreadcount changed: " + SpreadMax.ToString());
				FLindenmayers.Clear();
				
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers.Add(new TLindenmayer());
				
				FOldSpreadMax = SpreadMax;
				revaluate = true;
			}
			
			if (FAxiom.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].Axiom = FAxiom[i];
				
				revaluate = true;
			}

			if (FProductionsF.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FLindenmayers[i].ProductionsF.Clear();
					
					string s = FProductionsF[i].Trim(); 
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
			
			if (FProductionsG.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
				{
					FLindenmayers[i].ProductionsG.Clear();
					
					string s = FProductionsG[i].Trim(); 
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
			
			if (FDepth.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].Depth = FDepth[i];
				
				revaluate = true;
			}
			
			if (FSeed.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].Seed = FSeed[i];
				
				revaluate = true;
			}

			if (FBranchLength.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].BranchLength = FBranchLength[i];
				
				revaluate = true;
			}
			
			if (FBranchLengthDeviation.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].BranchLengthDeviation = FBranchLengthDeviation[i];
				
				revaluate = true;
			}
			
			if (FAngle.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].Angle = FAngle[i];
				
				revaluate = true;
			}
			
			if (FAngleDeviation.IsChanged || revaluate)
			{
				for (int i=0; i<SpreadMax; i++)
					FLindenmayers[i].AngleDeviation = FAngleDeviation[i];
				
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
				    FBinSizes[i] = FLindenmayers[i].BranchCount;
					for (int j=0; j<FLindenmayers[i].BranchCount; j++)
					{
					    FTransform[sliceF] = (Matrix4x4) FLindenmayers[i].Transforms[j];
					    FLevel[sliceF] = (int) FLindenmayers[i].Level[j];
						sliceF++;
					}
					
					FBinSizesG[i] = FLindenmayers[i].TransformsG.Count;
					for (int j=0; j<FLindenmayers[i].TransformsG.Count; j++)
					{
					    FTransformG[sliceG] = (Matrix4x4) FLindenmayers[i].TransformsG[j];
					    FGAtFSlice[sliceG] = (int) FLindenmayers[i].GAtFSlice[j];
						sliceG++;
					}
				}
			}
		}
	}
}