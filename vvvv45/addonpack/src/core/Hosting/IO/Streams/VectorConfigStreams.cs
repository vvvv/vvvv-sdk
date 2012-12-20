using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V1;
using SlimDX;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO.Streams
{
    class Vector2ConfigStream : PluginConfigStream<Vector2>
    {
        private readonly IValueConfig FValueConfig;

        public Vector2ConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector2 GetSlice(int index)
        {
            double x,y;
            FValueConfig.GetValue2D(index,out x,out y);
            return new Vector2((float)x,(float)y);
        }

        protected override void SetSlice(int index, Vector2 value)
        {
            FValueConfig.SetValue2D(index,value.X,value.Y);
        }
    }

    class Vector3ConfigStream : PluginConfigStream<Vector3>
    {
        private readonly IValueConfig FValueConfig;

        public Vector3ConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector3 GetSlice(int index)
        {
            double x, y, z;
            FValueConfig.GetValue3D(index, out x, out y, out z);
            return new Vector3((float)x, (float)y,(float)z);
        }

        protected override void SetSlice(int index, Vector3 value)
        {
            FValueConfig.SetValue3D(index, value.X, value.Y, value.Z);
        }
    }

    class Vector4ConfigStream : PluginConfigStream<Vector4>
    {
        private readonly IValueConfig FValueConfig;

        public Vector4ConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector4 GetSlice(int index)
        {
            double x, y, z,w;
            FValueConfig.GetValue4D(index, out x, out y, out z, out w);
            return new Vector4((float)x, (float)y, (float)z,(float)w);
        }

        protected override void SetSlice(int index, Vector4 value)
        {
            FValueConfig.SetValue4D(index, value.X, value.Y, value.Z, value.W);
        }
    }

    class QuaternionConfigStream : PluginConfigStream<Quaternion>
    {
        private readonly IValueConfig FValueConfig;

        public QuaternionConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Quaternion GetSlice(int index)
        {
            double x, y, z, w;
            FValueConfig.GetValue4D(index, out x, out y, out z, out w);
            return new Quaternion((float)x, (float)y, (float)z, (float)w);
        }

        protected override void SetSlice(int index, Quaternion value)
        {
            FValueConfig.SetValue4D(index, value.X, value.Y, value.Z, value.W);
        }
    }

    class Vector2DConfigStream : PluginConfigStream<Vector2D>
    {
        private readonly IValueConfig FValueConfig;

        public Vector2DConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector2D GetSlice(int index)
        {
            double x, y;
            FValueConfig.GetValue2D(index, out x, out y);
            return new Vector2D(x, y);
        }

        protected override void SetSlice(int index, Vector2D value)
        {
            FValueConfig.SetValue2D(index, value.x, value.y);
        }
    }

    class Vector3DConfigStream : PluginConfigStream<Vector3D>
    {
        private readonly IValueConfig FValueConfig;

        public Vector3DConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector3D GetSlice(int index)
        {
            double x, y, z;
            FValueConfig.GetValue3D(index, out x, out y, out z);
            return new Vector3D(x, y, z);
        }

        protected override void SetSlice(int index, Vector3D value)
        {
            FValueConfig.SetValue3D(index, value.x, value.y, value.z);
        }
    }

    class Vector4DConfigStream : PluginConfigStream<Vector4D>
    {
        private readonly IValueConfig FValueConfig;

        public Vector4DConfigStream(IValueConfig valueConfig)
            : base(valueConfig)
        {
            FValueConfig = valueConfig;
        }

        protected override Vector4D GetSlice(int index)
        {
            double x, y, z, w;
            FValueConfig.GetValue4D(index, out x, out y, out z, out w);
            return new Vector4D(x, y, z, w);
        }

        protected override void SetSlice(int index, Vector4D value)
        {
            FValueConfig.SetValue4D(index, value.x, value.y, value.z, value.w);
        }
    }
}
