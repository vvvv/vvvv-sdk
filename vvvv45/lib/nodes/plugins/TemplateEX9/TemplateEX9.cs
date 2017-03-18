#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.SlimDX;

#endregion usings

//here you can change the vertex type
using VertexType = VVVV.Utils.SlimDX.SimpleVertex;

namespace VVVV.Nodes
{
	//custom data per graphics device
	public class CustomDeviceData : DeviceData
	{
		//texture for this device
		public Texture Tex { get; set; }
		
		//vertex buffer for this device
		public VertexBuffer VB { get; set; }
		
		public CustomDeviceData(Texture tex, VertexBuffer vb)
			: base()
		{
			Tex = tex;
			VB = vb;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Template",
	            Category = "EX9",
	            Version = "",
	            Help = "Basic template which renders directly into the vvvv Renderer(EX9)",
	            Tags = "c#")]
	#endregion PluginInfo
	public class Template : DXLayerOutPluginBase<CustomDeviceData>, IPluginEvaluate
	{
		#region fields & pins
		
		[Input("Transform In")]
        public IDiffSpread<Matrix> FTransformIn;
		
		[Input("Vertex Positions")]
        public IDiffSpread<Vector3> FVertexIn;
		
		[Input("Texture Filename", StringType = StringType.Filename)]
        public IDiffSpread<string> FTexFileName;

		[Import]
        public ILogger FLogger;
		
		//slice count
		int FSpreadCount;
		
		#endregion fields & pins
		
		// import host and hand it to base constructor
		// the two booleans set whether to create a render state and/or sampler state pin
		[ImportingConstructor]
		public Template(IPluginHost host)
			: base(host, true, true)
		{
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//recreate device data if filename or slice count has changed
			if (FTexFileName.IsChanged || FSpreadCount != SpreadMax)
			{
				FSpreadCount = SpreadMax;
				Reinitialize();
			}
			
			//update vertex buffer
			if (FVertexIn.IsChanged)
			{
				//update device data, if vertex positions are changed
				Update();
			}
		}
		
		#region device data handling
		
		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override CustomDeviceData CreateDeviceData(Device device)
		{
		    FLogger.Log(LogType.Message, "Creating resource...");
		
			//create a vertex buffer with desired size
			var pool = device is DeviceEx ? Pool.Default : Pool.Managed;
			var vb = new VertexBuffer(device, FSpreadCount*Marshal.SizeOf(typeof(VertexType)), Usage.WriteOnly, VertexFormat.None, pool);
			
			//try to load the texture
			Texture tex;
			try
			{
				tex = Texture.FromFile(device, FTexFileName[0]);
			}
			catch
			{
				//set 1 pixel white texture if image load fails
				tex = TextureUtils.CreateColoredTexture(device, 1, 1, 0xFFFFFFFF);
			}
			
			//return a new device data
			return new CustomDeviceData(tex, vb);
		}
		
		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its data, here you fill the resources with the actual data
		protected override void UpdateDeviceData(CustomDeviceData deviceData)
		{
			//lock the vertexbuffer and get its data stream
			var stream = deviceData.VB.Lock(0, 0, LockFlags.None);

			//write the vertex data
			for (int i = 0; i < FSpreadCount; i++)
			{
				stream.Write(new VertexType(FVertexIn[i]));
			}

			//unlock the vertex buffer
			deviceData.VB.Unlock();
		}
		
		//this is called by vvvv to delete the resources of a specific device data
		protected override void DestroyDeviceData(CustomDeviceData deviceData, bool OnlyUnManaged)
		{
			deviceData.Tex.Dispose();
			deviceData.VB.Dispose();
		}
		
		#endregion device data handling
		
		//render into the vvvv renderer
		protected override void Render(Device device, CustomDeviceData deviceData)
		{
		    //enable simple sprite rendering
			device.SetRenderState(RenderState.PointSpriteEnable, true);
			device.SetRenderState(RenderState.PointScaleEnable, true);
			device.SetRenderState(RenderState.PointSize, 0.05f);
			
			//set vertex buffer
			device.SetStreamSource(0, deviceData.VB, 0, Marshal.SizeOf(typeof(VertexType)));
			
			//set vertex format
			device.VertexFormat = VertexType.Format;
			
			//set texture
			device.SetTexture(0, deviceData.Tex);
			
			for(int i=0; i<FSpreadCount; i++)
			{
				//set render state from pin
				FRenderStatePin.SetSliceStates(i);
				
				//set transform
				device.SetTransform(TransformState.World, FTransformIn[i]);
				
				//draw the geometry
				device.DrawPrimitives(PrimitiveType.PointList, 0, FSpreadCount);
			}
			
		}
	}
}
