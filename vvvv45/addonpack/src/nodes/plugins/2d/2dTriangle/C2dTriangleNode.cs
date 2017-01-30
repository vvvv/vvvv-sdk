#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

//using TriangleNet.Data;
//using TriangleNet.Log;
using TriangleNet.IO;
//using TriangleNet.Algorithm;
using TriangleNet.Smoothing;
using TriangleNet.Meshing;
using TriangleNet.Geometry;
using TriangleNet.Tools;
using TriangleNet.Topology;


#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Triangle", Category = "2d", Help = "A 2d Quality Mesh Generator and Delaunay Triangulator", Author="digitalWannabe", Credits = "Jonathan Richard Shewchuk//University of California at Berkeley,Christian Woltering,lichterloh",Tags = "Triangle, Mesh, 2d Delaunay, triangulate")]
	#endregion PluginInfo
	public class C2dTriangleNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input ", DefaultValue = 1.0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Vertices", DefaultValue = 1.0, MinValue=0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> Fvert;
		
		[Input("Is Hole", BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<bool>> FIsH;
		
		[Input("Is Segment", DefaultValue = 0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<bool>> FConSeg;
		
		[Input("Boundary Markers", DefaultValue = 1.0, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<int>> FBmark;

        [Input("Use Regions")]
        public ISpread<bool> Freg;

        [Input("Region Indicators ", DefaultValue = 1.0, BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<Vector2D>> FRegInd;

        [Input("Region Markers ", DefaultValue = 1.0, BinVisibility = PinVisibility.OnlyInspector)]
        public ISpread<ISpread<int>> FRegMark;

        [Input("Conforming Delaunay", DefaultValue = 1.0)]
		public ISpread<bool> FConDel;
		
		[Input("Split Segment Settings", DefaultValue = 0.0, MinValue=0.0, MaxValue=2)]
		public ISpread<int> FSplit;
		
		[Input("Minimun Angle", DefaultValue = 15.0)]
		public ISpread<double> FMinAngle;
		
		[Input("Maximum Angle", DefaultValue = 180.0)]
		public ISpread<double> FMaxAngle;
		
		[Input("Maximum Area", DefaultValue = 0.1, BinVisibility = PinVisibility.OnlyInspector)]
		public ISpread<ISpread<double>> FMaxArea;
		
		[Input("Steiner Points", DefaultValue = 0.0, MinValue=0)]
		public ISpread<int> FSteiner;
		
		[Input("Smoothing", DefaultValue = 0.0)]
		public ISpread<bool> FSmooth;
		
		[Input("Smoothing Limit", DefaultValue = 10.0, MinValue =1.0)]
		public ISpread<int> FSmoothLimit;
		
		[Input("Unify", DefaultValue = 0)]
		public ISpread<bool> FUni;
		
		[Input("Shift Bin Indices", DefaultValue = 1, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
		public ISpread<bool> FShift;
		
		[Input("Generate Mesh", DefaultValue = 0.0, IsBang=true)]
		public ISpread<bool> FCal;
	
		[Output("Output ", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<Vector2D>> FOutput;
		
//		[Output("Triangles")]
//		public ISpread<ISpread<ITriangle> FTriangles;
		
		[Output("Indices", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FInd;
		
		[Output("Boundary Markers", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FVmark;

        [Output("Region Markers", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<int>> FRmark;

        //		[Output("Polygon Marker")]
        //		public ISpread<ISpread<int>> FPmark;

//        [Output("Mesh")]
//		public ISpread<IMesh> FMesh;

		[Import()]
		public ILogger FLogger;
		
//		[Output("Mesh")]
//       public IDXMeshOut MOut;
		#endregion fields & pins
		
/*		public void Unify(IMesh mesh, ISpread<Vector2D> UniOut, ISpread<int> Indices, I)
		{
			var vertices = mesh.Vertices;
			int vCount = vertices.Count;
			
		}
*/
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{

			SpreadMax = SpreadUtils.SpreadMax(Fvert,FInput,FIsH,FMaxAngle,FMinAngle,FMaxArea,FConDel,FSplit,FSteiner,FBmark,FSmooth,FSmoothLimit,Freg,FRegInd,FConSeg);
			FOutput.SliceCount = SpreadMax;
			FVmark.SliceCount = SpreadMax;
//			FMesh.SliceCount = SpreadMax;
            FRmark.SliceCount = SpreadMax;
			FInd.SliceCount = SpreadMax;

            int index = 0;			

            if (FCal[0])
            {
            	
            List<int> shift = new List<int>();			
			shift.Add(0);
            	
            
            for (int meshID=0;meshID<SpreadMax;meshID++){
				
	            var polygon = new Polygon();
				
				for (int contourID=0;contourID<Fvert[meshID].SliceCount;contourID++){
					
				int vertexcount =0;
					
				
					
				if (Fvert[meshID][contourID]!=0){
					vertexcount = Fvert[meshID][contourID];
					}
					else{
						vertexcount=FInput[meshID].SliceCount;
					}
				
				ISpread<Vertex> vcsSpread = new Spread<Vertex>();
				vcsSpread.SliceCount=vertexcount;
				
				
				
	            for (int i = 0; i < vertexcount; i++)
	            {          	
	                vcsSpread[i] = new Vertex(FInput[meshID][index+i].x, FInput[meshID][index+i].y, FBmark[meshID][index+i]);
				}
				
				if (vertexcount>1){
						if(FConSeg[meshID][contourID])
							{
							for (int segID=0;segID<vertexcount-1;segID++)
								{
								polygon.Add(new Segment(vcsSpread[segID],vcsSpread[segID+1]), true);
								}
							}
						else
							{
							polygon.Add(new Contour(vcsSpread, contourID+1), FIsH[meshID][contourID]);	
							}					
	            	}
	            	else
					{
						polygon.Add(vcsSpread[0]);
					}
					
					
					
				index+=vertexcount;
	
				}
				
	            var quality = new QualityOptions() { MinimumAngle = FMinAngle[meshID], MaximumAngle = FMaxAngle[meshID], SteinerPoints = FSteiner[meshID] };
	            if (Freg[meshID])
	                {
	                    for (int regionID = 0; regionID < FRegInd[meshID].SliceCount; regionID++)
	                    {
	                        polygon.Regions.Add(new RegionPointer(FRegInd[meshID][regionID].x, FRegInd[meshID][regionID].x, FRegMark[meshID][regionID]));
	                    }
	
					
	                }
				else{
					quality.MaximumArea=FMaxArea[meshID][0];	
					}
				
				
	            // Triangulate the polygon
				var options = new ConstraintOptions() { ConformingDelaunay = FConDel[meshID], SegmentSplitting = FSplit[meshID] };
				if (FSmooth[meshID])options = new ConstraintOptions() { ConformingDelaunay = true, SegmentSplitting = FSplit[meshID] };	
				
				
	            
					
				var mesh = polygon.Triangulate(options, quality);
				
	            if (Freg[meshID])
	                {
	                    foreach (var t in mesh.Triangles)
					    {
					        // Set area constraint for all triangles
					        t.Area = FMaxArea[meshID][t.Label-1];
					    }
					// Use per triangle area constraint for next refinement
	            	quality.VariableArea = true;
	                
	
		            // Refine mesh to meet area constraint.
		            mesh.Refine(quality,FConDel[meshID]);
		            
	
	                }
					
				// Do some smoothing.
	        	if (FSmooth[meshID]) (new SimpleSmoother()).Smooth(mesh, FSmoothLimit[meshID]);
	
				mesh.Renumber();
					
			
	            int triangleCount = mesh.Triangles.Count;
				int SliceOut = triangleCount*3;
				int vertexCount = mesh.Vertices.Count;
					
				
	            
				var vertices = mesh.Vertices.ToSpread();		
				var triangles = mesh.Triangles.ToSpread();
					
				FRmark[meshID] = new Spread<int>();
					
				FInd[meshID].SliceCount = SliceOut;	
					
				if (FUni[meshID]){
					
					shift.Add(shift[meshID]+vertexCount*(Convert.ToInt32(FShift[0])));
					
					FOutput[meshID].SliceCount = vertexCount;
					FVmark[meshID].SliceCount = vertexCount;
		            FRmark[meshID].SliceCount = vertexCount;
		//			FMesh[meshID] = mesh;
						
							
				
		            for (int i = 0; i < triangleCount; i++)
		            	{	     	
		            	for (int tri =0; tri<3;tri++)
		            		{
		            		FInd[meshID][i * 3+tri] = triangles[i].GetVertexID(tri)+shift[meshID];
		            		if (triangles[i].Label>=FRmark[meshID][triangles[i].GetVertexID(tri)]) FRmark[meshID][triangles[i].GetVertexID(tri)]= triangles[i].Label;
		            		
		            		}
		              	}
					
					for (int j = 0; j < vertexCount; j++)
		            { 

		            		FOutput[meshID][j]=new Vector2D(vertices[j].X,vertices[j].Y);
		            		FVmark[meshID][j] = vertices[j].Label;   		           	
		            }	

					
					
					
/*					List<int> OldIndices = new List<int>();
					List<int> NewIndices = new List<int>();
					
					for (int s=0; s<FConSeg[meshID].SliceCount;s++){
						if (FConSeg[meshID][s]){
							int startindex=0;
							
							int first=0;
							for (int u=0; u<s;u++){
								startindex+=Fvert[meshID][u];
							}
							
							//only check for points on segments, excluding start and end points
							
							for (int segPID=1; segPID<Fvert[meshID][s]-1;segPID++){
								int counter=0;
								for (int vertID=0;vertID<vertexCount;vertID++){
									if (vertices[vertID].X==FInput[meshID][startindex+segPID].x && vertices[vertID].Y==FInput[meshID][startindex+segPID].y){
										counter++;
										if (counter>1){
											OldIndices.Add(vertID);
											NewIndices.Add(first);
										}
										else{
											first = vertID;
										}
									}
								}
							}
							
						for (int indexID=0; indexID<OldIndices.Count;indexID++){
							
							FInd[meshID].Select(num => num == OldIndices[indexID] ? NewIndices[indexID] : num);
							}
						}
						}
					*/
					//remove duplicate points which do not belong to any triangle
				
					List<int> GarbageIndex = new List<int>();
					for (int vecID=0; vecID<FOutput[meshID].SliceCount;vecID++){
						bool isPart = false;
						
						isPart = FInd[meshID].Contains(vecID);
						
						if (!isPart) {
							GarbageIndex.Add(vecID);								
						}							
					}
					
					
					GarbageIndex.Reverse();
					for (int vecID=0; vecID<GarbageIndex.Count;vecID++){
						FOutput[meshID].RemoveAt(GarbageIndex[vecID]);
					}
					for (int g = 0; g<FInd[meshID].SliceCount;g++){
							for (int h =0; h<GarbageIndex.Count;h++){
								if (FInd[meshID][g]>=GarbageIndex[h]){
									int r = FInd[meshID][g]-1; 
									FInd[meshID][g] = r<0 ? r+FOutput[meshID].SliceCount : r;
								} //can be -1?
							}							
					}		
				}
				else
					{
						
					shift.Add(shift[meshID]+SliceOut*(Convert.ToInt32(FShift[0])));	
						
					FOutput[meshID].SliceCount = SliceOut;
					FVmark[meshID].SliceCount = SliceOut;
		            FRmark[meshID].SliceCount = SliceOut;
						
					for (int i = 0; i < triangleCount; i++)
		            	{	
		     	
		            	for (int tri =0; tri<3;tri++)
		            		{
		            		FOutput[meshID][i * 3+tri]=new Vector2D(triangles[i].GetVertex(tri).X,triangles[i].GetVertex(tri).Y);
		            		FVmark[meshID][i * 3+tri]=triangles[i].GetVertex(tri).Label;
		            		FRmark[meshID][i * 3+tri]=triangles[i].Label;	
		            		FInd[meshID][i * 3+tri] = i*3+tri+ +shift[meshID];
		            		}	
		              	}						
					}				
			}            
		}
		}
	}



    ////////   REMOVE REGIONS PLUGIN


    #region PluginInfo
    [PluginInfo(Name = "RemoveRegions", Category = "2D", Help = "Remove regions (vertices + indices) by vertex markers", Author = "digitalWannabe", Credits = "lichterloh", Tags = "Triangle, Mesh, Region, dope")]
    #endregion PluginInfo
    public class C2DRemoveRegionsNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input ")]
        public IDiffSpread<Vector2D> FInputVertices;

        [Input("Triangle Indices")]
        public IDiffSpread<int> FTriangleIndices;

        [Input("Region Markers")]
        public IDiffSpread<int> FRegionMarkers;

        [Input("Remove Regions")]
        public IDiffSpread<int> FRemoveRegion;


        [Output("Output ")]
        public ISpread<Vector2D> FOutputVertices;

        [Output("Triangle Indices")]
        public ISpread<int> FTriangleIndicesOut;

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

            if (FInputVertices.IsChanged || FTriangleIndices.IsChanged || FRemoveRegion.IsChanged || FRegionMarkers.IsChanged)
            {

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

                FSelect.SliceCount = FRegionMarkers.SliceCount;
                for (int i = 0; i < FRegionMarkers.SliceCount; i++)
                {

                    FSelect[i] = !deleteIndex.Contains(i);

                }

                List<int> NewIndices = new List<int>();

                for (int g = 0; g < FTriangleIndices.SliceCount / 3; g++)
                {

                    if (!(deleteIndex.Contains(FTriangleIndices[g * 3]) || deleteIndex.Contains(FTriangleIndices[g * 3 + 1]) || deleteIndex.Contains(FTriangleIndices[g * 3 + 2])))
                    {
                        NewIndices.Add(FTriangleIndices[g * 3]);
                        NewIndices.Add(FTriangleIndices[g * 3 + 1]);
                        NewIndices.Add(FTriangleIndices[g * 3 + 2]);
                    }

                }


                FTriangleIndicesOut.SliceCount = NewIndices.Count;
                FTriangleIndicesOut.AssignFrom(NewIndices);

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

                for (int vecID = 0; vecID < deleteIndex.Count; vecID++)
                {
                    FOutputVertices.RemoveAt(deleteIndex[vecID]);
                }


                //FLogger.Log(LogType.Debug, "hi tty!");
            }
        }

    }
	
}
/*
    #region PluginInfo
    [PluginInfo(Name = "Refine", Category = "2d", Help = "Refine a triangulated Mesh", Author = "digitalWannabe", Credits = "Jonathan Richard Shewchuk//University of California at Berkeley,Christian Woltering,lichterloh", Tags = "Triangle, Mesh, 2d Delaunay, triangulate")]
    #endregion PluginInfo
    public class C2dRefineNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input ", DefaultValue = 1.0, BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<Vector2D>> FInput;
    	
    	[Input("Mesh")]
        public ISpread<IMesh> FMesh;

//       [Input("Vertices", DefaultValue = 1.0, MinValue = 0, BinVisibility = PinVisibility.Hidden)]
 //       public ISpread<ISpread<int>> Fvert;

//        [Input("Indices", BinVisibility = PinVisibility.Hidden)]
//        public ISpread<ISpread<int>> FID;

//        [Input("Is Hole", BinVisibility = PinVisibility.Hidden)]
//        public ISpread<ISpread<bool>> FIsH;

//        [Input("Boundary Marker", DefaultValue = 1.0, BinVisibility = PinVisibility.Hidden)]
//        public ISpread<ISpread<int>> FBmark;

        [Input("Conforming Delaunay", DefaultValue = 1.0)]
        public ISpread<bool> FConDel;

//        [Input("Split Segment Settings", DefaultValue = 0.0, MinValue = 0.0, MaxValue = 2)]
 //       public ISpread<int> FSplit;

        [Input("Minimun Angle", DefaultValue = 15.0)]
        public ISpread<double> FMinAngle;

        [Input("Maximum Angle", DefaultValue = 180.0)]
        public ISpread<double> FMaxAngle;

        [Input("Maximum Area", DefaultValue = 0.3)]
        public ISpread<double> FMaxArea;

        [Input("Steiner Points", DefaultValue = 0.0, MinValue = 0)]
        public ISpread<int> FSteiner;

//        [Input("Smoothing", DefaultValue = 0.0)]
//        public ISpread<bool> FSmooth;

//        [Input("Smoothing Limit", DefaultValue = 10.0, MinValue = 1.0)]
//        public ISpread<int> FSmoothLimit;

        [Input("Generate Mesh", DefaultValue = 0.0, IsBang = true)]
        public ISpread<bool> FCal;

        [Output("Output ", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<Vector2D>> FOutput;

        //		[Output("Triangles")]
        //		public ISpread<ISpread<ITriangle> FTriangles;

        [Output("Boundary Marker", BinVisibility = PinVisibility.Hidden)]
        public ISpread<ISpread<int>> FVmark;

        //		[Output("Polygon Marker")]
        //		public ISpread<ISpread<int>> FPmark;	

        [Import()]
        public ILogger FLogger;

        //		[Output("Mesh")]
        //       public IDXMeshOut MOut;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {

            SpreadMax = SpreadUtils.SpreadMax(FInput,FMaxAngle, FMinAngle, FMesh, FMaxArea, FConDel,FSteiner);
            FOutput.SliceCount = SpreadMax;
            FVmark.SliceCount = SpreadMax;


            int index = 0;


            for (int meshID = 0; meshID < SpreadMax; meshID++)
            {
//                var mesh = new InputGeometry();
                var polygon = new Polygon();
                //                List<ITriangle> triangles;
                
                //InputGeometry geometry;\
//                polygon.
                

/*                for (int contourID = 0; contourID < Fvert[meshID].SliceCount; contourID++)
                {

                    int vertexcount = 0;

                    if (Fvert[meshID][contourID] != 0)
                    {
                        vertexcount = Fvert[meshID][contourID];
                    }
                    else {
                        vertexcount = FInput[meshID].SliceCount;
                    }

                    ISpread<Vertex> vcsSpread = new Spread<Vertex>();
                    vcsSpread.SliceCount = vertexcount;

                    for (int i = 0; i < vertexcount; i++)
                    {
                        vcsSpread[i] = new Vertex(FInput[meshID][index + i].x, FInput[meshID][index + i].y, FBmark[meshID][index + i]);
                    }

                    index += vertexcount;

                    polygon.Add(new Contour(vcsSpread, contourID + 1), FIsH[meshID][contourID]);
                }


                // Triangulate the polygon
                var options = new ConstraintOptions() { ConformingDelaunay = FConDel[meshID], SegmentSplitting = FSplit[meshID] };
                if (FSmooth[meshID]) options = new ConstraintOptions() { ConformingDelaunay = true, SegmentSplitting = FSplit[meshID] };

                var quality = new QualityOptions() { MinimumAngle = FMinAngle[meshID], MaximumAngle = FMaxAngle[meshID], MaximumArea = FMaxArea[meshID], SteinerPoints = FSteiner[meshID] };

                if (FCal[0])
                {
//                    var mesh = polygon.Triangulate(options, quality);
//                    IList<Point> Holes;
//                    ICollection<Triangle> Triangles;
//                    ICollection<Vertex> Vertices= new ICollection<Vertex>();
 //*                   int InTriangleCount = FID.SliceCount/3;
/*                    for (int m = 0; m < InTriangleCount; m++)
                    {

                    }
                	
               
                    int InVertexCount = FInput.SliceCount;
//                    TriangleNet.IO.IMeshFormat
                    var config = new TriangleNet.Configuration();
//                	var mesh = new TriangleNet.Mesh(config);
//                	mesh.Triangles=t;
                    var mesh = FMesh[meshID];
                	var vertixes = mesh.Vertices;
 //                   mesh.Holes.Add(Holes);
                	for (int m = 0; m < InVertexCount; m++)
                    {
                        Vertex v = new Vertex();
                    	v.X=FInput[meshID][m].x;
                    	v.Y=FInput[meshID][m].y;
                    	vertixes.Add(v);
 //                       mesh.Vertices.
                    	
                      
                    }
                	
					mesh.Vertices.Concat(vertixes);
 /*               	
                	for (int m = 0; m < InVertexCount/3; m++)
                    {
                        InputTriangle t = new InputTriangle(m*3,(m*3)+1,(m*3)+2);
                    	mesh.Triangles.Add(t);
                                      
                    }
                	
                	
                	

//                    Triangle t = new Triangle();
					


                    mesh.Refine(quality, FConDel[meshID]);
                    //			mesh.Renumber();
                    // Do some smoothing.

                 // if (FSmooth[meshID]) (new SimpleSmoother()).Smooth(mesh, FSmoothLimit[meshID]);

 //                   mesh.Refine(quality);

                    int triangleCount = mesh.Triangles.Count;

                    int SliceOut = triangleCount * 3;
                    FOutput[meshID].SliceCount = SliceOut;
                    FVmark[meshID].SliceCount = SliceOut;
                    var vertices = mesh.Vertices.ToSpread();
                    var triangles = mesh.Triangles.ToSpread();



                    for (int i = 0; i < triangleCount; i++)
                    {
                        FOutput[meshID][i * 3] = new Vector2D(triangles[i].GetVertex(0).X, triangles[i].GetVertex(0).Y);
                        FOutput[meshID][i * 3 + 1] = new Vector2D(triangles[i].GetVertex(1).X, triangles[i].GetVertex(1).Y);
                        FOutput[meshID][i * 3 + 2] = new Vector2D(triangles[i].GetVertex(2).X, triangles[i].GetVertex(2).Y);
                        FVmark[meshID][i * 3] = (int)triangles[i].GetVertex(0).Label;
                        FVmark[meshID][i * 3 + 1] = (int)triangles[i].GetVertex(1).Label;
                        FVmark[meshID][i * 3 + 2] = (int)triangles[i].GetVertex(2).Label;
                    }
                }

            }
        }
    }
}
*/