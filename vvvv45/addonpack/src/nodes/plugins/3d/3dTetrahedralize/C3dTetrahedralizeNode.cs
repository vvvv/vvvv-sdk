#region usings
using System;
using System.ComponentModel.Composition;
using System.Text;
using System.Collections.Generic;
using System.Linq;


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
	[PluginInfo(Name = "Tetrahedralize", Category = "3d", Author="digitalWannabe", Credits = "Hang Si,Weierstrass Institute for Applied Analysis and Stochastics (WIAS),lichterloh",Help = "Tetrahedral Mesh Generator and 3D Delaunay Triangulator", Tags = "TetGen, Mesh, 3D Delaunay, dope")]
	#endregion PluginInfo
	public unsafe class C3dTetrahedralizeNode : IPluginEvaluate, IDisposable
    {

    static C3dTetrahedralizeNode()
    {
            var platform = IntPtr.Size == 4 ? "x86" : "x64";
            var pathToThisAssembly = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var pathToBinFolder = Path.Combine(pathToThisAssembly, "dependencies", platform);
            var envPath = Environment.GetEnvironmentVariable("PATH");
            envPath = string.Format("{0};{1}", envPath, pathToBinFolder);
            Environment.SetEnvironmentVariable("PATH", envPath);
        }


    #region fields & pins
    	[Input("Input ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FVec;
    	
//    	[Input("Vertex Attributes", BinVisibility = PinVisibility.OnlyInspector)]
//		public ISpread<ISpread<double>> FPA;
    
		[Input("Vertex Markers", DefaultValue = 1, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FPM;
    	
		[Input("Polygons", DefaultValue = 1, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FPoly;
		
		[Input("Vertices", DefaultValue = 3, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FNVert;
		
		[Input("Vertex Indices", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FVI;
		
		[Input("Facet Holes", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FFH;
		
		[Input("Facet Hole Indicators ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FFHI;
		
		[Input("Facet Markers", DefaultValue = 1, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FFM;
		
		[Input("Holes")]
		public ISpread<int> FHO;
		
		[Input("Hole Indicators ", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector3D>> FHOI;
		
		[Input("Regions")]
		public ISpread<int> FR;
		
		[Input("Region Indicators ", BinVisibility = PinVisibility.OnlyInspector)]
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
		
		
		[Output("Output ", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<Vector3D>> FPoints;
		
		[Output("Triangle Indices", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FTri;
		
		[Output("Tetrahedron Indices", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FTet;
    	
    	[Output("Region Attributes", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<double>> FRegAttr;
    	
    	[Output("Vertex Markers", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FPtMarker;
    	
//    	[Output("Vertex Attributes", BinVisibility = PinVisibility.OnlyInspector)]
//		public ISpread<ISpread<double>> FPtAttr;
    	
    	[Output("Face Markers", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FFaceMarker;
    	
    	[Output("Tetrahedron Neighbors", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FTetNeighbor;
    	
//    	[Output("Debug")]
//		public ISpread<int> FDebug;
/*		
		[Output("Points/Faces/Tetrahedras")]
		public ISpread<Vector3D> FStats;
*/
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		[System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
        private static extern IntPtr tetCalculate(String behaviour, double[] vertXYZ,double[] vertAttr,int[] vertMarker,int[] numPoly,int[] numVertices,int[] vertIndex,int[] numFHoles, double[] fHoleXYZ,int[] facetMarker, double[] HoleXYZ, double[] RegionXYZ, double[] RegionAttrib, double[] RegionVolConst, int[] binSizes, String fileName);
		
    	[System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
		private static extern void getValues([In, Out] double[] _vertXYZ, [In, Out] int[] _triIndices, [In, Out] int[] _tetIndices,  [In, Out] double[] _regionMarker, [In, Out] int[] _pointMarker, [In, Out] int[] _faceMarker, [In, Out] int[] _pointAttr, [In, Out] int[] _neighborList);
    
		[System.Runtime.InteropServices.DllImport("TetGen2vvvv.dll")]
		private static extern int ReleaseMemory(IntPtr ptr);


		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			SpreadMax = SpreadUtils.SpreadMax(FVec, FPoly,FNVert,FVI,FFH,FFHI,FFM,FHOI,FRI,FRA,FRVC,FHO,FR,FWriteIn,FFile,FWriteOut/*,FPA,FPM*/);
			FPoints.SliceCount=SpreadMax;
			FTri.SliceCount=SpreadMax;
			FTet.SliceCount=SpreadMax;
			FRegAttr.SliceCount=SpreadMax;
			FPtMarker.SliceCount=SpreadMax;
			FFaceMarker.SliceCount=SpreadMax;
			FTetNeighbor.SliceCount=SpreadMax;
//			FDebug.SliceCount=SpreadMax;
//			FPtAttr.SliceCount=SpreadMax;
			
			for (int binID=0; binID<SpreadMax;binID++){
			
			string bhvr = FB[binID];
			string fileName=FFile[binID];
			
			int computeNeighbors = Convert.ToInt32(bhvr.Contains("n"));
			int computeRegionAttr = Convert.ToInt32(bhvr.Contains("A"));	
			
//			//always compute neighbors
//			bhvr +="n";
//			FDebug[0]=computeRegionAttr;
			int entries = FVec[binID].SliceCount;
			int entriesXYZ =entries*3;
			
			int numFacets = FPoly[binID].SliceCount;
			int numHoles = FHO[binID];
			int numRegions = FR[binID];
			int writeIn = Convert.ToInt32(FWriteIn[binID]);
			int writeOut = Convert.ToInt32(FWriteOut[binID]);

            var help = new Helpers();
			
			double[] V = new double[entriesXYZ];
			V = help.Vector3DToArray(V,FVec[binID]);
			
			int[] nP = new int[numFacets];
			nP=FPoly[binID].ToArray();
			
			int numVert=FNVert[binID].SliceCount;
			int[] nV = new int[numVert];
			nV=FNVert[binID].ToArray();
			
			int numFHolesXYZ=FFHI[binID].SliceCount*3;
			double[] FfHI = new double[numFHolesXYZ];	//test	
			FfHI = help.Vector3DToArray(FfHI,FFHI[binID]);
			
			int numIndices = FVI[binID].SliceCount;
			int[] VI = new int[numIndices];
			VI=FVI[binID].ToArray();			
			for(int nInd=0;nInd<numIndices;nInd++) VI[nInd]+=1;//tetgen expects indices starting at 1
			
			//facet marker
			int[] FM = new int[numFacets];
			help.SpreadToArray(FM,FFM[binID]); //can not use toArray() here, as Slicecount may be smaller than numFacets
			
			//facet holes
			int[] FfH = new int[numFacets];
			help.SpreadToArray(FfH,FFH[binID]); //can not use toArray() here, as Slicecount may be smaller than numFacets

            //hole indicators
            int sizeHI=FHOI[binID].SliceCount*3;
			double[] HI = new double[sizeHI];
			HI= help.Vector3DToArray(HI,FHOI[binID]);
			
			int sizeRI=FRI[binID].SliceCount*3;
			double[] RI = new double[sizeRI];
			RI= help.Vector3DToArray(RI,FRI[binID]);
			
			int sizeRA=FRA[binID].SliceCount;
			double[] RA = new double[sizeRA];
			RA=FRA[binID].ToArray();
			
			int sizeRVC=FRVC[binID].SliceCount;			
			double[] RVC = new double[sizeRVC];
			RVC=FRVC[binID].ToArray();
				
			//point Markers
			int[] PM = new int[entries];
			help.SpreadToArray(PM,FPM[binID]); //can not use toArray() here, as Slicecount may be smaller than entries

            //point Attributes
            double[] PA = new double[entries];
//			help.SpreadToArray(PA,FPA[binID]);//can not use toArray() here, as Slicecount may be smaller than entries	

            int[] binSizes = new int[8];
			binSizes[0]=entries;
			binSizes[1]=numFacets;
			binSizes[2]=numHoles;
			binSizes[3]=numRegions;
			binSizes[4]=writeIn;
			binSizes[5]=writeOut;
			binSizes[6]=computeNeighbors;
			binSizes[7]=computeRegionAttr;	
				
			
			
			if (FCal[0]){
				
				try
				{
									
				IntPtr tet = tetCalculate(bhvr,V,PA,PM, nP,nV,VI,FfH,FfHI,FM,HI,RI,RA,RVC,binSizes,fileName);
				int size=5;
				int[] tetArr = new int[size];
				Marshal.Copy(tet, tetArr,0,size );
					
				int nOfPoints = tetArr[0];
				int nOfFaces = tetArr[1];
				int nOfTet = tetArr[2];
				int nOfTetAttr = tetArr[3];
				int nOfPointAttr = tetArr[4];
				
					
				int nOfTriIndices = nOfFaces*3;
				int nOfTetIndices = nOfTet*4;
					
				double[] _vertXYZ= new double[nOfPoints*3];
				int[] _triIndices= new int[nOfTriIndices];
				int[] _tetIndices= new int[nOfTetIndices];
				double[] _regionAttr = new double[nOfTet];
				int[] _pointMarker= new int[nOfPoints];
				int[] _faceMarker= new int[nOfFaces];
				int[] _pointAttr= new int[nOfPoints];
				int[] _neighborList=new int[nOfTetIndices];
				
				FPoints[binID].SliceCount = nOfPoints;
//				FPtAttr[binID].SliceCount = nOfPoints*nOfPointAttr;
				FTri[binID].SliceCount = nOfTriIndices;
				FTet[binID].SliceCount = nOfTetIndices;				
				FPtMarker[binID].SliceCount = nOfPoints;
				FFaceMarker[binID].SliceCount=nOfFaces;
				
					
				getValues(_vertXYZ,_triIndices,_tetIndices,_regionAttr,_pointMarker, _faceMarker,_pointAttr,_neighborList);
					
				
				for (int i = 0; i < nOfPoints; i++){
					FPoints[binID][i]= new Vector3D(_vertXYZ[i*3],_vertXYZ[i*3+1],_vertXYZ[i*3+2]);
					FPtMarker[binID][i]=_pointMarker[i];
//					for (int j = 0; j < nOfPointAttr; j++){ //more than 1 attribute possible? what for?
//						FPtAttr[binID][i]= _pointAttr[i];
//						}

					}
				for (int i = 0; i < nOfFaces; i++){
					for (int j = 0; j < 3; j++){
						FTri[binID][i*3+j] = _triIndices[i*3+j]-1;
						}
					FFaceMarker[binID][i]=_faceMarker[i];
					}
				for (int i = 0; i < nOfTet; i++){
					if (computeRegionAttr>0){
						FRegAttr[binID].SliceCount = nOfTet*nOfTetAttr;	
						for (int j = 0; j < nOfTetAttr; j++){ //more than 1 attribute possible? what for?
							FRegAttr[binID][i]= _regionAttr[i];
							}
						}else{
							FRegAttr[binID].SliceCount = 0;	
						}
					
					for (int j = 0; j < 4; j++){
						FTet[binID][i*4+j] = _tetIndices[i*4+j]-1;
						if (computeNeighbors>0){
							FTetNeighbor[binID].SliceCount = nOfTetIndices;
							FTetNeighbor[binID][i*4+j] = _neighborList[i*4+j]-1;
							}else{
								FTetNeighbor[binID].SliceCount = 0;
							}
						
						}
					}
				
				ReleaseMemory(tet);  
				}
						
			finally
			{
				
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

    //// REMOVE REGIONS PLUGIN


    #region PluginInfo
    [PluginInfo(Name = "RemoveRegions", Category = "3D", Author = "digitalWannabe", Credits = "lichterloh", Help = "Remove regions (vertices + indices) by vertex or tetrahedron markers", Tags = "TetGen, Mesh, Region, 3D Delaunay, dope")]
    #endregion PluginInfo
    public class C3DRemoveRegionsNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input ")]
        public IDiffSpread<Vector3D> FInputVertices;

        [Input("Triangle Indices")]
        public IDiffSpread<int> FTriangleIndices;

        [Input("Tetrahedron Indices")]
        public IDiffSpread<int> FTetIndices;

        [Input("Region Markers")]
        public IDiffSpread<int> FRegionMarkers;

        [Input("Remove Regions")]
        public IDiffSpread<int> FRemoveRegion;

        [Input("Remove By Points/Tetrahedra")]
        public IDiffSpread<bool> FRemoveBy;

//        [Input("Evaluate")]
//        public ISpread<bool> FEval;


        [Output("Output ")]
        public ISpread<Vector3D> FOutputVertices;

        [Output("Triangle Indices")]
        public ISpread<int> FTriangleIndicesOut;

        [Output("Tetrahedron Indices")]
        public ISpread<int> FTetIndicesOut;

        //		[Output("Debug")]
        //		public ISpread<int> FDebug;

        [Output("Removed Vertex Indices")]
        public ISpread<int> FRemovedIndices;

        [Output("Select")]
        public ISpread<bool> FSelect;

        [Import()]
        public ILogger FLogger;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            List<int> deleteIndex = new List<int>();
            List<int> NewTriIndices = new List<int>();
            List<int> NewTetIndices = new List<int>();

            if (FInputVertices.IsChanged || FTriangleIndices.IsChanged || FTetIndices.IsChanged || FRemoveBy.IsChanged || FRemoveRegion.IsChanged || FRegionMarkers.IsChanged)
            {

                if (FRemoveBy[0])
                {
                    for (int i = 0; i < FRegionMarkers.SliceCount; i++)
                    {
                        for (int j = 0; j < FRemoveRegion.SliceCount; j++)
                        {
                            if (FRegionMarkers[i] != FRemoveRegion[j])
                            {
                                //						for (int k = 0; k < 4; k++){
                                //							deleteIndex.Add(FTetIndices[i*4+k]);
                                //							}
                                NewTetIndices.Add(FTetIndices[i * 4]);
                                NewTetIndices.Add(FTetIndices[i * 4 + 1]);
                                NewTetIndices.Add(FTetIndices[i * 4 + 2]);
                                NewTetIndices.Add(FTetIndices[i * 4 + 3]);
                            }
                        }
                    }


                    for (int g = 0; g < FInputVertices.SliceCount; g++)
                    {
                        if (!NewTetIndices.Contains(g)) deleteIndex.Add(g);
                    }

                }
                else {


                    for (int i = 0; i < FRegionMarkers.SliceCount; i++)
                    {
                        for (int j = 0; j < FRemoveRegion.SliceCount; j++)
                        {
                            if (FRegionMarkers[i] == FRemoveRegion[j])
                            {
                                deleteIndex.Add(i);
                            }

                        }
                    }

                    for (int g = 0; g < FTetIndices.SliceCount / 4; g++)
                    {

                        if (!(deleteIndex.Contains(FTetIndices[g * 4]) || deleteIndex.Contains(FTetIndices[g * 4 + 1]) || deleteIndex.Contains(FTetIndices[g * 4 + 2]) || deleteIndex.Contains(FTetIndices[g * 4 + 3])))
                        {
                            NewTetIndices.Add(FTetIndices[g * 4]);
                            NewTetIndices.Add(FTetIndices[g * 4 + 1]);
                            NewTetIndices.Add(FTetIndices[g * 4 + 2]);
                            NewTetIndices.Add(FTetIndices[g * 4 + 3]);
                        }

                    }
                }

                for (int g = 0; g < FTriangleIndices.SliceCount / 3; g++)
                {

                    if (!(deleteIndex.Contains(FTriangleIndices[g * 3]) || deleteIndex.Contains(FTriangleIndices[g * 3 + 1]) || deleteIndex.Contains(FTriangleIndices[g * 3 + 2])))
                    {
                        NewTriIndices.Add(FTriangleIndices[g * 3]);
                        NewTriIndices.Add(FTriangleIndices[g * 3 + 1]);
                        NewTriIndices.Add(FTriangleIndices[g * 3 + 2]);
                    }

                }

                FTriangleIndicesOut.SliceCount = NewTriIndices.Count;
                FTriangleIndicesOut.AssignFrom(NewTriIndices);

                FTetIndicesOut.SliceCount = NewTetIndices.Count;
                FTetIndicesOut.AssignFrom(NewTetIndices);

                FRemovedIndices.SliceCount = deleteIndex.Count;
                FRemovedIndices.AssignFrom(deleteIndex);

                FOutputVertices.SliceCount = FInputVertices.SliceCount;
                FOutputVertices.AssignFrom(FInputVertices);

                deleteIndex.Reverse();

                for (int g = 0; g < FTriangleIndicesOut.SliceCount; g++)
                {
                    for (int h = 0; h < deleteIndex.Count; h++)
                    {
                        if (FTriangleIndicesOut[g] >= deleteIndex[h]) FTriangleIndicesOut[g] -= 1;
                    }
                }

                for (int g = 0; g < FTetIndicesOut.SliceCount; g++)
                {
                    for (int h = 0; h < deleteIndex.Count; h++)
                    {
                        if (FTetIndicesOut[g] >= deleteIndex[h]) FTetIndicesOut[g] -= 1;
                    }
                }

                for (int vecID = 0; vecID < deleteIndex.Count; vecID++)
                {
                    FOutputVertices.RemoveAt(deleteIndex[vecID]);
                }



                FSelect.SliceCount = FInputVertices.SliceCount;
                for (int i = 0; i < FInputVertices.SliceCount; i++)
                {

                    FSelect[i] = !deleteIndex.Contains(i);

                }
                //FLogger.Log(LogType.Debug, "hi tty!");


            }

        }
    }



    /// HELPERS
    /// 

    public class Helpers
    {
        public double[] Vector3DToArray(double[] V, ISpread<Vector3D> VertexSpread)
        {
            int entries = VertexSpread.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                V[i * 3] = VertexSpread[i].x;
                V[i * 3 + 1] = VertexSpread[i].y;
                V[i * 3 + 2] = VertexSpread[i].z;
            }
            return V;
        }

        public double[] Vector3DToArray2D(double[] V, ISpread<Vector3D> VertexSpread)
        {
            int entries = VertexSpread.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                V[i * 2] = VertexSpread[i].x;
                V[i * 2 + 1] = VertexSpread[i].y;
            }
            return V;
        }

        public double[] Matrix4x4ToArray(double[] V, ISpread<Matrix4x4> Transform)
        {
            int entries = Transform.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                double[] trans = Transform[i].Values;
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 4; k++)
                    {
                        V[i * 16 + j * 4 + k] = trans[j * 4 + k];
                    }
                }
            }
            return V;
        }

        public double[] Matrix4x4ToArray3x4(double[] V, ISpread<Matrix4x4> Transform)
        {
            int entries = Transform.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                double[] trans = Transform[i].Values;
                for (int j = 0; j < 4; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        V[i * 12 + j * 3 + k] = trans[j * 4 + k];
                    }
                }
            }
            return V;
        }

        public int[] IndicesToArray(int[] I, ISpread<int> IndexSpread)
        {
            int entries = IndexSpread.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                I[i] = IndexSpread[i];
            }
            return I;
        }


        public void SpreadToArray<T>(T[] I, ISpread<T> Spread)
        {
            int entries = Spread.SliceCount;
            for (int i = 0; i < entries; i++)
            {
                I[i] = Spread[i];
            }
        }

    }


}

