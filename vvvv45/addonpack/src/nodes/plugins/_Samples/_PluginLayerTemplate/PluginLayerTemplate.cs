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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	public struct DeviceFont
	{
		public SlimDX.Direct3D9.Font Font;
		public SlimDX.Direct3D9.Sprite Sprite;
	}
	
	//class definition
	public class PluginLayerTemplate: IPlugin, IDisposable, IPluginDXLayer
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private int FSpreadCount;
		private IStringIn FMyStringInput;
		
		private ITransformIn FWorldTransform;
		private IEnumIn FTransformSpace;
		
		//a layer output pin
		private IDXLayerIO FMyLayerOutput;
		
		//a list that holds a font for every device
		private Dictionary<int, DeviceFont> FDeviceFonts = new Dictionary<int, DeviceFont>();
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public PluginLayerTemplate()
		{
			//the nodes constructor
			//nothing to declare for this node
		}
		
		// Implementing IDisposable's Dispose method.
		// Do not make this method virtual.
		// A derived class should not be able to override this method.
		public void Dispose()
		{
			Dispose(true);
			// Take yourself off the Finalization queue
			// to prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}
		
		// Dispose(bool disposing) executes in two distinct scenarios.
		// If disposing equals true, the method has been called directly
		// or indirectly by a user's code. Managed and unmanaged resources
		// can be disposed.
		// If disposing equals false, the method has been called by the
		// runtime from inside the finalizer and you should not reference
		// other objects. Only unmanaged resources can be disposed.
		protected virtual void Dispose(bool disposing)
		{
			// Check to see if Dispose has already been called.
			if(!FDisposed)
			{
				if(disposing)
				{
					// Dispose managed resources.
				}
				// Release unmanaged resources. If disposing is false,
				// only the following code is executed.
				
				FHost.Log(TLogType.Debug, "PluginMeshTemplate is being deleted");
				
				// Note that this is not thread safe.
				// Another thread could start disposing the object
				// after the managed resources are disposed,
				// but before the disposed flag is set to true.
				// If thread safety is necessary, it must be
				// implemented by the client.
			}
			FDisposed = true;
		}

		// Use C# destructor syntax for finalization code.
		// This destructor will run only if the Dispose method
		// does not get called.
		// It gives your base class the opportunity to finalize.
		// Do not provide destructors in types derived from this class.
		~PluginLayerTemplate()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		#endregion constructor/destructor
		
		#region node name and info
		
		//provide node infos
		private static IPluginInfo FPluginInfo;
		public static IPluginInfo PluginInfo
		{
			get
			{
				if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Template";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Template";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Layer";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Offers a basic code layout to start from when writing a vvvv plugin";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
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
		
		#endregion node name and info
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create inputs
			FHost.CreateStringInput("Text", TSliceMode.Dynamic, TPinVisibility.True, out FMyStringInput);
			
			FHost.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FWorldTransform);
			
			FHost.CreateEnumInput("Space", TSliceMode.Single, TPinVisibility.OnlyInspector, out FTransformSpace);
			FTransformSpace.SetSubType("Spaces");			
			
			//create outputs
			FHost.CreateLayerOutput("Layer", TPinVisibility.True, out FMyLayerOutput);
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
			FSpreadCount = SpreadMax;
		}
		
		#endregion mainloop
		
		#region DXLayer
		private void RemoveResource(int OnDevice)
		{
			DeviceFont df = FDeviceFonts[OnDevice];
			FHost.Log(TLogType.Debug, "Destroying Resource...");
			FDeviceFonts.Remove(OnDevice);
			
			df.Font.Dispose();
			df.Sprite.Dispose();
		}
		
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			bool needsupdate = false;
			
			try
			{
				DeviceFont df = FDeviceFonts[OnDevice];
				if (FMyStringInput.PinIsChanged)
				{
					RemoveResource(OnDevice);
					needsupdate = true;
				}
			}
			catch
			{
				//if resource is not yet created on given Device, create it now
				needsupdate = true;
			}
			
			if (needsupdate)
			{
				FHost.Log(TLogType.Debug, "Creating Resource...");
				Device dev = Device.FromPointer(new IntPtr(OnDevice));
				
				DeviceFont df = new DeviceFont();
				df.Font = new SlimDX.Direct3D9.Font(dev, new System.Drawing.Font("Verdana", 10));
				df.Sprite = new Sprite(dev);
				FDeviceFonts.Add(OnDevice, df);

				//dispose device
				dev.Dispose();
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//Called by the PluginHost whenever a resource for a specific pin needs to be destroyed on a specific device. 
			//This is also called when the plugin is destroyed, so don't dispose dxresources in the plugins destructor/Dispose()
			
			try
			{
				RemoveResource(OnDevice);				
			}
			catch
			{
				//resource is not available for this device. good. nothing to do then.
			}
		}
		
		public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
		{
			//Called by the PluginHost everytime the plugin is supposed to render itself.
			//This is called from the PluginHost from within DirectX BeginScene/EndScene,
			//therefore the plugin shouldn't be doing much here other than some drawing calls.

			DeviceFont df = FDeviceFonts[DXDevice.DevicePointer()];
			
			DXDevice.SetSpace(FTransformSpace);
			
			Matrix4x4 world;
			
			SpriteFlags sf = SpriteFlags.DoNotAddRefTexture;// | SpriteFlags.ObjectSpace;
			
			string text;
			df.Sprite.Begin(sf);
			for (int i=0; i<FSpreadCount; i++)
			{
				FMyStringInput.GetString(i, out text);
				
				FWorldTransform.GetMatrix(i, out world);
				DXDevice.SetWorldTransform(world);
				
				df.Font.DrawString(df.Sprite, text, 0, i*10, new SlimDX.Color4(1, 1, 1, 1));
			}
			df.Sprite.End();
		}
		#endregion
	}
}
