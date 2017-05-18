#region licence/info

//////project name
//2d gui nodes

//////description
//nodes to build 2d guis in a EX9 renderer

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VMath;

//////initial author
//tonfilm

#endregion licence/info

//use what you need
using System;
using System.ComponentModel.Composition;
using System.Collections;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.Utils.IO;


namespace VVVV.Nodes
{
	//parent class for gui2d nodes
	public class BasicGui2dNode: IPluginEvaluate
	{
		#region field declaration

		//the host
		[Import]
		protected IHDEHost FHost;
		
		//input pin declaration
		[Input("Transform In", Order = -20)]
		protected IDiffSpread<Matrix4x4> FTransformIn;
		
		[Input("Set Value", IsBang = true, Order = -10)]
		protected IDiffSpread<bool> FSetValueIn;
		
		[Input("Count", DefaultValues = new double[] { 1, 1 }, AsInt = true)]
		protected IDiffSpread<Vector2D> FCountIn;
		
		[Input("Size", DefaultValues = new double[] { 0.9, 0.9 })]
		protected IDiffSpread<Vector2D> FSizeIn;
		
		[Input("Mouse")]
		protected IDiffSpread<MouseState> FMouseIn;
		
		[Input("Color", DefaultColor = new double[] { 0.2, 0.2, 0.2, 1 })]
		protected IDiffSpread<RGBAColor> FColorIn;

		[Input("Mouse Over Color", DefaultColor = new double[] { 0.5, 0.5, 0.5, 1 })]
		protected IDiffSpread<RGBAColor> FOverColorIn;
		
		[Input("Activated Color", DefaultColor = new double[] { 1, 1, 1, 1 })]
		protected IDiffSpread<RGBAColor> FActiveColorIn;
		
		//output pin declaration
		
		[Output("Transform Out")]
		protected ISpread<Matrix4x4> FTransformOut;
		
		[Output("Color")]
		protected ISpread<RGBAColor> FColorOut;
		
		[Output("Active")]
		protected ISpread<bool> FActiveOut;
		
		[Output("Hit", IsBang = true)]
		protected ISpread<bool> FHitOut;
		
		[Output("Mouse Over")]
		protected ISpread<bool> FMouseOverOut;
		
		[Output("Spread Counts")]
		protected ISpread<double> FSpreadCountsOut;
		
		//internal fields
		protected ArrayList FControllerGroups;
		protected Spread<bool> FLastMouseLeft;
		protected bool FFirstframe = true;
		
		#endregion field declaration
		
		public BasicGui2dNode()
		{
			FControllerGroups = new ArrayList();
			FLastMouseLeft = new Spread<bool>(1);
		}
			
		
		#region mainloop
		
		public virtual void Evaluate(int SpreadMax)
		{
		}
		
		#endregion mainloop
		
		protected bool AnyParameterPinChanged()
		{
			return FCountIn.IsChanged
			    || FSizeIn.IsChanged
			    || FTransformIn.IsChanged
			    || FColorIn.IsChanged
			    || FOverColorIn.IsChanged
			    || FActiveColorIn.IsChanged;
		}
		
		protected bool AnyMouseUpdatePinChanged()
		{
			return FMouseIn.IsChanged
			    || FColorIn.IsChanged
			    || FOverColorIn.IsChanged
			    || FActiveColorIn.IsChanged
			    || FCountIn.IsChanged
			    || FLastMouseLeft.IsChanged
			    || FSetValueIn.IsChanged;
		}
		
		//calc how many groups are required
		protected virtual int GetSpreadMax()
		{
			
			int max = 0;
			max = Math.Max(max, FCountIn.SliceCount);
			max = Math.Max(max, FSizeIn.SliceCount);
			max = Math.Max(max, FTransformIn.SliceCount);
			max = Math.Max(max, FColorIn.SliceCount);
			max = Math.Max(max, FActiveColorIn.SliceCount);
			return Math.Max(max, FOverColorIn.SliceCount);
		}
		
		protected bool UpdateMouse<TGroup, TController>(int inputSpreadCount) where TGroup : BasicGui2dGroup<TController> where TController : BasicGui2dController, new()
		{
			bool valueSet = false;
			if ( AnyMouseUpdatePinChanged() )
			{
				var mouseCount = FMouseIn.SliceCount;
				FLastMouseLeft.SliceCount = mouseCount;
				for (int mouseSlice = 0; mouseSlice < mouseCount; mouseSlice++)
				{
					var mouse = FMouseIn[mouseSlice];
					
					bool mousDownEdge = mouse.IsLeft && !FLastMouseLeft[mouseSlice];
					
					for (int slice = 0; slice < inputSpreadCount; slice++)
					{
						TGroup group = (TGroup) FControllerGroups[slice];
						group.IsMultiTouch = mouseCount > 1;
						valueSet |= group.UpdateMouse(mouse.Position, mousDownEdge, mouse.IsLeft);
					}
					
					FLastMouseLeft[mouseSlice] = mouse.IsLeft;
				}
			}
			
			return valueSet;
		}
		
	}
	

	public class BasicGui2dSliderNode : BasicGui2dNode
	{
		#region field declaration
		
		//additional slider color pin
		
		[Input("Slider Color", DefaultColor = new double[] { 1, 1, 1, 1 })]
		protected IDiffSpread<RGBAColor> FSliderColorIn;
		
		[Input("Slider Speed", DefaultValue = 0.25)]
		protected IDiffSpread<double> FSliderSpeedIn;
		
		#endregion field declaration
		
		//calc how many groups are required
		protected override int GetSpreadMax()
		{
			
			int max = base.GetSpreadMax();
			
			max = Math.Max(max, FSliderColorIn.SliceCount);
			return Math.Max(max, FSliderSpeedIn.SliceCount);
			
		}
		
	}

}

