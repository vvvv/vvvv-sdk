#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;

using TriangleNet.Geometry;
using TriangleNet.Meshing;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "PolyTriangle", Category = "2d", Help = "Basic template with one value in/out", Tags = "")]
	#endregion PluginInfo
	public class C2dPolyTriangleNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input", DefaultValue = 1.0)]
		public ISpread<Vector2D> FInput;

		[Output("Output")]
		public ISpread<Vector2D> FOutput;
		
		[Output("Index")]
		public ISpread<int> FIndices;
		
		[Output("Triangles")]
		public ISpread<ITriangle> FTriangles;
		
		[Output("Vertex Type")]
		public ISpread<int> FVType;
		

		[Import()]
		public ILogger FLogger;
		
		[Output("Mesh")]
        public IDXMeshOut MOut;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
//			FOutput.SliceCount = SpreadMax;

            int boundaryMarker = 1;

            var polygon = new Polygon();
//            var vcs = new Vertex[SpreadMax];
			
			ISpread<Vertex> vcsSpread = new Spread<Vertex>();
			vcsSpread.SliceCount=SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
            {
                vcsSpread[i] = new Vertex(FInput[i].x, FInput[i].y, boundaryMarker);
            }
            //if changed!!!
            // Add the outer box contour with boundary marker 1.
            polygon.Add(new Contour(vcsSpread, boundaryMarker));

            var options = new ConstraintOptions() { ConformingDelaunay = false };
            var quality = new QualityOptions() { MinimumAngle = 15 };

            // Triangulate the polygon
            var mesh = polygon.Triangulate(options, quality);
//			mesh.Refine(quality,options);
			
			
            int SliceOut = mesh.Triangles.Count;
            FOutput.SliceCount = SliceOut*3;
			FIndices.SliceCount = SliceOut*3;
			FVType.SliceCount = SliceOut*3;
            var vertices = mesh.Vertices.ToSpread();
			
			var triangles = mesh.Triangles.ToSpread();
		//	vertices[0].
			
			
			
//			FOutput = vertices.ToSpread();
			
            for (int i = 0; i < SliceOut; i++)
            {
            	FOutput[i*3] = new Vector2D(triangles[i].GetVertex(0).X,triangles[i].GetVertex(0).Y);
            	FOutput[i*3+1] = new Vector2D(triangles[i].GetVertex(1).X,triangles[i].GetVertex(1).Y);
            	FOutput[i*3+2] = new Vector2D(triangles[i].GetVertex(2).X,triangles[i].GetVertex(2).Y);
//            	Vector2D vec = new Vector2D(vertices[i].X,vertices[i].Y);
//                FOutput[i] = vec;
            	FIndices[i*3] =   3*i+triangles[i].GetVertexID(0);
            	FIndices[i*3+1] = 3*i+triangles[i].GetVertexID(1);
            	FIndices[i*3+2] = 3*i+triangles[i].GetVertexID(2);
            	FVType[i*3] = (int)triangles[i].GetVertex(0).Type;
            	FVType[i*3+1] = (int)triangles[i].GetVertex(1).Type;
            	FVType[i*3+2] = (int)triangles[i].GetVertex(2).Type;
            }

            
//            var m = new IDXMeshOut
//				FOutput[i] = FInput[i] * 2;

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
}
