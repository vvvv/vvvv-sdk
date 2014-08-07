using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    [PluginInfo(Name="NearestNeighbour", Category="Value", Version="Self", Author="DominikKoller")]
    public class NearestNeighbourSelfValueNode : NearestNeighbourSelf<double>
    {
        protected override double Distance(double t1, double t2)
        {
            return Math.Abs(t1 - t2);
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "2d", Version = "Self", Author = "DominikKoller")]
    public class NearestNeighbourSelf2dNode : NearestNeighbourSelf<Vector2D>
    {
        protected override double Distance(Vector2D t1, Vector2D t2)
        {
            Vector2D diff = t1 - t2;
            return diff.Length;
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "3d", Version = "Self", Author = "DominikKoller")]
    public class NearestNeighbourSelf3dNode : NearestNeighbourSelf<Vector3D>
    {
        protected override double Distance(Vector3D t1, Vector3D t2)
        {
            Vector3D diff = t1 - t2;
            return diff.Length;
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "4d", Version = "Self", Author = "DominikKoller")]
    public class NearestNeighbourSelf4dNode : NearestNeighbourSelf<Vector4D>
    {
        protected override double Distance(Vector4D t1, Vector4D t2)
        {
            Vector4D diff = t1 - t2;
            return diff.Length;
        }
    }


}
