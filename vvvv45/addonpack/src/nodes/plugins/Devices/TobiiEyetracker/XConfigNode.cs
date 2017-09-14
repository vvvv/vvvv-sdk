// standard values of eyetracker device
// 172.68.195.1
// Portnumber 4455
// syncport 5547

#region usings

using System;
using System.ComponentModel.Composition;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using Tobii.Eyetracking.Sdk;
using Tobii.Eyetracking.Sdk.Exceptions;

#endregion usings


namespace TobiiEyetracker
{
    #region PluginInfo
    [PluginInfo(Name = "XConfig", Category = "Devices", Version = "TobiiEyetracker", Help = "Tobii Eyetracker XConfiguration Node", Tags = "", Author = "niggos, phlegma")]
    #endregion PluginInfo


    public class XConfigNode : IPluginEvaluate
    {
        #region fields & pins

        public enum Units
        {
            cm,
            inch
        }

        [Input("Units", DefaultEnumEntry = "cm")]
        IDiffSpread<Units> FUnit;

        [Input("Eyetracker Degree", StepSize = 0.1, IsSingle = true)]
        IDiffSpread<double> FEyetrackerDegree;

        [Input("Display Degree", IsSingle = true)]
        IDiffSpread<double> FDisplayDegree;

        [Input("Display Height", IsSingle = true)]
        IDiffSpread<double> FDisplayHeight;

        [Input("Diplay Width", IsSingle = true)]
        IDiffSpread<double> FDisplayWidth;

        [Input("Eyetracker Offset", IsSingle = true)]
        IDiffSpread<double> FEyetrackerOffset;

        [Input("Display Offset", IsSingle = true)]
        IDiffSpread<double> FDisplayOffset;

        [Input("Eyetracker Side Offset", IsSingle = true)]
        IDiffSpread<double> FEyetrackerSideOffset;

        [Input("Eyetracker Rotation", IsSingle = true)]
        IDiffSpread<double> FEyetrackerRotation;

        [Input("PlaneNormal Rotation")]
        IDiffSpread<double> FPlaneNormalRotate;

        [Input("Compensate Init Offset", IsSingle = true)]
        IDiffSpread<bool> FUseInitOffset;

        [Output("Output")]
        ISpread<string> FXconfig;

        [Output("LowerLeft")]
        ISpread<Vector3D> FLowerLeft;

        [Output("UpperLeft")]
        ISpread<Vector3D> FUpperLeft;

        [Output("UpperRight")]
        ISpread<Vector3D> FUpperRight;

        [Output("XConfiguration")]
        ISpread<XConfiguration> FXConfiguration;

        [Import()]
        ILogger FLogger;

        private IEyetracker FEyetracker;
        private const double CDegToRad = 0.0174532925199432957692;

        // TODO: make this offset definable by input vector
        private Vector3D initOffset = new Vector3D(0, -2.6650, -1.2556);



        #endregion fields & pins

