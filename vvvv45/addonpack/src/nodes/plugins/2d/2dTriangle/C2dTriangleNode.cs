#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using TriangleNet.Geometry;
//using TriangleNet.
using TriangleNet.Meshing;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Triangle", Category = "2d", Help = "A 2d Quality Mesh Generator and Delaunay Triangulator", Author="digitalWannabe", Credits = "Jonathan Richard Shewchuk//University of California at Berkeley,Christian Woltering,lichterloh",Tags = "Triangle, Mesh, 2d Delaunay, triangulate")]
	#endregion PluginInfo
	public class C2dTriangleNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input ", DefaultValue = 1.0, BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<Vector2D>> FInput;
		
		[Input("Vertices", DefaultValue = 0.0, MinValue=0, BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> Fvert;
		
		[Input("Is Hole", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<bool>> FIsH;
		
		[Input("Conforming Delaunay", DefaultValue = 1.0)]
		public ISpread<bool> FConDel;
		
		[Input("Split Segment Settings", DefaultValue = 0.0, MinValue=0.0, MaxValue=2)]
		public ISpread<int> FSplit;
		
		[Input("Minimun Angle", DefaultValue = 15.0)]
		public ISpread<double> FMinAngle;
		
		[Input("Maximum Angle", DefaultValue = 180.0)]
		public ISpread<double> FMaxAngle;
		
		[Input("Maximum Area", DefaultValue = 0.3)]
		public ISpread<double> FMaxArea;
		
		[Input("Steiner Points", DefaultValue = 0.0, MinValue=0)]
		public ISpread<int> FSteiner;
		
		[Input("Generate Mesh", DefaultValue = 0.0, IsBang=true)]
		public ISpread<bool> FCal;
	

		[Output("Output ", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<Vector2D>> FOutput;
		
//		[Output("Index")]
//		public ISpread<ISpread<int> FIndices;
		
//		[Output("Triangles")]
//		public ISpread<ISpread<ITriangle> FTriangles;
		
		[Output("Vertex Type", BinVisibility = PinVisibility.Hidden)]
		public ISpread<ISpread<int>> FVType;
		

		[Import()]
		public ILogger FLogger;
		
//		[Output("Mesh")]
//       public IDXMeshOut MOut;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
//			FOutput.SliceCount = SpreadMax;
			SpreadMax = SpreadUtils.SpreadMax(Fvert,FInput,FIsH,FMaxAngle,FMinAngle,FMaxArea,FConDel,FSplit,FSteiner);
			FOutput.SliceCount = SpreadMax;
			FVType.SliceCount = SpreadMax;

            int boundaryMarker = 1;
			int index = 0;
			//, SegmentSplitting = FSplit[0] };
			
			for (int meshID=0;meshID<SpreadMax;meshID++){
			
			
            var polygon = new Polygon();
//            var vcs = new Vertex[SpreadMax];
			
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
                vcsSpread[i] = new Vertex(FInput[meshID][index+i].x, FInput[meshID][index+i].y, boundaryMarker);
            }
				
			index+=vertexcount;
            //if changed!!!
            // Add the outer box contour with boundary marker 1.
            polygon.Add(new Contour(vcsSpread, boundaryMarker), FIsH[meshID][contourID]);

      //      options.ConformingDelaunay = FConDel[0];//new ConstraintOptions() { ConformingDelaunay = , SegmentSplitting = FSplit[0] };
    //        quality = new QualityOptions() { MinimumAngle = FMinAngle[0], MaximumAngle = FMaxAngle[0], MaximumArea = FMaxArea[0], SteinerPoints = FSteiner[0] };
			}
            // Triangulate the polygon
			var options = new ConstraintOptions() { ConformingDelaunay = FConDel[meshID], SegmentSplitting = FSplit[meshID] };
			var quality = new QualityOptions() { MinimumAngle = FMinAngle[meshID], MaximumAngle = FMaxAngle[meshID], MaximumArea = FMaxArea[meshID], SteinerPoints = FSteiner[meshID] };
            
			if (FCal[0]){	
			var mesh = polygon.Triangulate(options, quality);
//			mesh.Refine(quality,options);
			
			
            int SliceOut = mesh.Triangles.Count;
            FOutput[meshID].SliceCount = SliceOut*3;
//			FIndices.SliceCount = SliceOut*3;
			FVType[meshID].SliceCount = SliceOut*3;
            var vertices = mesh.Vertices.ToSpread();
			
			var triangles = mesh.Triangles.ToSpread();
		//	vertices[0].
			
			
			
//			FOutput = vertices.ToSpread();
			
            for (int i = 0; i < SliceOut; i++)
            {
            	FOutput[meshID][i*3] = new Vector2D(triangles[i].GetVertex(0).X,triangles[i].GetVertex(0).Y);
            	FOutput[meshID][i*3+1] = new Vector2D(triangles[i].GetVertex(1).X,triangles[i].GetVertex(1).Y);
            	FOutput[meshID][i*3+2] = new Vector2D(triangles[i].GetVertex(2).X,triangles[i].GetVertex(2).Y);
//            	Vector2D vec = new Vector2D(vertices[i].X,vertices[i].Y);
//                FOutput[i] = vec;
//            	FIndices[i*3] =   3*i+triangles[i].GetVertexID(0);
//            	FIndices[i*3+1] = 3*i+triangles[i].GetVertexID(1);
//            	FIndices[i*3+2] = 3*i+triangles[i].GetVertexID(2);
            	FVType[meshID][i*3] = (int)triangles[i].GetVertex(0).Type;
            	FVType[meshID][i*3+1] = (int)triangles[i].GetVertex(1).Type;
            	FVType[meshID][i*3+2] = (int)triangles[i].GetVertex(2).Type;
            }
			}
            
		}
		}
	}
}
