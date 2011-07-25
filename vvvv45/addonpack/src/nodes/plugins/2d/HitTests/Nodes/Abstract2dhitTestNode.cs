using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;

namespace VVVV.Nodes
{
    public abstract class Abstract2dhitTestNode
    {
        #region Fields
        protected IPluginHost FHost;

        protected ITransformIn FPinInTransform;
        protected IValueIn FPinInPoint;

        protected IValueOut FPinOutPointId;
        protected IValueOut FPinOutObjectId;
        protected IValueOut FPinOutPointHit;
        protected IValueOut FPinOutObjectHit;

        protected List<Hit> FHits = new List<Hit>();
        protected List<bool> FPointHit = new List<bool>();
        protected List<bool> FObjectHit = new List<bool>();

        protected abstract void SetInputPins();
        protected abstract void SetOutputPins();
        protected abstract bool OnEvaluate(int SpreadMax, bool inputchanged);
        
        protected virtual bool ObjectChanged()
        {
            return this.FPinInTransform.PinIsChanged;
        }

        protected virtual int GetSpreadPoint()
        {
            return this.FPinInPoint.SliceCount;
        }

        protected virtual int GetSpreadObject()
        {
            return this.FPinInTransform.SliceCount;
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.CreateTransformInput("Transform In", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInTransform);

            this.FHost.CreateValueInput("Points", 2, null , TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPoint);
            this.FPinInPoint.SetSubType2D(double.MinValue, double.MaxValue, 0.01, 0, 0, false, false, false);

            this.SetInputPins();
           
            this.FHost.CreateValueOutput("Point Id", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPointId);
            this.FPinOutPointId.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);

            this.FHost.CreateValueOutput("Object Id", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutObjectId);
            this.FPinOutObjectId.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);

            this.FHost.CreateValueOutput("Point Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPointHit);
            this.FPinOutPointHit.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueOutput("Object Hit", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutObjectHit);
            this.FPinOutObjectHit.SetSubType(0, 1, 1, 0, false, true, false);

            this.SetOutputPins();
        }
        #endregion

        protected void ResetLists()
        {
            this.FHits.Clear();
            this.FPointHit.Clear();
            this.FObjectHit.Clear();

            for (int i = 0; i < this.GetSpreadPoint(); i++)
            {
                this.FPointHit.Add(false);
            }
        }

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.OnEvaluate(SpreadMax, this.FPinInPoint.PinIsChanged || this.ObjectChanged()))
            {

                this.FPinOutObjectHit.SliceCount = this.FObjectHit.Count;
                this.FPinOutPointHit.SliceCount = this.FPointHit.Count;
                this.FPinOutObjectId.SliceCount = this.FHits.Count;
                this.FPinOutPointId.SliceCount = this.FHits.Count;

                int cnt = 0;
                foreach (Hit hit in this.FHits)
                {
                    this.FPinOutPointId.SetValue(cnt, hit.PointId);
                    this.FPinOutObjectId.SetValue(cnt, hit.ObjectId);
                    cnt++;
                }

                for (int i = 0; i < this.FObjectHit.Count; i++)
                {
                    this.FPinOutObjectHit.SetValue(i, Convert.ToDouble(this.FObjectHit[i]));
                }

                for (int i = 0; i < this.FPointHit.Count; i++)
                {
                    this.FPinOutPointHit.SetValue(i, Convert.ToDouble(this.FPointHit[i]));
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
}
