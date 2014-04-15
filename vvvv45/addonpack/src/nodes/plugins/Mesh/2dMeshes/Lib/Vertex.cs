using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SlimDX.Direct3D9;
using SlimDX;

namespace VVVV.Lib
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 pv;
        public Vector3 nv;
        public float tu1;
        public float tv1;

        public static readonly VertexFormat Format = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
        public static readonly int SizeInByte = 8 * sizeof(float);

    }
}
