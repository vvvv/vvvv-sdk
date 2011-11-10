using System;
using System.Collections.Generic;
using System.Text;

namespace GridTransform
{
    class GridTransformer
    {
        #region Fields

        private List<Triangle> FGridFrom, FGridTo;

        #endregion Fields

        #region Methods

        public GridTransformer()
        {
            FGridFrom = new List<Triangle>();
            FGridTo = new List<Triangle>();
        }

        public void Insert(Triangle from, Triangle to)
        {
            FGridFrom.Add(from);
            FGridTo.Add(to);
        }

        public bool Transform(Point2D pIn, out Point2D pOut)
        {
            int triCount = 0;

            foreach (Triangle triangle in FGridFrom)
                if (triangle.Transform(pIn, FGridTo[triCount++], out pOut))
                    return true;

            pOut = new Point2D(0, 0);
            return false;
        }
        /*
        public Point2D Transform(Point2D pIn)
        {
            Point2D pOut;
            int triCount = 0;

            foreach (Triangle triangle in FGridFrom)
                if (triangle.Transform(pIn, FGridTo[triCount++], out pOut))
                    return pOut;

            return new Point2D(0, 0);
        }
        */

        #endregion Methods
    }
}
