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
	//class definition
	public class PluginMeshTemplate: IPlugin, IDisposable, IPluginDXMesh
	{
		#region field declaration
		
		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		private IDXMeshOut FMyMeshOutput;
		
		private List<Mesh> FDeviceMeshes = new List<Mesh>();
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public PluginMeshTemplate()
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
				
				if (FHost != null)
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
		~PluginMeshTemplate()
		{
			// Do not re-create Dispose clean-up code here.
			// Calling Dispose(false) is optimal in terms of
			// readability and maintainability.
			Dispose(false);
		}
		#endregion constructor/destructor
		
		#region node name and infos
		
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
					FPluginInfo.Version = "Mesh";
					
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
		
		#endregion node name and infos
		
		#region pin creation
		
		//this method is called by vvvv when the node is created
		public void SetPluginHost(IPluginHost Host)
		{
			//assign host
			FHost = Host;
			
			//create outputs
			FHost.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out FMyMeshOutput);
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
			FMyMeshOutput.SliceCount = SpreadMax;
			
			//change some positions
				py += sy;
				if (py > 0.5f) sy = -0.01f;
				if (py < 0.0f) sy = 0.01f;
				
				v3Vx[0].Y = py;
				v3Vx[1].X = 1.0f+py;
				v3Vx[2].Y = 1.0f;
											
				Ix[0] = 0;
				Ix[1] = 1;
				Ix[2] = 2;
				
				FMyMeshOutput.MarkPinAsChanged();
		}
		
		#endregion mainloop
		
		#region DXMesh
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) {return ms.Device.ComPointer == (IntPtr)OnDevice;});
			
			if (m != null) //if resource is created
			{
				Device dev = Device.FromPointer(new IntPtr(OnDevice));
							
				
				// create new Mesh
				Mesh g = new Mesh(dev, 1, 3, MeshFlags.Dynamic, VertexFormat.Position);
				
				// lock buffers
				DataStream sVx = g.VertexBuffer.Lock(0, 0, LockFlags.None);
				DataStream sIx = g.IndexBuffer.Lock(0, 0, LockFlags.None);
				
				// write buffers
				sVx.WriteRange<Vector3>(v3Vx);
				sIx.WriteRange<short>(Ix);
				
				// unlock buffers
				g.VertexBuffer.Unlock();
				g.IndexBuffer.Unlock();
				
				//dispose buffers
				sVx.Dispose();
				sIx.Dispose();
				
				// remove old mesh, add new and dispose old mesh
				// throw in teapot mesh sometimes
				
				FDeviceMeshes.Remove(m);
				if (py < 0.1) FDeviceMeshes.Add(Mesh.CreateTeapot(dev));
				else FDeviceMeshes.Add(g);
								
				m.Dispose();
				m = null;

				
			}
			
			else //if resource is not yet created on given Device, create it now
			{
				FHost.Log(TLogType.Debug, "Creating Resource...");
				Device dev = Device.FromPointer(new IntPtr(OnDevice));
				
				Mesh FMeshOut = new Mesh(dev, 1, 3, MeshFlags.Dynamic, VertexFormat.Position);
				
				DataStream sVx = FMeshOut.VertexBuffer.Lock(0, 0, LockFlags.None);
				DataStream sIx = FMeshOut.IndexBuffer.Lock(0, 0, LockFlags.None);
								
			
				v3Vx[0] = new Vector3(0, py, 0);
				v3Vx[1] = new Vector3(1+py, 0, 0);
				v3Vx[2] = new Vector3(0, 1, 0);
											
				
				Ix[0] = 0;
				Ix[1] = 1;
				Ix[2] = 2;
				
				sVx.WriteRange<Vector3>(v3Vx);
				sIx.WriteRange<short>(Ix);
							
				FMeshOut.VertexBuffer.Unlock();
				FMeshOut.IndexBuffer.Unlock();
				sVx.Dispose();
				sIx.Dispose();
								
				FDeviceMeshes.Add(FMeshOut);

			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			//dispose resources that were created on given Device
			Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) {return ms.Device.ComPointer == (IntPtr)OnDevice;});
			
			if (m != null)
			{
				FHost.Log(TLogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(m);
				m.Dispose();
				m = null;
			}
		}
		
		public void GetMesh(IDXMeshOut ForPin, int OnDevice, out int Mesh)
		{
			Mesh = 0;
			if (ForPin == FMyMeshOutput)
			{
				Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) {return ms.Device.ComPointer == (IntPtr)OnDevice;});
				if (m != null)
					Mesh = m.ComPointer.ToInt32();
			}
		}
		#endregion
		

		public float py = 0.1f;
		public float sy = 0.01f;
		public Vector3[] v3Vx = new Vector3[3];
		public short[] Ix = new short[3];
		
	}
}
