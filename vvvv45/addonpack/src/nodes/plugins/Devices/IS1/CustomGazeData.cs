#region usings

using VVVV.Utils.VMath;
using Tobii.Eyetracking.Sdk;
#endregion

namespace IS1
{
    public class CustomGazeData
    {

        #region data description
        /* Eye Position (3D): 
         * The eye position is provided for the left and right eye individually and describes the position of the eyeball in
         * 3D space. Three floating point values are used to describe the x, y and z coordinate respectively. The position is
         * described in the UCS coordinate system.
         * 
         * Relative Eye Position (3D)
         * The relative eye position is provided for the left and right eye individually and gives the relative position of the 
         * eyeball in the headbox volume as three normalized coordinates. This data can be used to visualize the position
         * of the eyes similar to how it is done in the Tobii track status control. This is mainly a tool to help the user
         * position himself/herself in front of the tracker.
         * 
         * Gaze Point (3D):
         * The gaze point is provided for the left and right eye individually and describes the position of the intersection 
         * between the line originating from the eye position point with the same direction as the gaze vector and the 
         * calibration plane.
         * The gaze vector can be computed by substracting the 3D gaze point and the 3D eye position and normalizing 
         * the resulting vector.
         * 
         * Relative Gaze Point (2D):
         * The relative gaze point is provided for the left and right eye individually and corresponds to the two
         * dimensional position of the gaze point within the calibration plane. The coordinates are normalized to [0,1]
         * with the point (0,0) in the upper left corner from the users point of view. The x-coordinate increases to the
         * right and the y-coordinate increases towards the bottom of the screen.
         * 
         * Validity Code:
         * The validity code is an estimate of how certain the eye tracker is that the data given for an eye really originates
         * from that eye. When the tracker finds two eyes in the camera image, identifying which one is the left and
         * which one is the right eye is very straightforward as well as when no eyes are found at all. The most
         * challenging case is when the tracker only finds one eye in the camera image. When that happens, the image
         * processing algorithms try to deduce if the eye in question is the left or the right one. This is done by referring
         * to previous eye positions, the position in the camera sensor and certain image features. The validity codes
         * describe the outcome of this deduction.
         * The validity codes can be used to filter out data that is most likely incorrect. Normally it is recommended that
         * all samples with validity code 2 or higher are removed or ignored.
         * 
         */
        #endregion data description

        #region attributes
        private Vector3D lePos3D;
        private Vector3D rePos3D;
        private Vector3D lePos3DRel;
        private Vector3D rePos3DRel;
        private Vector3D leGazePoint;
        private Vector3D reGazePoint;
        private Vector2D leGazePointRel;
        private Vector2D reGazePointRel;

        private float invertXAxis = 1;

        private int leValidity;
        private int reValidity;
        private float lePupilDiameter;
        private float rePupilDiameter;
        #endregion

        #region get set
        public Vector3D LEPos3D
        {
            get { return lePos3D; }
            set { lePos3D = value; }
        }
        public Vector3D REPos3D
        {
            get { return rePos3D; }
            set { rePos3D = value; }
        }
        public Vector3D LEPos3DRel
        {
            get { return lePos3DRel; }
            set { lePos3DRel = value; }
        }
        public Vector3D REPos3DRel
        {
            get { return rePos3DRel; }
            set { rePos3DRel = value; }
        }
        public Vector3D LEGazePoint
        {
            get { return leGazePoint; }
            set { leGazePoint = value; }
        }
        public Vector3D REGazePoint
        {
            get { return reGazePoint; }
            set { reGazePoint = value; }
        }
        public Vector2D LEGazePointRel
        {
            get { return leGazePointRel; }
            set { leGazePointRel = value; }
        }
        public Vector2D REGazePointRel
        {
            get { return reGazePointRel; }
            set { reGazePointRel = value; }
        }
        public int LEValidity
        {
            get { return leValidity; }
        }
        public int REValidity
        {
            get { return reValidity; }
        }
        public float LEPupilDiameter
        {
            get { return lePupilDiameter; }
        }
        public float REPupilDiameter
        {
            get { return rePupilDiameter; }
        }
        #endregion

        // constructor
        public CustomGazeData(GazeDataItem gdi)
        {
            lePos3D = new Vector3D(gdi.LeftEyePosition3D.X * invertXAxis,
                                             gdi.LeftEyePosition3D.Y,
                                             gdi.LeftEyePosition3D.Z);

            rePos3D = new Vector3D(gdi.RightEyePosition3D.X * invertXAxis,
                                             gdi.RightEyePosition3D.Y,
                                             gdi.RightEyePosition3D.Z);

            lePos3DRel = new Vector3D(gdi.LeftEyePosition3DRelative.X * invertXAxis,
                                             gdi.LeftEyePosition3DRelative.Y,
                                             gdi.LeftEyePosition3DRelative.Z);

            rePos3DRel = new Vector3D(gdi.RightEyePosition3DRelative.X * invertXAxis,
                                             gdi.RightEyePosition3DRelative.Y,
                                             gdi.RightEyePosition3DRelative.Z);

            leGazePoint = new Vector3D(gdi.LeftGazePoint3D.X * invertXAxis,
                                             gdi.LeftGazePoint3D.Y,
                                             gdi.LeftGazePoint3D.Z);

            reGazePoint = new Vector3D(gdi.RightGazePoint3D.X * invertXAxis,
                                             gdi.RightGazePoint3D.Y,
                                             gdi.RightGazePoint3D.Z);

            leGazePointRel = new Vector2D(gdi.LeftGazePoint2D.X,
                                             gdi.LeftGazePoint2D.Y);

            reGazePointRel = new Vector2D(gdi.RightGazePoint2D.X,
                                             gdi.RightGazePoint2D.Y);

            leValidity = gdi.LeftValidity;
            reValidity = gdi.RightValidity;
            lePupilDiameter = gdi.LeftPupilDiameter;
            rePupilDiameter = gdi.RightPupilDiameter;
        }


    }
}
