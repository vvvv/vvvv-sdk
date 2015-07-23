using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Assimp.Lib;
using SlimDX;

namespace VVVV.Assimp.Nodes
{
    [PluginInfo(Name = "Camera", Category = "Assimp", Version = "Transform DX9", Author = "vux, flateric")]
    public class AssimpCameraTransformNode : IPluginEvaluate
    {
        [Input("Camera")]
        Pin<AssimpCamera> FInCameras;

        [Output("Name")]
        ISpread<string> FOutName;

        [Output("View")]
        ISpread<Matrix> FOutView;

        [Output("Projection")]
        ISpread<Matrix> FOutProj;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInCameras.PluginIO.IsConnected)
            {
                if (this.FInCameras.IsChanged)
                {
                    int camcount = this.FInCameras.SliceCount;

                    this.FOutView.SliceCount = camcount;
                    this.FOutProj.SliceCount = camcount;
                    this.FOutName.SliceCount = camcount;

                    for (int i = 0; i < camcount; i++)
                    {
                        AssimpCamera cam = this.FInCameras[i];

                        Matrix proj = new Matrix();

                        //Mini near plane fix: make sure tiny bit > 0
                        float znear = cam.NearPlane <= 0.0f ? 0.0001f : cam.NearPlane;

                        Matrix.PerspectiveFovLH(cam.HFOV, cam.AspectRatio == 0 ? 1 : cam.AspectRatio, znear, cam.FarPlane, out proj);
                        Matrix view = Matrix.LookAtLH(cam.Position, cam.LookAt, cam.UpVector);

                        this.FOutView[i] = view;
                        this.FOutProj[i] = proj;
                        this.FOutName[i] = cam.Name;
                    }
                }
            }
            else
            {
                this.FOutName.SliceCount = 0;
                this.FOutView.SliceCount = 0;
                this.FOutProj.SliceCount = 0;
            }
        }

    }
}
