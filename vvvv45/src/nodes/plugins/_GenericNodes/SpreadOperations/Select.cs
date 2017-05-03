using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.Utils.Streams;
using VVVV.Nodes.Generic;

namespace VVVV.Nodes
{
  
    [PluginInfo(Name = "Select",
                Category = "Value",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
	            Tags = "repeat, resample, duplicate, spreadop")]
    public class ValueSelectNode : Select<double> { }

    [PluginInfo(Name = "Select",
            Category = "2d",
            Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
            Tags = "repeat, resample, duplicate, spreadop")]
    public class Vecto2DSelectNode : Select<Vector2D> { }

    [PluginInfo(Name = "Select",
            Category = "3d",
            Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
            Tags = "repeat, resample, duplicate, spreadop")]
    public class Vector3DSelectNode : Select<Vector3D> { }

    [PluginInfo(Name = "Select",
            Category = "4d",
            Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
            Tags = "repeat, resample, duplicate, spreadop")]
    public class Vector4DSelectNode : Select<Vector4D> { }

    [PluginInfo(Name = "Select",
                Category = "Transform",
                Help = "Returns each slice of the Input spread as often as specified by the corresponding Select slice. 0 meaning the slice will be omitted.",
                Tags = "repeat, resample, duplicate, spreadop")]
    public class TransformSelectNode : Select<Matrix4x4> { }
}
