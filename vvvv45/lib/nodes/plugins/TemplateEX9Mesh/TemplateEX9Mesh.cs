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

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Template",
	            Category = "EX9.Geometry",
	            Version = "",
	            Help = "Basic template which creates a mesh",
	            Tags = "")]
	#endregion PluginInfo
	public class Template : DXMeshOutPluginBase, IPluginEvaluate
	{
		#region fields & pins
		
        [Input("Randomize")]
		IDiffSpread<Vector3> FRandomizeIn;
		
		[Input("Reset", IsBang = true)]
		ISpread<bool> FResetIn;

		[Input("Resolution", DefaultValue = 20, IsSingle = true)]
		IDiffSpread<int> FResoIn;

		[Import()]
		ILogger FLogger;

		#endregion fields & pins

		// import host and hand it to base constructor
		[ImportingConstructor()]
		public Template(IPluginHost host) 
		    : base(host)
		{
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			//recreate mesh
			if (FResoIn.IsChanged || FResetIn[0]) 
			{
				Reinitialize();
			}
			
			//update mesh
			if (FRandomizeIn.IsChanged)
			{
			    Update();
			}
		}

		//this method gets called, when Reinitialize() was called in evaluate,
		//or a graphics device asks for its data
		protected override Mesh CreateMesh(Device device)
		{
			FLogger.Log(LogType.Debug, "Creating Mesh...");
			return Mesh.CreateSphere(device, 1, FResoIn[0], FResoIn[0]);
		}

		//this method gets called, when Update() was called in evaluate,
		//or a graphics device asks for its mesh, here you can alter the data of the mesh
		protected override void UpdateMesh(Mesh mesh)
		{
			//do something with the mesh data
			var vertices = mesh.LockVertexBuffer(LockFlags.None);

            for (int i=0; i<mesh.VertexCount; i++)
            {
                //get the vertex content
			    var pos = vertices.Read<Vector3>();
			    var norm = vertices.Read<Vector3>();

			    pos.X = (pos.X + FRandomizeIn[i].X)%2 - 0;
			    pos.Y = (pos.Y + FRandomizeIn[i].Y)%2 - 1;
			    pos.Z = (pos.Z + FRandomizeIn[i].Z)%2 - 0;
			    
			    //to write the data move the stream position back!
			    vertices.Position -= mesh.BytesPerVertex;
			    vertices.Write(pos);
			    vertices.Write(norm);
			}
			
			mesh.UnlockVertexBuffer();
		}
	}
}
