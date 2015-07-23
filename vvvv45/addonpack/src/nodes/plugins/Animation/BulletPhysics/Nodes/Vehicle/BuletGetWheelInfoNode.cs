using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;

namespace VVVV.Bullet.Nodes.Vehicle
{
    [PluginInfo(Name = "WheelInfo", Category = "Bullet", Version = "Vehicle DX9", Author = "vux",
    Help = "Drives Bullet Vehicle", AutoEvaluate = true)]
    public class BuletGetWheelInfoNode : IPluginEvaluate
    {
        [Input("Vehicle")]
        Pin<RaycastVehicle> FInVehicle;

        [Output("Transform")]
        ISpread<Matrix4x4> FOutTransform;
		float skidinfos = 0.1f;//1.0f;
		
		[Output("skidinfo")]
        protected ISpread<float> Fskidinfo;
        
        [Output("VehicleSpeed")]
        protected ISpread<float> FvSpeed;
		
        public void Evaluate(int SpreadMax)
        {

            if (FInVehicle.PluginIO.IsConnected)
            {
                FOutTransform.SliceCount = this.FInVehicle[0].NumWheels;
				Fskidinfo.SliceCount = this.FInVehicle[0].NumWheels;
                RaycastVehicle v = this.FInVehicle[0];

                for (int i = 0; i < v.NumWheels; i++)
                {
                    WheelInfo wi = v.GetWheelInfo(i);
                    skidinfos = wi.SkidInfo;
                    Fskidinfo[i] = skidinfos;
                    
                    Matrix m = wi.WorldTransform;

                    Matrix4x4 mn = new Matrix4x4(m.M11, m.M12, m.M13, m.M14,
								m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34,
								m.M41, m.M42, m.M43, m.M44);
                    FvSpeed[0] = v.CurrentSpeedKmHour;

                    this.FOutTransform[i] = mn;
                }
            }
        }
    }
}
