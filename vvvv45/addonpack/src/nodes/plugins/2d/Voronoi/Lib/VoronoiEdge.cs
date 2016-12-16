using System;
using System.Collections.Generic;
using System.Text;

namespace BenTools.Mathematics
{
    public class VoronoiEdge
    {
        internal bool Done = false;
        public Vector RightData, LeftData;
        public Vector VVertexA = Fortune.VVUnkown, VVertexB = Fortune.VVUnkown;
        public void AddVertex(Vector V)
        {
            if (VVertexA == Fortune.VVUnkown)
                VVertexA = V;
            else if (VVertexB == Fortune.VVUnkown)
                VVertexB = V;
            else throw new Exception("Tried to add third vertex!");
        }
        public bool IsInfinite
        {
            get { return VVertexA == Fortune.VVInfinite && VVertexB == Fortune.VVInfinite; }
        }
        public bool IsPartlyInfinite
        {
            get { return VVertexA == Fortune.VVInfinite || VVertexB == Fortune.VVInfinite; }
        }
        public Vector FixedPoint
        {
            get
            {
                if (IsInfinite)
                    return 0.5 * (LeftData + RightData);
                if (VVertexA != Fortune.VVInfinite)
                    return VVertexA;
                return VVertexB;
            }
        }
        public Vector DirectionVector
        {
            get
            {
                if (!IsPartlyInfinite)
                    return (VVertexB - VVertexA) * (1.0 / Math.Sqrt(Vector.Dist(VVertexA, VVertexB)));
                if (LeftData[0] == RightData[0])
                {
                    if (LeftData[1] < RightData[1])
                        return new Vector(-1, 0);
                    return new Vector(1, 0);
                }
                Vector Erg = new Vector(-(RightData[1] - LeftData[1]) / (RightData[0] - LeftData[0]), 1);
                if (RightData[0] < LeftData[0])
                    Erg.Multiply(-1);
                Erg.Multiply(1.0 / Math.Sqrt(Erg.SquaredLength));
                return Erg;
            }
        }
        public double Length
        {
            get
            {
                if (IsPartlyInfinite)
                    return double.PositiveInfinity;
                return Math.Sqrt(Vector.Dist(VVertexA, VVertexB));
            }
        }
    }
}