        // called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FUnit.IsChanged || FEyetrackerDegree.IsChanged || FDisplayDegree.IsChanged || FDisplayHeight.IsChanged || FDisplayWidth.IsChanged || FEyetrackerOffset.IsChanged || FDisplayOffset.IsChanged || FEyetrackerSideOffset.IsChanged || FEyetrackerRotation.IsChanged || FPlaneNormalRotate.IsChanged || FUseInitOffset.IsChanged)
            {
                StringBuilder XConfig = new StringBuilder();
                XConfig.Append("XDATA");
                XConfig.Append(Environment.NewLine);

                XConfig.Append("1");
                XConfig.Append(Environment.NewLine);

                if (FUnit[0].ToString() == "cm")
                    XConfig.Append("0");
                else
                    XConfig.Append("1");
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FEyetrackerDegree[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FDisplayDegree[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FDisplayHeight[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FDisplayWidth[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FEyetrackerOffset[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FDisplayOffset[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FEyetrackerSideOffset[0].ToString());
                XConfig.Append(Environment.NewLine);

                XConfig.Append(FEyetrackerRotation[0].ToString());
                XConfig.Append(Environment.NewLine);

                if (FEyetrackerSideOffset[0] != 0)
                    XConfig.Append("1");
                else
                    XConfig.Append("0");
                XConfig.Append(Environment.NewLine);

                if (FEyetrackerRotation[0] != 0)
                    XConfig.Append("1");
                else
                    XConfig.Append("0");
                XConfig.Append(Environment.NewLine);


                FXconfig[0] = XConfig.ToString();
                CalcXConfigPointsToOutput(FUseInitOffset[0]);
            }
        }

        // XConfig calculation
        void CalcXConfigPointsToOutput(bool compensateInitOffset)
        {
            Vector3D ll = new Vector3D();
            Vector3D lr = new Vector3D();
            Vector3D ul = new Vector3D();
            Vector3D ur = new Vector3D();

            ll.x = FDisplayWidth[0] * 0.5 * -1 - (FEyetrackerSideOffset[0] * -1);
            ll.y = FDisplayOffset[0];
            ll.z = FEyetrackerOffset[0] * -1;

            lr.x = FDisplayWidth[0] * 0.5 - (FEyetrackerSideOffset[0] * -1);
            lr.y = FDisplayOffset[0];
            lr.z = FEyetrackerOffset[0] * -1;

            ul.x = FDisplayWidth[0] * 0.5 * -1 - (FEyetrackerSideOffset[0] * -1);
            ul.y = FDisplayOffset[0] + FDisplayHeight[0];
            ul.z = FEyetrackerOffset[0] * -1;

            ur.x = FDisplayWidth[0] * 0.5 - (FEyetrackerSideOffset[0] * -1);
            ur.y = FDisplayOffset[0] + FDisplayHeight[0];
            ur.z = FEyetrackerOffset[0] * -1;

            double oZ = ll.z;
            double oY = ll.y;

            Vector3D uLTemp = ul;
            Vector3D uRTemp = ur;

            uLTemp.z = uLTemp.z - oZ;
            uLTemp.y = uLTemp.y - oY;
            uRTemp.z = uRTemp.z - oZ;
            uRTemp.y = uRTemp.y - oY;

            uLTemp = VMath.RotateX(FDisplayDegree[0] * CDegToRad * -1) * uLTemp;
            uRTemp = VMath.RotateX(FDisplayDegree[0] * CDegToRad * -1) * uRTemp;

            uLTemp.z = uLTemp.z + oZ;
            uLTemp.y = uLTemp.y + oY;
            uRTemp.z = uRTemp.z + oZ;
            uRTemp.y = uRTemp.y + oY;

            ul = uLTemp;
            ur = uRTemp;

            ll = VMath.RotateX(FEyetrackerDegree[0] * CDegToRad) * ll;
            ll = VMath.RotateY(FEyetrackerRotation[0] * CDegToRad * -1) * ll;
            lr = VMath.RotateX(FEyetrackerDegree[0] * CDegToRad) * lr;
            lr = VMath.RotateY(FEyetrackerRotation[0] * CDegToRad * -1) * lr;
            ul = VMath.RotateX(FEyetrackerDegree[0] * CDegToRad) * ul;
            ul = VMath.RotateY(FEyetrackerRotation[0] * CDegToRad * -1) * ul;
            ur = VMath.RotateX(FEyetrackerDegree[0] * CDegToRad) * ur;
            ur = VMath.RotateY(FEyetrackerRotation[0] * CDegToRad * -1) * ur;

            if (compensateInitOffset)
            {
                ll += initOffset;
                lr += initOffset;
                ul += initOffset;
                ur += initOffset;
            }

            // apply plane normal rotation
            double angle = FPlaneNormalRotate[0] * CDegToRad;
            Vector3D llToUr = ur - ll;
            Vector3D planeCenter = ll + (llToUr * 0.5);
            Vector3D dir = (ul - ll).CrossRH(ur - ul);

            dir = ~dir;
            ll = RotateArbitrary(ll, angle, planeCenter, dir);
            ul = RotateArbitrary(ul, angle, planeCenter, dir);
            ur = RotateArbitrary(ur, angle, planeCenter, dir);

            XConfiguration xConfig = new XConfiguration();
            xConfig.LowerLeft = new Point3D(ll.x * 10, ll.y * 10, ll.z * 10);
            xConfig.UpperLeft = new Point3D(ul.x * 10, ul.y * 10, ul.z * 10);
            xConfig.UpperRight = new Point3D(ur.x * 10, ur.y * 10, ur.z * 10);

            FLowerLeft[0] = ll;
            FUpperLeft[0] = ul;
            FUpperRight[0] = ur;

            FXConfiguration[0] = xConfig;
        }

        // rotate "point" about angle "phi" about the line going through "planeCenter" with the direction "dir", phi comes as degree
        private Vector3D RotateArbitrary(Vector3D point, double phi, Vector3D planeCenter, Vector3D dir)
        {
            double x, y, z, a, b, c, u, v, w, fi;
            x = point.x;
            y = point.y;
            z = point.z;
            a = planeCenter.x;
            b = planeCenter.y;
            c = planeCenter.z;
            u = dir.x;
            v = dir.y;
            w = dir.z;
            fi = phi;
            double compX = (a * (v * v + w * w) - u * (b * v + c * w - u * x - v * y - w * z)) * (1 - Math.Cos(fi)) + x * Math.Cos(fi) + (-c * v + b * w - w * y + v * z) * Math.Sin(fi);
            double compY = (b * (u * u + w * w) - v * (a * u + c * w - u * x - v * y - w * z)) * (1 - Math.Cos(fi)) + y * Math.Cos(fi) + (c * u - a * w + w * x - u * z) * Math.Sin(fi);
            double compZ = (c * (u * u + v * v) - w * (a * u + b * v - u * x - v * y - w * z)) * (1 - Math.Cos(fi)) + z * Math.Cos(fi) + (-b * u + a * v - v * x + u * y) * Math.Sin(fi);
            return new Vector3D(compX, compY, compZ);
        }
    }
}


