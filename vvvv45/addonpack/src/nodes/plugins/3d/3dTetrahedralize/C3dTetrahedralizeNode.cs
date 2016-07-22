#region usings
using System;
using System.ComponentModel.Composition;
using System.Text;


using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using System.Runtime.InteropServices;
using System.IO;
using System.Reflection;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Tetrahedralize", Category = "3d", Author="digitalWannabe", Credits = "Hang Si,Weierstrass Institute for Applied Analysis and Stochastics (WIAS),lichterloh",Help = "Tetrahedral Mesh Generator and 3D Delaunay Triangulator", Tags = "TetGen, Mesh, 3D Delaunay")]
	#endregion PluginInfo
	public unsafe class C3dTetrahedralizeNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
    {

        
    public void OnImportsSatisfied()
    {
            var platform = IntPtr.Size == 4 ? "x86" : "x64";
            var pathToThisAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToBinFolder = Path.Combine(pathToThisAssembly, "dependencies", platform);
            var envPath = Environment.GetEnvironmentVariable("PATH");
            envPath = string.Format("{0};{1}", envPath, pathToBinFolder);
            Environment.SetEnvironmentVariable("PATH", envPath);
        }


    #region fields & pins
    [Input("Vertex ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FVec;
		
		[Input("Polygons", DefaultValue = 1.0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FPoly;
		
		[Input("Vertices", DefaultValue = 3.0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FNVert;
		
		[Input("Vertex Indices", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FVI;
		
		[Input("Facet Holes", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FFH;
		
		[Input("Facet Hole Indicator ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FFHI;
		
		[Input("Facet Marker", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FFM;
		
		[Input("Holes")]
		public ISpread<int> FHO;
		
		[Input("Hole Indicator ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FHOI;
		
		[Input("Regions")]
		public ISpread<int> FR;
		
		[Input("Region Indicator ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FRI;
		
		[Input("Region Attributes", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<double>> FRA;
		
		[Input("Region Volume Constraints", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<double>> FRVC;
		
		[Input("Behaviour", DefaultString="pq1.414a0.1")]
		public ISpread<string> FB;
		
		[Input("Generate Mesh", DefaultValue = 0.0, IsBang=true)]
		public ISpread<bool> FCal;
		
		[Input("Write Input Files", Visibility = PinVisibility.OnlyInspector)]
		public ISpread<bool> FWriteIn;
		
		[Input("Write Output Files", Visibility = PinVisibility.OnlyInspector)]
		public ISpread<bool> FWriteOut;
		
		[Input("Base Filename", DefaultString="file", Visibility = PinVisibility.OnlyInspector)]
		public ISpread<string> FFile;
		
		
		[Output("Vertex ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FPoints;
		
		[Output("Triangle Face Indices", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FTri;
		
		[Output("Tetrahedra Indices", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FTet;
/*		
		[Output("Points/Faces/Tetrahedras")]
		public ISpread<Vector3D> FStats;
*/
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		[System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
        private static extern IntPtr tetCalculate(ref int size, IntPtr behaviour, IntPtr vertXYZ,IntPtr numPoly,IntPtr numVertices,IntPtr vertIndex,IntPtr numFHoles, IntPtr fHoleXYZ,IntPtr facetMarker, IntPtr HoleXYZ, IntPtr RegionXYZ, IntPtr RegionAttrib, IntPtr RegionVolConst, IntPtr binSizes, IntPtr fileName);
		
		[System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
		private static extern int ReleaseMemory(IntPtr ptr);
		
		public static IntPtr NativeUtf8FromString(string managedString) {
        	int len = Encoding.UTF8.GetByteCount(managedString);
        	byte[] buffer = new byte[len + 1];
        	Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
        	IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        	Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
       	 	return nativeUtf8;
    		}
		
		public double[] Vector3DToArray(double[] V,ISpread<Vector3D> VertexSpread){
				int entries = VertexSpread.SliceCount;				
				for (int i=0; i<entries; i++){
				V[i*3]=VertexSpread[i].x;
				V[i*3+1]=VertexSpread[i].y;
				V[i*3+2]=VertexSpread[i].z;
			}
			return V;
		}
		
		public int[] IntegerToArray(int[] I,ISpread<int> IndexSpread){
				int entries = IndexSpread.SliceCount;
				for (int i=0; i<entries; i++){
				I[i]=IndexSpread[i];
			}
			return I;
		}
		
		public void SpreadToArray<T>(T[] I,ISpread<T> Spread){
				int entries = Spread.SliceCount;
				for (int i=0; i<entries; i++){
				I[i]=Spread[i];
			}
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = SpreadUtils.SpreadMax(FVec, FPoly,FNVert,FVI,FFH,FFHI,FFM,FHOI,FRI,FRA,FRVC,FHO,FR,FWriteIn,FFile,FWriteOut);
			FPoints.SliceCount=SpreadMax;
			FTri.SliceCount=SpreadMax;
			FTet.SliceCount=SpreadMax;
			
			
			int size=1;
			Vector3D temp;
			
			for (int binID=0; binID<SpreadMax;binID++){
			
			string bhvr = FB[binID];
			string fileName=FFile[binID];
			
			int entries = FVec[binID].SliceCount;
			int entriesXYZ =entries*3;
			
			int numFacets = FPoly[binID].SliceCount;
			int numHoles = FHO[binID];
			int numRegions = FR[binID];
			int writeIn = Convert.ToInt32(FWriteIn[binID]);
			int writeOut = Convert.ToInt32(FWriteOut[binID]);
			
			double[] V = new double[entriesXYZ];
			V = Vector3DToArray(V,FVec[binID]);
			
			int[] nP = new int[numFacets];
			SpreadToArray(nP,FPoly[binID]);
			
			int numVert=FNVert[binID].SliceCount;
			int[] nV = new int[numVert];
			SpreadToArray(nV,FNVert[binID]);
			

			int numFHolesXYZ=FFHI[binID].SliceCount*3;
			double[] FfHI = new double[numFHolesXYZ];	//test	
			FfHI = Vector3DToArray(FfHI,FFHI[binID]);
			
			int numIndices = FVI[binID].SliceCount;
			int[] VI = new int[numIndices];
			SpreadToArray(VI,FVI[binID]);
			
			for(int nInd=0;nInd<numIndices;nInd++) VI[nInd]+=1;//tetgen expects indices starting at 1
			
			//facet marker
			int[] FM = new int[numFacets];
			SpreadToArray(FM,FFM[binID]);
			
			//facet holes
			int[] FfH = new int[numFacets];
			SpreadToArray(FfH,FFH[binID]);
			
			//hole indicators
			int sizeHI=FHOI[binID].SliceCount*3;
			double[] HI = new double[sizeHI];
			HI= Vector3DToArray(HI,FHOI[binID]);
			
			int sizeRI=FRI[binID].SliceCount*3;
			double[] RI = new double[sizeRI];
			RI= Vector3DToArray(RI,FRI[binID]);
			
			int sizeRA=FRA[binID].SliceCount;
			double[] RA = new double[sizeRA];
			SpreadToArray(RA,FRA[binID]);
			
			int sizeRVC=FRVC[binID].SliceCount;			
			double[] RVC = new double[sizeRVC];
			SpreadToArray(RVC,FRVC[binID]);
			
			
			int[] binSizes = new int[6];
			binSizes[0]=entries;
			binSizes[1]=numFacets;
			binSizes[2]=numHoles;
			binSizes[3]=numRegions;
			binSizes[4]=writeIn;
			binSizes[5]=writeOut;
			
			
			
			if (FCal[0]){
				
				IntPtr BhvrPtr = NativeUtf8FromString(bhvr);
				IntPtr FileNamePtr = NativeUtf8FromString(fileName);
				
				IntPtr Vptr = Marshal.AllocHGlobal(entriesXYZ*sizeof(double));
				IntPtr Binptr = Marshal.AllocHGlobal(6*sizeof(int));
				IntPtr nVptr = Marshal.AllocHGlobal(numVert*sizeof(int));
				IntPtr VIptr = Marshal.AllocHGlobal(numIndices*sizeof(int));
				IntPtr FHIptr = Marshal.AllocHGlobal(numFHolesXYZ*sizeof(double));
				IntPtr FMptr = Marshal.AllocHGlobal(numFacets*sizeof(int));
				IntPtr nPptr = Marshal.AllocHGlobal(numFacets*sizeof(int));
				IntPtr FHptr = Marshal.AllocHGlobal(numFacets*sizeof(int));
				IntPtr HIptr = Marshal.AllocHGlobal(sizeHI*sizeof(double));
				IntPtr RIptr = Marshal.AllocHGlobal(sizeRI*sizeof(double));
				IntPtr RAptr = Marshal.AllocHGlobal(sizeRA*sizeof(double));
				IntPtr RVCptr = Marshal.AllocHGlobal(sizeRVC*sizeof(double));
				
				try
				{			
				
				Marshal.Copy(V, 0, Vptr, entriesXYZ);
				Marshal.Copy(binSizes, 0, Binptr, 6);
				Marshal.Copy(nV, 0, nVptr, numVert);
				Marshal.Copy(VI, 0, VIptr, numIndices);
				Marshal.Copy(FfHI, 0, FHIptr, numFHolesXYZ);
				Marshal.Copy(FM, 0, FMptr, numFacets);
				Marshal.Copy(nP, 0, nPptr, numFacets);
				Marshal.Copy(FfH, 0, FHptr, numFacets);
				Marshal.Copy(HI, 0, HIptr, sizeHI);
				Marshal.Copy(RI, 0, RIptr, sizeRI);
				Marshal.Copy(RA, 0, RAptr, sizeRA);
				Marshal.Copy(RVC, 0, RVCptr, sizeRVC);
				
				IntPtr tet = tetCalculate(ref size,BhvrPtr,Vptr,nPptr,nVptr,VIptr,FHptr,FHIptr,FMptr,HIptr,RIptr,RAptr,RVCptr,Binptr,FileNamePtr);
				double[] tetArr = new double[size];
				Marshal.Copy(tet, tetArr,0,size );
					
				int nOfPoints = (int)tetArr[0];
				int nOfTriIndices = (int)tetArr[1];
				int nOfTetIndices = (int)tetArr[2];
				
				
				FPoints[binID].SliceCount = nOfPoints;
				FTri[binID].SliceCount = nOfTriIndices;
				FTet[binID].SliceCount = nOfTetIndices;
			
				for (int i = 0; i < nOfPoints; i++){
					int j = 3+i*3;
					temp.x=tetArr[j];
					temp.y=tetArr[j+1];
					temp.z=tetArr[j+2];
					FPoints[binID][i] = temp;
				}
				for (int i = 0; i < nOfTriIndices; i++){
					int j = 3+nOfPoints*3+i;
					FTri[binID][i] = (int)tetArr[j]-1;
				}
				for (int i = 0; i < nOfTetIndices; i++){
					int j = 3+nOfPoints*3+nOfTriIndices+i;
					FTet[binID][i] = (int)tetArr[j]-1;
				}
				
				ReleaseMemory(tet);  
				}
						
			finally
			{
			  
				Marshal.FreeHGlobal(BhvrPtr);
				Marshal.FreeHGlobal(Vptr);
				Marshal.FreeHGlobal(Binptr);
				Marshal.FreeHGlobal(nVptr);
				Marshal.FreeHGlobal(VIptr);
				Marshal.FreeHGlobal(FHIptr);
				Marshal.FreeHGlobal(FMptr);
				Marshal.FreeHGlobal(nPptr);
				Marshal.FreeHGlobal(FHptr);
				Marshal.FreeHGlobal(HIptr);
				Marshal.FreeHGlobal(RIptr);
				Marshal.FreeHGlobal(RAptr);
				Marshal.FreeHGlobal(RVCptr);
				Marshal.FreeHGlobal(FileNamePtr);
				
			}
			}

			
		}
			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	public void Dispose()
         {
			//	Marshal.FreeHGlobal(Vptr);
         }
	}
}

