using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes
{
    [PluginInfo(
		Name = "Length", 
		Category = "2d",
		Version = "Vector Spectral",
		Help = "Calculates the length of a path consisting of a spread of 2d points", 
		Tags = "",
		Author = "motzi"
	)]
    public class Vector2dLengthSpectralNode : LengthSpectral<Vector2D>
    {
        override protected double Distance(Vector2D t1, Vector2D t2)
        {
            return VMath.Dist(t1, t2);
        }
    }

    [PluginInfo(
        Name = "Length",
        Category = "3d",
        Version = "Vector Spectral",
        Help = "Calculates the length of a path consisting of a spread of 3d points",
        Tags = "",
        Author = "motzi"
    )]
    public class Vector3dLengthSpectralNode : LengthSpectral<Vector3D>
    {
        override protected double Distance(Vector3D t1, Vector3D t2)
        {
            return VMath.Dist(t1, t2);
        }
    }
}