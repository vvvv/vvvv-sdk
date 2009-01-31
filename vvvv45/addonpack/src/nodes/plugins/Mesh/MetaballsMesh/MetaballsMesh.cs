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
		
		//input pin declaration
		private IValueIn pos;
		private IValueIn mass;
		private IValueIn gsize;
		private IValueIn level;
		
		//output pin declaration
		private IValueOut position;
		private IValueOut normal;
		private IValueOut indices;
		
		private IDXMeshIO FMyMeshOutput;
		
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
					FPluginInfo.Name = "MetaBallsMesh";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Template";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Mesh";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "MetaBallsMesh debug";
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
			
			//create inputs
			FHost.CreateValueInput("Position ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out pos);
			pos.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
			
			FHost.CreateValueInput("Mass", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out mass);
			mass.SetSubType(0, double.MaxValue, 0.01, 1, false, false, false);
			
			FHost.CreateValueInput("Grid Size", 1, null, TSliceMode.Single, TPinVisibility.True, out gsize);
			gsize.SetSubType(2, 64, 1, 2, false, false, true);
			// MaxValue is set to 64... dare to go higher
			
			FHost.CreateValueInput("Level", 1, null, TSliceMode.Single, TPinVisibility.True, out level);
			level.SetSubType(double.MinValue, double.MaxValue, 0.01, 100, false, false, false);
			
			
			//create outputs
			FHost.CreateValueOutput("Position ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out position);
			position.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
			
			FHost.CreateValueOutput("Normal ", 3, null, TSliceMode.Dynamic, TPinVisibility.True, out normal);
			normal.SetSubType3D(double.MinValue, double.MaxValue, 0.01, 0, 0, 0, false, false, false);
			
			FHost.CreateValueOutput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out indices);
			indices.SetSubType(0, int.MaxValue, 1, 0, false, false, true);
			
			
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
			//initialize metaball arrays
			
			if (pos.SliceCount != NumBalls)
			{
				NumBalls = pos.SliceCount;
				
				
				m_Ball = new Vector3[NumBalls];
				m_BallMass = new float[NumBalls];
				update = true;
				
			}
			
			//initialize grid variables and arrays
			
			if (gsize.PinIsChanged)
			{
				double dSize;
				gsize.GetValue(1, out dSize);
				VoxelSize = 2/(float) dSize;
				GridSize = (int) dSize;
				
				update = true;
			}
			
			if (level.PinIsChanged)
			{
				double dLevel;
				level.GetValue(1, out dLevel);
				Level = (float) dLevel;
				update = true;
			}
			

			
			if (mass.PinIsChanged || pos.PinIsChanged || update)
			{
				update = true;
				
				
				//initialize all tables
				
				GridEnergy = new float[(GridSize+1)*(GridSize+1)*(GridSize+1)];
				GridPointStatus = new bool[(GridSize+1)*(GridSize+1)*(GridSize+1)];
				GridVoxelStatus = new byte[GridSize*GridSize*GridSize];
				GridVoxelSeek = new int[GridSize*GridSize*GridSize];
				
				

				
				for (int i=0; i<NumBalls; i++)
				{
					double px, py, pz, pm;
					pos.GetValue3D(i, out px, out py, out pz);
					mass.GetValue(i, out pm);
					
					Vector3 p3 = new Vector3((float)px, (float)py, (float)pz);
					
					m_Ball[i] = p3;

					m_BallMass[i] = (float)pm;

				}

				Render();
				
				// Output data

				#region unsafe 2.1.1
				/*
				
				
				// FALLBACK TO ORIGINAL PLUGIN.
				// enable use of Position, Normal and Indices output pins
				// because
				// in MeshOutput version Vertices and Normals are
				// SlimDX Vector3 type, not Vector3D type
				
				
				
				// unsafe is fast
				
				position.SliceCount = NumVertices;
				normal.SliceCount   = NumVertices;
				indices.SliceCount  = NumIndices;
				
				
				unsafe
				{
					double* pVertices, pNormals, pIndices;
					
					position.GetValuePointer(out pVertices);
					normal.GetValuePointer(out pNormals);
					indices.GetValuePointer(out pIndices);
					

					Vector3D[] Vertices2 = new Vector3D[NumVertices];
					Vector3D[] Normals2 = new Vector3D[NumVertices];
					
					for (int k = 0; k < NumVertices; k++)
					{
						Vertices2[k].x = (double)Vertices[k].X;
						Vertices2[k].y = (double)Vertices[k].Y;
						Vertices2[k].z = (double)Vertices[k].Z;
						
						Normals2[k].x = (double)Normals[k].X;
						Normals2[k].y = (double)Normals[k].Y;
						Normals2[k].z = (double)Normals[k].Z;
						
					}
					
					
					
					// fix all arrays before copying
									
					
					fixed (Vector3D* fixTemp = &Vertices2[0])
					{
						double* fixVertices = (double*) fixTemp;  // cast Vector3D* to double*
						for (int j = 0; j < NumVertices * 3; j++) // and collect XYZ in order
						{
							*(pVertices+j) = *(fixVertices+j);    // this is fast!
						}
					}	
					
					fixed (Vector3D* fixTemp = &Normals2[0])
					{
						double* fixNormals = (double*) fixTemp;
						for (int j = 0; j < NumVertices * 3; j++)
						{
							*(pNormals+j) = *(fixNormals+j);
						}
					}
					
					fixed (int* fixIndices = &Indices[0])
					{
						for (int j = 0; j < NumIndices; j++)
						{
							*(pIndices+j) = (double) *(fixIndices+j);
						}
					}
					
				}
				
				*/
				#endregion unsafe
				
				
				#region MeshOutput
				
				
				// with some more code changes VxBuffer would be
				// built already, but for now we have to build it from arrays
				// in every frame
				
				VxBuffer = new sVxBuffer[NumVertices];
				
				for (int j = 0; j < NumVertices; j++)
				{
					VxBuffer[j].Vel = Vertices[j];
					VxBuffer[j].Nel = Normals[j];
				}
				
				// this is not taken care of in metaballs code yet
				// cast int indicices to short IxBuffer
				
				
				IxBuffer = new short[NumIndices];
				
				for (int j = 0; j < NumIndices; j++)
				{
					IxBuffer[j] = (short)Indices[j];
				}
				
				
				FMyMeshOutput.SliceCount = 1;
				
							
				#endregion MeshOutput
			}

		}
		
		#endregion mainloop
		
		#region DXMesh
		
		
		public void UpdateResource(IPluginOut ForPin, int OnDevice)
		{
			
			Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) 
			                            {return ms.Device.ComPointer == (IntPtr)OnDevice;});
			
			//if resource is not yet created on given Device, create it now
			if (m == null)
			{
				FHost.Log(TLogType.Debug, "Creating Resource...");
				//Device dev = Device.FromPointer(new IntPtr(OnDevice));
				
				//FDeviceMeshes.Add(Mesh.CreateTeapot(dev));
				update = true;
				
				
			}
			if (update)
			{
				
				Device dev = Device.FromPointer(new IntPtr(OnDevice));
							
				// create new Mesh
				Mesh NewMesh = new Mesh(dev, NumIndices/3, NumVertices, 
				                  MeshFlags.Managed,
				                  VertexFormat.PositionNormal);
				
				// lock buffers
				DataStream sVx = NewMesh.VertexBuffer.Lock(0, 0, LockFlags.Discard);
				DataStream sIx = NewMesh.IndexBuffer.Lock(0, 0, LockFlags.Discard);
				
				// write buffers
				// this can be done without int offset and int count also
								
				sVx.WriteRange(VxBuffer, 0, NumVertices);
				sIx.WriteRange(IxBuffer, 0, NumIndices);
				
				// unlock buffers
				NewMesh.VertexBuffer.Unlock();
				NewMesh.IndexBuffer.Unlock();
				
				//dispose streams
				sVx.Dispose();
				sIx.Dispose();
				
				// don't think this is necessary, trying to solve memory leak
				sVx = null;
				sIx = null;
				
				// remove old mesh, add new mesh and dispose old mesh
				
				FDeviceMeshes.Remove(m);
				FDeviceMeshes.Add(NewMesh);
								
				m.Dispose();
				m = null;
				
				
				update = false;
				
				
			}
		}
		
		public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
		{
			
			//dispose resources that were created on given Device
			Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) 
			                            {return ms.Device.ComPointer == (IntPtr)OnDevice;});
			
			if (m != null)
			{
				FHost.Log(TLogType.Debug, "Destroying Resource...");
				FDeviceMeshes.Remove(m);
				m.Dispose();
				m = null;
			}
		}
		
		public void GetMesh(IDXMeshIO ForPin, int OnDevice, out int Mesh)
		{
			Mesh = 0;
			if (ForPin == FMyMeshOutput)
			{

				Mesh m = FDeviceMeshes.Find(delegate(Mesh ms) 
				                            {return ms.Device.ComPointer == (IntPtr)OnDevice;});
				if (m != null)
					Mesh = m.ComPointer.ToInt32();
			}
		}
		

		
		
		
		#endregion
		
		#region metaballs
		
		protected void Render()
		{
			int nCase = 255;
			int x, y, z;
			bool bComputed;
			
			MaxOpenVoxels = 1024;
			
			NumOpenVoxels = 0;
			OpenVoxels = new int[MaxOpenVoxels * 3];
			PreComputed = new int[MaxOpenVoxels * 12];
			
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
				
				// TODO add some finite fall-off formulas and other primitives
				
				

				fSqDist = (m_Ball[i].X - x)*(m_Ball[i].X - x) +
					(m_Ball[i].Y - y)*(m_Ball[i].Y - y) +
					(m_Ball[i].Z - z)*(m_Ball[i].Z - z);
				
				
				if( fSqDist < 0.0001f ) fSqDist = 0.0001f;

				fEnergy += m_BallMass[i] / fSqDist;
			}

			return fEnergy;
		}
		
		
		#region ComputeNormal
		
		protected void  ComputeNormal(int Vertex)
		{
			float fSqDist;
			Vector3 Normal = new Vector3(0);
			
			for( int i = 0; i < NumBalls; i++ )
			{
				// To compute the normal we derive the energy formula and get
				//
				//   n += 2 * mass * vector / distance^4
				
				Vector3 xyz = Vertices[Vertex] - m_Ball[i];
								
				fSqDist = xyz.LengthSquared();
				

				Normal.X += 2 * m_BallMass[i] * xyz.X / (fSqDist * fSqDist);
				Normal.Y += 2 * m_BallMass[i] * xyz.Y / (fSqDist * fSqDist);
				Normal.Z += 2 * m_BallMass[i] * xyz.Z / (fSqDist * fSqDist);

			}

			//normalize vector
			
			Normal.Normalize(); // SlimDx!
			Normals[Vertex] = Normal;

			// To compute the sphere-map texture coordinates
			// normals should be transformed to camera space...
			
			//Texture[Vertex].x = Normals[Vertex].x/2 + 0.5f;
			//Texture[Vertex].y = -Normals[Vertex].y/2 + 0.5f;
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

			GridEnergy[address3D] = ComputeEnergy(fx,fy,fz);

			SetGridPointComputed(x,y,z);

			return GridEnergy[address3D];
		}
		
		
		protected int ComputeGridVoxel(int x, int y, int z)
		{
			
			float[] b = new float[8];

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

					Vertices[NumVertices].X =
						MarchingCubes.CubeVertices[nIndex0, 0]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 0]*t;
					Vertices[NumVertices].Y =
						MarchingCubes.CubeVertices[nIndex0, 1]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 1]*t;
					Vertices[NumVertices].Z =
						MarchingCubes.CubeVertices[nIndex0, 2]*(1-t) +
						MarchingCubes.CubeVertices[nIndex1, 2]*t;
					
					fx = Grid2World(x);
					fy = Grid2World(y);
					fz = Grid2World(z);
					
					Vertices[NumVertices].X = fx +
						Vertices[NumVertices].X * VoxelSize;
					Vertices[NumVertices].Y = fy +
						Vertices[NumVertices].Y * VoxelSize;
					Vertices[NumVertices].Z = fz +
						Vertices[NumVertices].Z * VoxelSize;

					// Compute the normal at the vertex
					ComputeNormal(NumVertices);

					NumVertices++;
					if (NumVertices == MaxVertices)
					{
						MaxVertices *= 2;
						Vector3[] TmpVx = new Vector3[MaxVertices];
						Vector3[] TmpNx = new Vector3[MaxVertices];
						//Vector2D[] TmpTx = new Vector2D[MaxVertices];
						int j = 0;
						foreach (Vector3 element in Vertices)
						{
							TmpVx[j] = element;
							j++;
						}
						j = 0;
						
						foreach (Vector3 element in Normals)
						{
							TmpNx[j] = element;
							j++;
						}
						
						Vertices = TmpVx;
						Normals = TmpNx;
						
						
					}
				}

				// Add the edge's vertex index to the index list
				
				Indices[NumIndices] = EdgeIndices[nEdge];

				NumIndices++;
				if (NumIndices == MaxIndices)
				{
					MaxIndices *= 2;
					int[] TmpIx = new int[MaxIndices];
					int j = 0;
					foreach (int element in Indices)
					{
						TmpIx[j] = element;
						j++;
					}
					Indices = TmpIx;
				}
				
				i++;
			}

			SetGridVoxelComputed(x,y,z);
			
			
			return c;
		}

		protected bool  IsGridPointComputed(int x, int y, int z)
		{
			if( GridPointStatus[x +
			                    y*(GridSize+1) +
			                    z*(GridSize+1)*(GridSize+1)] == true )
				return true;
			else
				return false;
		}
		
		protected bool  IsGridVoxelComputed(int x, int y, int z)
		{
			if( GridVoxelStatus[x +
			                    y*GridSize +
			                    z*GridSize*GridSize] == 1 )
				return true;
			else
				return false;
		}
		
		protected bool  IsGridVoxelInList(int x, int y, int z)
		{
			if( GridVoxelStatus[x +
			                    y*GridSize +
			                    z*GridSize*GridSize] == 2 )
				return true;
			else
				return false;
		}
		
		protected void  SetGridPointComputed(int x, int y, int z)
		{
			GridPointStatus[x +
			                y*(GridSize+1) +
			                z*(GridSize+1)*(GridSize+1)] = true;
		}
		
		protected void  SetGridVoxelComputed(int x, int y, int z)
		{
			GridVoxelStatus[x +
			                y*GridSize +
			                z*GridSize*GridSize] = 1;
		}
		
		protected void  SetGridVoxelInList(int address)
		{
			
			GridVoxelStatus[address] = 2;
			GridVoxelSeek[address] = NumOpenVoxels;
		}

		protected float Grid2World(int x)
		{
			return (float)x*VoxelSize - 1.0f;
		}
		
		protected int   World2Grid(double x)
		{
			return (int)((x + 1.0f)/VoxelSize + 0.5f);
		}
		
		protected void  AddNeighborsToList(int nCase, int x, int y, int z)
		{
			if( (MarchingCubes.CubeNeighbors[nCase] & 1) != 0 )
				AddNeighbor(x+1, y, z, 1);

			if( (MarchingCubes.CubeNeighbors[nCase] & 2) != 0 )
				AddNeighbor(x-1, y, z, 2);

			if( (MarchingCubes.CubeNeighbors[nCase] & 4) != 0 )
				AddNeighbor(x, y+1, z, 4);

			if( (MarchingCubes.CubeNeighbors[nCase] & 8) != 0 )
				AddNeighbor(x, y-1, z, 8);

			if( (MarchingCubes.CubeNeighbors[nCase] & 16) != 0 )
				AddNeighbor(x, y, z+1, 16);

			if( (MarchingCubes.CubeNeighbors[nCase] & 32) != 0 )
				AddNeighbor(x, y, z-1, 32);
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
				PreComputeEdge(GridVoxelSeek[address], side);
				return;
			}

			// Make sure the arrays are large enough
			
			if( MaxOpenVoxels == NumOpenVoxels )
			{
				MaxOpenVoxels += 1024;
				int[] OVTmp = new int[MaxOpenVoxels * 3];
				int[] PCTmp = new int[MaxOpenVoxels * 12];
				int j = 0;
				foreach (int element in OpenVoxels)
				{
					OVTmp[j] = element;
					j++;
				}
				j = 0;
				foreach (int element in PreComputed)
				{
					PCTmp[j] = element;
					j++;
				}
				//***** enlarge arrays
				
				OpenVoxels = OVTmp;
				PreComputed = PCTmp;
				
				
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
			
			
			OpenVoxelAdr = OpenVoxelAdr * 12; // there are 12 edges
			
			if (side == 1) // x+1
			{
				PreComputed[OpenVoxelAdr + 3] = EdgeIndices[1] + 1;
				PreComputed[OpenVoxelAdr + 7] = EdgeIndices[5] + 1;
				PreComputed[OpenVoxelAdr + 8] = EdgeIndices[9] + 1;
				PreComputed[OpenVoxelAdr + 10] = EdgeIndices[11] + 1;
			}
			
			if (side == 2) // x-1
			{
				PreComputed[OpenVoxelAdr + 1] = EdgeIndices[3] + 1;
				PreComputed[OpenVoxelAdr + 5] = EdgeIndices[7] + 1;
				PreComputed[OpenVoxelAdr + 9] = EdgeIndices[8] + 1;
				PreComputed[OpenVoxelAdr + 11] = EdgeIndices[10] + 1;
			}
			
			if (side == 4) // y+1
			{
				PreComputed[OpenVoxelAdr + 0] = EdgeIndices[4] + 1;
				PreComputed[OpenVoxelAdr + 1] = EdgeIndices[5] + 1;
				PreComputed[OpenVoxelAdr + 2] = EdgeIndices[6] + 1;
				PreComputed[OpenVoxelAdr + 3] = EdgeIndices[7] + 1;
			}
			
			if (side == 8) // y-1
			{
				PreComputed[OpenVoxelAdr + 4] = EdgeIndices[0] + 1;
				PreComputed[OpenVoxelAdr + 5] = EdgeIndices[1] + 1;
				PreComputed[OpenVoxelAdr + 6] = EdgeIndices[2] + 1;
				PreComputed[OpenVoxelAdr + 7] = EdgeIndices[3] + 1;
			}
			
			if (side == 16) // z+1
			{
				PreComputed[OpenVoxelAdr + 0] = EdgeIndices[2] + 1;
				PreComputed[OpenVoxelAdr + 4] = EdgeIndices[6] + 1;
				PreComputed[OpenVoxelAdr + 8] = EdgeIndices[10] + 1;
				PreComputed[OpenVoxelAdr + 9] = EdgeIndices[11] + 1;
			}
			
			if (side == 32) // z-1
			{
				PreComputed[OpenVoxelAdr + 2] = EdgeIndices[0] + 1;
				PreComputed[OpenVoxelAdr + 6] = EdgeIndices[4] + 1;
				PreComputed[OpenVoxelAdr + 10] = EdgeIndices[8] + 1;
				PreComputed[OpenVoxelAdr + 11] = EdgeIndices[9] + 1;
			}
			
			
			
		}

		
		
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
		protected Vector3[]  	Vertices = new Vector3[4000];
		protected Vector3[]		Normals  = new Vector3[4000];
//		protected Vector2[]		Texture  = new Vector2[4000];
		protected int[]			Indices  = new int[8000];
		protected int			MaxVertices = 4000;
		protected int			MaxIndices  = 8000;
		

		
		#endregion
		
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
		
		
		protected sVxBuffer[]	VxBuffer;
		protected short[]		IxBuffer;

		
		
	}
}
