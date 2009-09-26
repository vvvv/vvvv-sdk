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
//using VVVV.Shared.VSlimDX;

using SlimDX;
using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class Penthacoron: IPlugin, IDisposable, IPluginDXMesh
	{
		#region field declaration

        //input pin declaration
        private IValueIn Radius;
        private double r = 1.0;

        private IValueIn Perspective;
        private double p;

        private ITransformIn LeftTransform;
        private Matrix4x4 currentLeftTransformSlice;

        private Vector4D[] vertex = new Vector4D[5];
        private Vector3[] v = new Vector3[5];

		//the host (mandatory)
		private IPluginHost FHost;
		//Track whether Dispose has been called.
		private bool FDisposed = false;
		
		//a mesh output pin
		private IDXMeshOut FMyMeshOutput;
		
		//a list that holds a mesh for every device
		private Dictionary<int, Mesh> FDeviceMeshes = new Dictionary<int, Mesh>();

        private SlimDX.DataStream sVx;
        private SlimDX.DataStream sIx;
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public Penthacoron()
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
		~Penthacoron()
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
					FPluginInfo.Name = "Penthacoron";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "fibo";
					//describe the nodes function
                    FPluginInfo.Help = "The 5 vertices regular polytope in 4D";
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
            FHost.CreateValueInput("Radius",1,null,TSliceMode.Single,TPinVisibility.True,out Radius);
            Radius.SetSubType(0.001,1000.0,0.001,1.0,false,false,false);

            FHost.CreateValueInput("Perspective", 1, null, TSliceMode.Single, TPinVisibility.True, out Perspective);
            Perspective.SetSubType(0.1, 1, 0.001, 0.5, false, false, false);

            FHost.CreateTransformInput("Left Transform Input", TSliceMode.Dynamic, TPinVisibility.True, out LeftTransform);
	    	
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

            if(Radius.PinIsChanged || Perspective.PinIsChanged || LeftTransform.PinIsChanged){
                
                Radius.GetValue(0, out r);

                Perspective.GetValue(0, out p);

                for (int i = 0; i < SpreadMax; i++)
                {
                    LeftTransform.GetMatrix(i, out currentLeftTransformSlice);
                }

                FDeviceMeshes.Clear();
            }
		}
		
		#endregion mainloop
		
		#region DXMesh
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			//Called by the PluginHost every frame for every device. Therefore a plugin should only do 
			//device specific operations here and still keep node specific calculations in the Evaluate call.

            try
            {
                Mesh m = FDeviceMeshes[OnDevice];  
            }
            catch
            {
                //if resource is not yet created on given Device, create it now
                //FHost.Log(TLogType.Debug, "Creating Resource...");
                Device dev = Device.FromPointer(new IntPtr(OnDevice));
                FDeviceMeshes.Add(OnDevice, createMesh(dev));

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
				Mesh m = FDeviceMeshes[OnDevice];
				FHost.Log(TLogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(OnDevice);
				
				//dispose mesh
				m.Dispose();
			}
			catch
			{
				//resource is not available for this device. good. nothing to do then.
			}
		}
		
		public void GetMesh(IDXMeshOut ForPin, int OnDevice, out int MeshPointer)
		{
			// Called by the PluginHost everytime a mesh is accessed via a pin on the plugin.
			// This is called from the PluginHost from within DirectX BeginScene/EndScene,
			// therefore the plugin shouldn't be doing much here other than handing back the right mesh
			
			MeshPointer = 0;
			//in case the plugin has several mesh outputpins a test for the pin can be made here to get the right mesh.
			if (ForPin == FMyMeshOutput)
			{
				Mesh m = FDeviceMeshes[OnDevice];
				if (m != null)
					MeshPointer = m.ComPointer.ToInt32();
			}
		}

        public Mesh createMesh(Device dev)
        {

            int NumIndices = 10;
            int NumVertices = 5;

            Matrix4x4 Projection = new Matrix4x4(
                1,0,0,0,
                0,1,0,0,
                0,0,1,0,
                p,p,p,0
                );
         
            vertex[0] = new Vector4D(0, 0, 0, 1);
            vertex[1] = new Vector4D(-0.559, 0.559, 0.559, -0.25);
            vertex[2] = new Vector4D(0.559, -0.559, 0.559, -0.25);
            vertex[3] = new Vector4D(0.559, 0.559, -0.559, -0.25);
            vertex[4] = new Vector4D(-0.559, -0.559, -0.559, -0.25);
            
            for (int i = 0; i < 5; i++) 
            {
                vertex[i] *= r; // scale it
                vertex[i] = this.currentLeftTransformSlice * vertex[i]; // apply left tranform
                // what about right transform ? I need a new * operator.
                vertex[i] = Projection * vertex[i]; // projection in 3D euclidean space.
            }

            // create new Mesh
            Mesh NewMesh = new Mesh(dev, NumIndices, NumVertices,
                                    MeshFlags.Dynamic | MeshFlags.WriteOnly,
                                    VertexFormat.Position);
            
            // lock buffers
            sVx = NewMesh.LockVertexBuffer(LockFlags.Discard);
            sIx = NewMesh.LockIndexBuffer(LockFlags.Discard);

            // write buffers

            for (int i = 0; i < 5; i++)
            {
                Vector3 v = new Vector3((float)vertex[i].x, (float)vertex[i].y, (float)vertex[i].z);

                // why this doesn't work?
                //Vector3 v = VVVV.Shared.VSlimDX.VSlimDXUtils.Vector3DToSlimDXVector3(vertex[i]);
                sVx.Write(v);
            }
            
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(2);
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(3);
            sIx.Write<short>(0); sIx.Write<short>(1); sIx.Write<short>(4);
            sIx.Write<short>(0); sIx.Write<short>(2); sIx.Write<short>(3);
            sIx.Write<short>(0); sIx.Write<short>(2); sIx.Write<short>(4);
            sIx.Write<short>(0); sIx.Write<short>(3); sIx.Write<short>(4);
            sIx.Write<short>(1); sIx.Write<short>(2); sIx.Write<short>(3);
            sIx.Write<short>(1); sIx.Write<short>(2); sIx.Write<short>(4);
            sIx.Write<short>(1); sIx.Write<short>(3); sIx.Write<short>(4);
            sIx.Write<short>(2); sIx.Write<short>(3); sIx.Write<short>(4);

            // unlock buffers
            NewMesh.UnlockIndexBuffer();
            NewMesh.UnlockVertexBuffer();

            return NewMesh; 
        }

		#endregion
	}
}
