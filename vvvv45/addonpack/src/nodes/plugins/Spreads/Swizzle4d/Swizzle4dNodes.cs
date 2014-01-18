#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VMath;
#endregion usings

namespace VVVV.Nodes
{
	public unsafe abstract class Swizzle4dJoin1d : IPluginEvaluate
	{
		#region fields & pins
		[Input("", Dimension = 3)]
		private IValueFastIn Fxyz;

		[Input("W")]
		protected IValueFastIn FSingle;
		
		[Output("", Dimension = 4)]
		private IValueOut FOutPin;
		
		public int[] component = new int[]{0,1,2,3};
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			int xyzCount, wCount;
			double* xyzData, wData, outData;
			
			Fxyz.GetValuePointer(out xyzCount, out xyzData);
			FSingle.GetValuePointer(out wCount, out wData);
			
			if (xyzCount == 0 || wCount == 0)
				spreadMax = 0;
			else 
			{
				spreadMax =(int)Math.Ceiling((double)xyzCount/3);
				spreadMax = Math.Max(spreadMax, wCount);
			}
			
			FOutPin.SliceCount = spreadMax;
			FOutPin.GetValuePointer(out outData);
			
			for (int i=0; i<spreadMax; i++)
			{
				outData[(i*4+component[0])] = xyzData[(i*3+0)%xyzCount];
				outData[(i*4+component[1])] = xyzData[(i*3+1)%xyzCount];
				outData[(i*4+component[2])] = xyzData[(i*3+2)%xyzCount];
				outData[(i*4+component[3])] = wData[(i)%wCount];
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Xyzw", Category = "4d", Version = "YZW", Help = "Returns a 4d vector from a 3d vector and a X value", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dJoinXNode : Swizzle4dJoin1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{1,2,3,0};
			FSingle.Name = "X";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xYzw", Category = "4d", Version = "XZW", Help = "Returns a 4d vector from a 3d vector and a Y value", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dJoinYNode : Swizzle4dJoin1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{0,2,3,1};
			FSingle.Name = "Y";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xyZw", Category = "4d", Version = "XYW", Help = "Returns a 4d vector from a 3d vector and a Z value", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dJoinZNode : Swizzle4dJoin1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{0,1,3,2};
			FSingle.Name = "Z";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xyzW", Category = "4d", Version = "XYZ", Help = "Returns a 4d vector from a 3d vector and a W value", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dJoinWNode : Swizzle4dJoin1d {}
	
	
	public unsafe abstract class Swizzle4dSplit1d : IPluginEvaluate
	{
		#region fields & pins
		[Input("", Dimension = 4)]
		private IValueFastIn FInPin;
		
		[Output("", Dimension = 3)]
		private IValueOut Fxyz;
		
		[Output("W")]
		protected IValueOut FSingle;
		
		protected int[] component = new int[]{0,1,2,3};
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			int inCount;
			double* xyzData, wData, inData;
			
			FInPin.GetValuePointer(out inCount, out inData);
			spreadMax = (int)Math.Ceiling((double)inCount/4);
			
			Fxyz.SliceCount = spreadMax;
			Fxyz.GetValuePointer(out xyzData);
			FSingle.SliceCount = spreadMax;
			FSingle.GetValuePointer(out wData);
			
			for (int i=0; i<spreadMax; i++)
			{
				xyzData[(i*3+0)] = inData[(i*4+component[0])%inCount];
				xyzData[(i*3+1)] = inData[(i*4+component[1])%inCount];
				xyzData[(i*3+2)] = inData[(i*4+component[2])%inCount];
				wData[(i)] = inData[(i*4+component[3])%inCount];
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "yzw", Category = "3d", Help = "YZW coordinates as a 3d vector from a 4d vector", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dSplitX : Swizzle4dSplit1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{1,2,3,0};
			FSingle.Name = "X";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xzw", Category = "3d", Help = "XZW coordinates as a 3d vector from a 4d vector", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dSplitY : Swizzle4dSplit1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{0,2,3,1};
			FSingle.Name = "Y";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xyw", Category = "3d", Help = "XYW coordinates as a 3d vector from a 4d vector", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dSplitZ : Swizzle4dSplit1d, IPartImportsSatisfiedNotification
	{
		public void OnImportsSatisfied() 
		{
			component = new int[]{0,1,3,2};
			FSingle.Name = "Z";
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "xyz", Category = "3d", Help = "XYZ coordinates as a 3d vector from a 4d vector", Author="woei", Tags = "swizzle, vector")]
	#endregion PluginInfo
	public unsafe class Swizzle4dSplitW : Swizzle4dSplit1d {}
	
}