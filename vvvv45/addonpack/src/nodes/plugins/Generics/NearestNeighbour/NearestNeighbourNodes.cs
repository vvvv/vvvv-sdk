using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    [PluginInfo(Name="NearestNeighbour", Category="Value", Author="vux")]
    public class NearestNeighbourValueNode : BaseNearestNeighbourNode<double>
    {
        protected override double Distance(double t1, double t2)
        {
            return Math.Abs(t1 - t2);
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "2d", Author = "vux")]
    public class NearestNeighbour2dNode : BaseNearestNeighbourNode<Vector2D>
    {
        protected override double Distance(Vector2D t1, Vector2D t2)
        {
            Vector2D diff = t1 - t2;
            return diff.Length;
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "3d", Author = "vux")]
    public class NearestNeighbour3dNode : BaseNearestNeighbourNode<Vector3D>
    {
        protected override double Distance(Vector3D t1, Vector3D t2)
        {
            Vector3D diff = t1 - t2;
            return diff.Length;
        }
    }

    [PluginInfo(Name = "NearestNeighbour", Category = "4d", Author = "vux")]
    public class NearestNeighbour4dNode : BaseNearestNeighbourNode<Vector4D>
    {
        protected override double Distance(Vector4D t1, Vector4D t2)
        {
            Vector4D diff = t1 - t2;
            return diff.Length;
        }
    }


}
