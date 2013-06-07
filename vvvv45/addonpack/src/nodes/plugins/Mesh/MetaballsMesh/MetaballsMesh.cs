#region licence/info

//////project name
//Metaballs Mesh

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
//majortom

#endregion licence/info

//use what you need
using System;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
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
		
		//input pin declaration
		private IValueIn pos;
		private IValueIn mass;
		private IValueIn gsize;
		private IValueIn level;
		private IValueIn smooth;
		
		//output pin declaration
		
		
		private IDXMeshOut FMyMeshOutput;
		
		DataStream sVx;
		DataStream sIx;
		
		private Dictionary<Device, Mesh> FDeviceMeshes = new Dictionary<Device, Mesh>();
		
		#endregion field declaration
		
		#region constructor/destructor
		
		public PluginMeshTemplate()
		{
			//the nodes constructor
			OpenVoxels = new int[MaxOpenVoxels * 3];
			PreComputed = new int[MaxOpenVoxels * 12];
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
		~PluginMeshTemplate()
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
					FPluginInfo.Name = "Metaballs";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "majortom";
					//describe the nodes function
					FPluginInfo.Help = "MetaballsMesh";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "Base code ported from example by Andreas Jönsson @ www.AngelCode.com";
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
			FHost.CreateValueInput("Position ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out pos);
			pos.SetSubType3D(-1, 1, 0.01, 0, 0, 0, false, false, false);
			
			FHost.CreateValueInput("Mass", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out mass);
			mass.SetSubType(0, 2, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Grid Size", 1, null, TSliceMode.Single, TPinVisibility.True, out gsize);
			gsize.SetSubType(2, 128, 1, 2, false, false, true);
			// MaxValue is set to 128...
			
			FHost.CreateValueInput("Level", 1, null, TSliceMode.Single, TPinVisibility.True, out level);
			level.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Smooth Mesh", 1, null, TSliceMode.Single, TPinVisibility.True, out smooth);
			smooth.SetSubType(0, 1, 1, 0, false, true, true);
			
			
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
			FMyMeshOutput.SliceCount = 1;
			
			if (pos.SliceCount != NumBalls) //initialize metaball arrays
			{
				NumBalls = pos.SliceCount;
				m_Ball = new Vector3[NumBalls];
				m_BallMass = new float[NumBalls];
				double pm;
				for (int i=0; i<NumBalls; i++)
				{
					mass.GetValue(i, out pm);
					m_BallMass[i] = (float)pm;
				}
				update = true;
			}
			
			if (gsize.PinIsChanged) //initialize grid
			{
				double dSize;
				gsize.GetValue(1, out dSize);
				GridSize = (int) dSize;
				VoxelSize = 2 / (float) dSize;
				
				GridEnergy = new float[(GridSize+1)*(GridSize+1)*(GridSize+1)];
				GridPointStatus = new bool[(GridSize+1)*(GridSize+1)*(GridSize+1)];
				GridVoxelStatus = new byte[GridSize*GridSize*GridSize];
				GridVoxelSeek = new int[GridSize*GridSize*GridSize];
				
				update = true;
			}
			
			if (level.PinIsChanged)
			{
				double dLevel;
				level.GetValue(1, out dLevel);
				Level = (float) dLevel * 100;
				update = true;
			}
			
			if (smooth.PinIsChanged)
			{
				smooth.GetValue(1, out Smooth);
				update = true;
			}
			
			if (mass.PinIsChanged) // initialize metaballs' mass array
			{
				double pm;
				for (int i=0; i<NumBalls; i++)
				{
					mass.GetValue(i, out pm);
					m_BallMass[i] = (float)pm;
				}
				update = true;
			}
			
			if (pos.PinIsChanged)
			{
				// store all positions as SlimDX Vector3
				for (int i=0; i<NumBalls; i++)
				{
					double px, py, pz;
					pos.GetValue3D(i, out px, out py, out pz);
					m_Ball[i] = new Vector3((float)px, (float)py, (float)pz);
				}
				update = true;
			}
			
			
			if (update) //initialize all tables
			{
				Array.Clear(GridEnergy, 0, GridEnergy.Length);
				Array.Clear(GridPointStatus, 0, GridPointStatus.Length);
				Array.Clear(GridVoxelStatus, 0, GridVoxelStatus.Length);
				Array.Clear(GridVoxelSeek, 0, GridVoxelSeek.Length);
				
				Render();
				
				#region MeshOutput
				
				// casting all int type Indices to short type IxBuffer here
				// seems faster than using short type Indices in program loop (?)
				
				IxBuffer = new short[NumIndices];
				
				unsafe
				{
					fixed (short* IxBufferPtr = &IxBuffer[0])
					{
						fixed (int* IndicesPtr = &Indices[0])
						{
							for (int j = 0; j < NumIndices; j++)
								*(IxBufferPtr + j) = (short) *(IndicesPtr + j);
						}
					}
				}
				
				if (Smooth == 1) SmoothMesh();
				
				// compute normals for all vertices
				ComputeNormals();
				
				#endregion MeshOutput
				
			} // endif update
		}
		
		#endregion mainloop
		
		#region DXMesh
		private void RemoveResource(Device OnDevice)
		{
			Mesh m = FDeviceMeshes[OnDevice];
			//FHost.Log(TLogType.Debug, "Destroying Resource...");
			FDeviceMeshes.Remove(OnDevice);
			m.Dispose();
		}
		
		public void UpdateResource(IPluginOut ForPin, Device OnDevice)
		{
			
			try
			{
				Mesh mtry = FDeviceMeshes[OnDevice];
				if (update)
					RemoveResource(OnDevice);
			}
			
			//if resource is not yet created on given Device, create it now
			catch
			{
				update = true;
			}
			
			if (update)
			{
				try
				{
					// create new Mesh
					Mesh NewMesh = new Mesh(OnDevice, NumIndices/3, NumVertices,
					                        MeshFlags.Dynamic | MeshFlags.WriteOnly,
					                        VertexFormat.PositionNormal);

					// lock buffers
					sVx = NewMesh.LockVertexBuffer(LockFlags.Discard);
					sIx = NewMesh.LockIndexBuffer(LockFlags.Discard);
					
					// write buffers
					unsafe
					{
						fixed (sVxBuffer* FixTemp = &VxBuffer[0])
						{
							IntPtr VxPointer = new IntPtr(FixTemp);
							sVx.WriteRange(VxPointer, sizeof(sVxBuffer) * NumVertices);
							
						}
						fixed (short* FixTemp = &IxBuffer[0])
						{
							IntPtr IxPointer = new IntPtr(FixTemp);
							sIx.WriteRange(IxPointer, sizeof(short) * NumIndices);
						}
					}

					// unlock buffers
					NewMesh.UnlockIndexBuffer();
					NewMesh.UnlockVertexBuffer();
					
					FDeviceMeshes.Add(OnDevice, NewMesh);
				}
				finally
				{
					update = false;
				}
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
		{
			
			//called when a resource needs to be disposed on a given device
			//this is also called when the plugin is destroyed,
			//so don't dispose dxresources in the plugins destructor/Dispose()
			try
			{
				RemoveResource(OnDevice);
			}
			catch
			{
				//nothing to do
			}
		}
		
		public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
		{
			//this is called from the plugin host from within directX beginscene/endscene
			//therefore the plugin shouldn't be doing much here other than handing back the right mesh
			
			//in case the plugin has several mesh outputpins a test for the pin can be made here to get the right mesh.
			if (ForPin == FMyMeshOutput && FDeviceMeshes.ContainsKey(OnDevice))
				return FDeviceMeshes[OnDevice];
			else
				return null;

		}

		#endregion
		
		#region metaballs
		
		protected void Render()
		{
			int nCase = 255;
			int x, y, z;
			bool bComputed;
			
			NumOpenVoxels = 0;
			NumIndices = 0;
			NumVertices = 0;
			
			for( int i = 0; i < NumBalls; i++ )
			{
				x = World2Grid(m_Ball[i].X);
				y = World2Grid(m_Ball[i].Y);
				z = World2Grid(m_Ball[i].Z);
				
				// Work our way out from the center of the ball until the surface is
				// reached. If the voxel at the surface is already computed then this
				// ball share surface with a previous ball.
				
				bComputed = false;
				while(true)
				{
					
					if( IsGridVoxelComputed(x,y,z) )
					{
						bComputed = true;
						break;
					}

					nCase = ComputeGridVoxel(x,y,z);
					if( nCase < 255 ) break;

					z--;
				}

				if( bComputed ) continue;

				// Compute all voxels on the surface by computing neighbouring voxels
				// if the surface goes into them.
				
				AddNeighborsToList(nCase, x, y, z);

				while( NumOpenVoxels > 0 )
				{
					NumOpenVoxels--;
					x = OpenVoxels[NumOpenVoxels * 3];
					y = OpenVoxels[NumOpenVoxels * 3 + 1];
					z = OpenVoxels[NumOpenVoxels * 3 + 2];

					nCase = ComputeGridVoxel(x,y,z);

					AddNeighborsToList(nCase,x,y,z);
					
				}

			}
		}
		
		protected float ComputeEnergy(float x, float y, float z)
		{
			float fEnergy = 0;
			float fSqDist;
			
			for( int i = 0; i < NumBalls; i++ )
			{
				// The formula for the energy is
				//   e += mass/distance^2
				
				// TODO add other finite fall-off formulas and other primitives

				fSqDist = (m_Ball[i].X - x)*(m_Ball[i].X - x) +
					(m_Ball[i].Y - y)*(m_Ball[i].Y - y) +
					(m_Ball[i].Z - z)*(m_Ball[i].Z - z);
				
				
				if( fSqDist < 0.0001f ) fSqDist = 0.0001f;

				fEnergy += m_BallMass[i] / fSqDist;
			}
			return fEnergy;
		}
		
		
		#region ComputeNormals
		
		protected void  ComputeNormals()
		{
			for (int j = 0; j < NumVertices; j++)
			{
				float f4Dist;
				Vector3 Normal = new Vector3(0);
				
				for( int i = 0; i < NumBalls; i++ )
				{
					// To compute the normal we derive the energy formula and get
					//
					//   n += 2 * mass * vector / distance^4
					
					float x = VxBuffer[j].Vel.X - m_Ball[i].X;
					float y = VxBuffer[j].Vel.Y - m_Ball[i].Y;
					float z = VxBuffer[j].Vel.Z - m_Ball[i].Z;
					
					f4Dist = (x*x + y*y + z*z);
					f4Dist *= f4Dist;

					Normal.X += 2 * m_BallMass[i] * x / f4Dist;
					Normal.Y += 2 * m_BallMass[i] * y / f4Dist;
					Normal.Z += 2 * m_BallMass[i] * z / f4Dist;

				}

				//normalize vector
				
				Normal.Normalize(); // SlimDx!
				VxBuffer[j].Nel = Normal;

				// To compute the sphere-map texture coordinates
				// normals should be transformed to camera space...
				
				//VxBuffer[Vertex].Tel.X = Normal.X/2 + 0.5f;
				//VxBuffer[Vertex].Tel.Y = -Normal.Y/2 + 0.5f;
			}
		}
		
		
		#endregion ComputeNormals
		
		
		protected float ComputeGridPointEnergy(int x, int y, int z)
		{

			int address3D = x + y*(GridSize+1) + z*(GridSize+1)*(GridSize+1);
			
			if( IsGridPointComputed(x,y,z) )
				return GridEnergy[address3D];

			// The energy on the edges is always zero to make sure the isosurface is
			// always closed. Going far out of range produces some funny results.
			
			if( x == 0 || y == 0 || z == 0 ||
			   x == GridSize || y == GridSize || z == GridSize )
			{
				GridEnergy[address3D] = 0;
				SetGridPointComputed(x,y,z);
				return 0;
			}

			float fx = Grid2World(x);
			float fy = Grid2World(y);
			float fz = Grid2World(z);

			float Energy = ComputeEnergy(fx,fy,fz);
			GridEnergy[address3D] = Energy;

			SetGridPointComputed(x,y,z);

			return Energy;
		}
		
		private float[] b = new float[8];
		protected int ComputeGridVoxel(int x, int y, int z)
		{
			b[0] = ComputeGridPointEnergy(x  , y  , z  );
			b[1] = ComputeGridPointEnergy(x+1, y  , z  );
			b[2] = ComputeGridPointEnergy(x+1, y  , z+1);
			b[3] = ComputeGridPointEnergy(x  , y  , z+1);
			b[4] = ComputeGridPointEnergy(x  , y+1, z  );
			b[5] = ComputeGridPointEnergy(x+1, y+1, z  );
			b[6] = ComputeGridPointEnergy(x+1, y+1, z+1);
			b[7] = ComputeGridPointEnergy(x  , y+1, z+1);

			float fx, fy, fz;

			int c = 0;
			c |= b[0] > Level ? (1<<0) : 0;
			c |= b[1] > Level ? (1<<1) : 0;
			c |= b[2] > Level ? (1<<2) : 0;
			c |= b[3] > Level ? (1<<3) : 0;
			c |= b[4] > Level ? (1<<4) : 0;
			c |= b[5] > Level ? (1<<5) : 0;
			c |= b[6] > Level ? (1<<6) : 0;
			c |= b[7] > Level ? (1<<7) : 0;




			int i = 0;
			
			// pull PreComputed vertices so triangles will connect to them
			// and clear the PreComputed slot
			
			for (int ei = 0; ei < 12; ei++)
			{
				EdgeIndices[ei] = PreComputed[NumOpenVoxels * 12 + ei] - 1;
				PreComputed[NumOpenVoxels * 12 + ei] = 0;
			}
			
			// find other PreComputed vertices
			
			SetGridVoxelComputed(x + y * GridSize + z * GridSize * GridSize);
			GetPreComputed(c, x + y * GridSize + z * GridSize * GridSize);
			
			while(true)
			{
				int nEdge =	MarchingCubes.CubeTriangles[c, i];
				if( nEdge == -1 )
					break;

				if( EdgeIndices[nEdge] == -1 )
				{
					EdgeIndices[nEdge] = NumVertices;

					// Compute the vertex by interpolating between two points
					
					int nIndex0 = MarchingCubes.CubeEdges[nEdge, 0];
					int nIndex1 = MarchingCubes.CubeEdges[nEdge, 1];

					float t = (Level - b[nIndex0])/(b[nIndex1] - b[nIndex0]);

					VxBuffer[NumVertices].Vel.X =
						MarchingCubes.CubeVertices[nIndex0, 0]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 0]*t;
					VxBuffer[NumVertices].Vel.Y =
						MarchingCubes.CubeVertices[nIndex0, 1]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 1]*t;
					VxBuffer[NumVertices].Vel.Z =
						MarchingCubes.CubeVertices[nIndex0, 2]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 2]*t;
					
					fx = Grid2World(x);
					fy = Grid2World(y);
					fz = Grid2World(z);
					
					VxBuffer[NumVertices].Vel.X = fx +
						VxBuffer[NumVertices].Vel.X * VoxelSize;
					VxBuffer[NumVertices].Vel.Y = fy +
						VxBuffer[NumVertices].Vel.Y * VoxelSize;
					VxBuffer[NumVertices].Vel.Z = fz +
						VxBuffer[NumVertices].Vel.Z * VoxelSize;


					NumVertices++;
					if (NumVertices == MaxVertices)
					{
						MaxVertices *= 2;
						
						Array.Resize(ref VxBuffer, MaxVertices);
					}
				}

				// Add the edge's vertex index to the index list
				
				Indices[NumIndices] = EdgeIndices[nEdge];

				NumIndices++;
				if (NumIndices == MaxIndices)
				{
					MaxIndices *= 2;
					
					Array.Resize(ref Indices, MaxIndices);
				}
				
				i++;
			}

			return c;
		}

		protected bool IsGridPointComputed(int x, int y, int z)
		{
			if( GridPointStatus[x +
			                    y*(GridSize+1) +
			                    z*(GridSize+1)*(GridSize+1)] == true )
				return true;
			else
				return false;
		}
		
		protected bool IsGridVoxelComputed(int x, int y, int z)
		{
			if( GridVoxelStatus[x +
			                    y*GridSize +
			                    z*GridSize*GridSize] == 1 )
				return true;
			else
				return false;
		}
		
		protected bool IsGridVoxelInList(int x, int y, int z)
		{
			if( GridVoxelStatus[x +
			                    y*GridSize +
			                    z*GridSize*GridSize] == 2 )
				return true;
			else
				return false;
		}
		
		protected void SetGridPointComputed(int x, int y, int z)
		{
			GridPointStatus[x +
			                y*(GridSize+1) +
			                z*(GridSize+1)*(GridSize+1)] = true;
		}
		
		protected void SetGridVoxelComputed(int address)
		{
			GridVoxelStatus[address] = 1;
			GridVoxelSeek[address] = 0;
		}
		
		protected void SetGridVoxelInList(int address)
		{
			
			GridVoxelStatus[address] = 2;
			GridVoxelSeek[address] = NumOpenVoxels + 1;
		}

		protected float Grid2World(int x)
		{
			return (float)x*VoxelSize - 1.0f;
		}
		
		protected int World2Grid(float x)
		{
			return (int)((x + 1.0f)/VoxelSize + 0.5f);
		}
		
		protected void AddNeighborsToList(int nCase, int x, int y, int z)
		{
			if( (MarchingCubes.CubeNeighbors[nCase] & 1) != 0 )
				AddNeighbor(x+1, y, z, 0);

			if( (MarchingCubes.CubeNeighbors[nCase] & 2) != 0 )
				AddNeighbor(x-1, y, z, 1);

			if( (MarchingCubes.CubeNeighbors[nCase] & 4) != 0 )
				AddNeighbor(x, y+1, z, 2);

			if( (MarchingCubes.CubeNeighbors[nCase] & 8) != 0 )
				AddNeighbor(x, y-1, z, 3);

			if( (MarchingCubes.CubeNeighbors[nCase] & 16) != 0 )
				AddNeighbor(x, y, z+1, 4);

			if( (MarchingCubes.CubeNeighbors[nCase] & 32) != 0 )
				AddNeighbor(x, y, z-1, 5);
		}
		
		protected void  AddNeighbor(int x, int y, int z, int side)
		{
			int address = x + y*GridSize + z*GridSize*GridSize;
			
			// if neighbor cube is computed there is nothing to do
			if( IsGridVoxelComputed(x,y,z))
				return;
			
			// if neighbor cube is already in list find its address
			// and copy computed vertices
			if ( IsGridVoxelInList(x,y,z) )
			{
				PreComputeEdge(GridVoxelSeek[address] - 1, side);
				return;
			}

			// Make sure the arrays are large enough
			
			if( MaxOpenVoxels == NumOpenVoxels )
			{
				MaxOpenVoxels *= 2;
				
				Array.Resize(ref OpenVoxels, MaxOpenVoxels * 3);
				Array.Resize(ref PreComputed, MaxOpenVoxels * 12);
			}

			OpenVoxels[NumOpenVoxels * 3] = x;
			OpenVoxels[NumOpenVoxels * 3 + 1] = y;
			OpenVoxels[NumOpenVoxels * 3 + 2] = z;
			
			SetGridVoxelInList(address);
			PreComputeEdge(NumOpenVoxels, side);
			
			NumOpenVoxels++;
		}
		
		protected void PreComputeEdge(int OpenVoxelAdr, int side)
		{
			// Push computed vertices to PreComputed list
			// on the proper OpenVoxel address.
			// Neighbor cube is on one of 6 sides:
			
			OpenVoxelAdr *= 12; // there are 12 edges
			
			if (side == 0) // x+1
			{
				PushVertex(OpenVoxelAdr + 3, 1);
				PushVertex(OpenVoxelAdr + 7, 5);
				PushVertex(OpenVoxelAdr + 8, 9);
				PushVertex(OpenVoxelAdr + 10, 11);
			}
			
			if (side == 1) // x-1
			{
				PushVertex(OpenVoxelAdr + 1, 3);
				PushVertex(OpenVoxelAdr + 5, 7);
				PushVertex(OpenVoxelAdr + 9, 8);
				PushVertex(OpenVoxelAdr + 11, 10);
			}
			
			if (side == 2) // y+1
			{
				PushVertex(OpenVoxelAdr + 0, 4);
				PushVertex(OpenVoxelAdr + 1, 5);
				PushVertex(OpenVoxelAdr + 2, 6);
				PushVertex(OpenVoxelAdr + 3, 7);
			}
			
			if (side == 3) // y-1
			{
				PushVertex(OpenVoxelAdr + 4, 0);
				PushVertex(OpenVoxelAdr + 5, 1);
				PushVertex(OpenVoxelAdr + 6, 2);
				PushVertex(OpenVoxelAdr + 7, 3);
			}
			
			if (side == 4) // z+1
			{
				PushVertex(OpenVoxelAdr + 0, 2);
				PushVertex(OpenVoxelAdr + 4, 6);
				PushVertex(OpenVoxelAdr + 8, 10);
				PushVertex(OpenVoxelAdr + 9, 11);
			}
			
			if (side == 5) // z-1
			{
				PushVertex(OpenVoxelAdr + 2, 0);
				PushVertex(OpenVoxelAdr + 6, 4);
				PushVertex(OpenVoxelAdr + 10, 8);
				PushVertex(OpenVoxelAdr + 11, 9);
			}
		}
		
		protected void PushVertex(int PCi, int EIi)
		{
			if (PreComputed[PCi] == 0 && EdgeIndices[EIi] != -1)
			{
				PreComputed[PCi] = EdgeIndices[EIi] + 1;
			}
		}
		protected void PullVertex(int EIi, int PCi)
		{
			if (EdgeIndices[EIi] == -1 && PreComputed[PCi] != 0)
			{
				EdgeIndices[EIi] = PreComputed[PCi] - 1;
			}
		}
		
		protected void  GetPreComputed(int nCase, int address)
		{
			if( (MarchingCubes.CubeNeighbors[nCase] & 1) != 0
			   && GridVoxelSeek[address + 1] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address + 1] - 1);
				PullVertex(1, loc + 3);
				PullVertex(5, loc + 7);
				PullVertex(9, loc + 8);
				PullVertex(11, loc + 10);
			}

			if( (MarchingCubes.CubeNeighbors[nCase] & 2) != 0
			   && GridVoxelSeek[address - 1] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address - 1] - 1);
				PullVertex(3, loc + 1);
				PullVertex(7, loc + 5);
				PullVertex(8, loc + 9);
				PullVertex(10, loc + 11);
			}

			if( (MarchingCubes.CubeNeighbors[nCase] & 4) != 0
			   && GridVoxelSeek[address + GridSize] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address + GridSize] - 1);
				PullVertex(4, loc + 0);
				PullVertex(5, loc + 1);
				PullVertex(6, loc + 2);
				PullVertex(7, loc + 3);
			}
			
			if( (MarchingCubes.CubeNeighbors[nCase] & 8) != 0
			   && GridVoxelSeek[address - GridSize] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address - GridSize] - 1);
				PullVertex(0, loc + 4);
				PullVertex(1, loc + 5);
				PullVertex(2, loc + 6);
				PullVertex(3, loc + 7);
			}

			if( (MarchingCubes.CubeNeighbors[nCase] & 16) != 0
			   && GridVoxelSeek[address + GridSize * GridSize] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address + GridSize * GridSize] - 1);
				PullVertex(2, loc + 0);
				PullVertex(6, loc + 4);
				PullVertex(10, loc + 8);
				PullVertex(11, loc + 9);
			}

			if( (MarchingCubes.CubeNeighbors[nCase] & 32) != 0
			   && GridVoxelSeek[address - GridSize * GridSize] != 0)
			{
				int loc = 12 * (GridVoxelSeek[address - GridSize * GridSize] - 1);
				PullVertex(0, loc + 2);
				PullVertex(4, loc + 6);
				PullVertex(8, loc + 10);
				PullVertex(9, loc + 11);
			}
		}
		
		#endregion

		protected void SmoothMesh()
		{
			// create sum vector for all triangles
			
			Vector3[] SumVec = new Vector3[NumIndices / 3];
			for (int i = 0; i < NumIndices / 3; i++)
			{
				SumVec[i] = VxBuffer[Indices[i * 3]].Vel +
					VxBuffer[Indices[i * 3 + 1]].Vel +
					VxBuffer[Indices[i * 3 + 2]].Vel;
			}
			
			
			unsafe
			{
				int* IndexPtr = stackalloc int[NumIndices];
				for (int i = 0; i < NumIndices / 3; i++)
				{
					*(IndexPtr + i * 3) = i;
					*(IndexPtr + i * 3 + 1) = i;
					*(IndexPtr + i * 3 + 2) = i;
				}
				fixed (int* IndicesPtr = &Indices[0])
					QuickSort(IndicesPtr, IndexPtr, 0, NumIndices-1);
				
				Vector3 TempVertex = new Vector3(0);
				int count = 0;
				for (int i = 0; i < NumIndices; i++)
				{
					count++;
					TempVertex += SumVec[*(IndexPtr + i)];
					if (i == NumIndices - 1 || Indices[i] != Indices[i + 1])
					{
						VxBuffer[Indices[i]].Vel = TempVertex / (count * 3);
						count = 0;
						TempVertex *= 0;
					}
				}
				
			}

		}
		
		unsafe protected void QuickSort(int* value, int* index, int L, int R)
		{
			int I, J, P, T;

			do	{
				I = L;
				J = R;
				P = *(value + ((L + R)>>1));
				do {
					while (*(value + I) < P) I++;
					while (*(value + J) > P) J--;
					if (I <= J)
					{
						T = *(value + I);
						*(value + I) = *(value + J);
						*(value + J) = T;
						T = *(index + I);
						*(index + I) = *(index + J);
						*(index + J) = T;
						I++;
						J--;
					}
				} while (I <= J);
				if (L < J) QuickSort(value, index, L, J);
				L = I;
			} while (I < R);
		}
		
		
		protected double	Smooth = 0;
		protected bool		update = false;
		protected float		Level = 100.0f;

		protected int		NumBalls = 1;
		
		protected int		NumOpenVoxels = 0;
		protected int		MaxOpenVoxels = 1024;
		protected int[]		OpenVoxels;
		protected int[]		PreComputed;

		protected int		GridSize = 32;
		protected float		VoxelSize;

		

		protected float[]	GridEnergy;
		protected bool[]	GridPointStatus;
		protected byte[]	GridVoxelStatus;
		protected int[]		GridVoxelSeek;
		protected int[] 	EdgeIndices = new int[12];

		protected int			NumVertices = 0;
		protected int			NumIndices  = 0;
		protected float[]		m_BallMass;
		protected Vector3[] 	m_Ball;

		protected int[]			Indices  = new int[8192];
		protected int			MaxVertices = 4096;
		protected int			MaxIndices  = 8192;
		

		

		
		
		
		public struct sVxBuffer
		{
			/// <summary>
			/// Vertex component in VertexBuffer
			/// </summary>
			public Vector3 Vel;
			/// <summary>
			/// Normal component in VertexBuffer
			/// </summary>
			public Vector3 Nel;
			
			public sVxBuffer(Vector3 v, Vector3 n)
			{
				this.Vel = v;
				this.Nel = n;
			}
			
			
		}
		
		
		protected sVxBuffer[]	VxBuffer = new sVxBuffer[4096];
		protected short[]		IxBuffer;

		
	}
}
