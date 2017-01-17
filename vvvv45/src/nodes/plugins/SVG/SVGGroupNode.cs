#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;

using SlimDX;
using Svg;
using Svg.Transforms;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.SlimDX;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Group", 
	            Category = "SVG", 
	            Help = "Groups multiple SVG layers to be rendered one after the other", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGGroupNode : SVGVisualElementNode<SvgElement>
	{
		#region fields & pins
		#pragma warning disable 649,169
		
		[Input("Layer", IsPinGroup=true)]
		IDiffSpread<ISpread<SvgElement>> FInput;
	
		#pragma warning restore
		
		List<SvgElement> FGroups = new List<SvgElement>();
		#endregion fields & pins
		
		public SVGGroupNode()
		{
			var g = new SvgGroup();
			FGroups.Add(g);
		}
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}
	
		//called when data for any output pin is requested
		public override void Evaluate(int SpreadMax)
		{
			//check transforms
			if(FTransformIn.IsChanged)
			{
				//assign size and clear group list
				FOutput.SliceCount = FTransformIn.SliceCount;
				FGroups.Clear();
				
				//create groups and add matrix to it
				for(int i=0; i<FTransformIn.SliceCount; i++)
				{
					var g = new SvgGroup();
					g.Transforms = new SvgTransformCollection();
					
					var m = FTransformIn[i];
					var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
					
					g.Transforms.Add(mat);	
					
					FGroups.Add(g);
				}
			}
			
			//add all elements to each group
			var pinsChanged = FInput.IsChanged || FTransformIn.IsChanged || FEnabledIn.IsChanged;
			if(pinsChanged)
			{
				foreach (var g in FGroups)
				{
					g.Children.Clear();
					
					if(FEnabledIn[0])
					{
						for(int i=0; i<FInput.SliceCount; i++)
						{
							var pin = FInput[i];
							for(int j=0; j<pin.SliceCount; j++)
							{
								var elem = pin[j];
								if(elem != null)
									g.Children.AddAndForceUniqueID(elem, true, true, LogIDFix);
							}
						}
					}
					
				}
				
				//write groups to output
				FOutput.AssignFrom(FGroups);
			}
			
			//set id and class to elements
			if(pinsChanged || FIDIn.IsChanged || FClassIn.IsChanged)
				base.SetIDs();
	
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Join", 
	            Category = "SVG",
	            Help = "Joins multiple SVG layers into a group", 
	            Tags = "")]
	#endregion PluginInfo
	public class SVGJoinNode : SVGVisualElementNode<SvgElement>
	{
		#region fields & pins
		#pragma warning disable 649,169
		
		[Input("Layer", IsPinGroup=true)]
		IDiffSpread<ISpread<SvgElement>> FInput;
	
		#pragma warning restore
		
		List<SvgElement> FGroups = new List<SvgElement>();
		#endregion fields & pins
		
		public SVGJoinNode()
		{
			var g = new SvgGroup();
			FGroups.Add(g);
		}
		
		void LogIDFix(SvgElement elem, string oldID, string newID)
		{
			var msg = "ID of " + elem + " was changed from " + oldID + " to " + newID;
			FLogger.Log(LogType.Warning, msg);
		}
	
		//called when data for any output pin is requested
		public override void Evaluate(int SpreadMax)
		{
			//check slicecount and create groups
			if(FGroups.Count != SpreadMax)
			{
				//assign size and clear group list
				FOutput.SliceCount = SpreadMax;
				FGroups.Clear();
				
				//create groups and add matrix to it
				for(int i=0; i<SpreadMax; i++)
				{
					var g = new SvgGroup();
					g.Transforms = new SvgTransformCollection();

					var m = FTransformIn[i];
					var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
					
					g.Transforms.Add(mat);

					FGroups.Add(g);
				}
			}
			
			if(FTransformIn.IsChanged)
			{
				for(int i=0; i<SpreadMax; i++)
				{
					var g = FGroups[i];
					
					var m = FTransformIn[i];
					var mat = new SvgMatrix(new List<float>(){m.M11, m.M12, m.M21, m.M22, m.M41, m.M42});
					
					g.Transforms[0] = mat;
				}
			}
			
			//add all elements to each group
			if(FInput.IsChanged || FEnabledIn.IsChanged)
			{
				for(int i=0; i<SpreadMax; i++)
				{
					var g = FGroups[i];
					g.Children.Clear();
					
					if(FEnabledIn[0])
					{
						//each pin
						for(int j=0; j<FInput.SliceCount; j++)
						{
							var pin = FInput[j];
							var elem = pin[i];
							if(elem != null)
								g.Children.AddAndForceUniqueID(elem, true, true, LogIDFix);
						}
					}
				}
				
				//write groups to output
				FOutput.AssignFrom(FGroups);
			}
			
			//set id and class to elements
			var pinsChanged = FInput.IsChanged || FTransformIn.IsChanged || FEnabledIn.IsChanged;
			if(pinsChanged || FIDIn.IsChanged || FClassIn.IsChanged)
				base.SetIDs();
	
		}
	}
}
