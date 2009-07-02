using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;
using VVVV.Nodes.HttpGUI.Datenobjekte;



namespace VVVV.Nodes.HttpGUI.CSS
{
    public abstract class BaseCssNode : IHttpGUIStyleIO, IPluginConnections
    {



        #region field Definition

        //Host
        protected IPluginHost FHost;

        // Standart Pins
        public INodeOut FCssPropertieOut;

        public INodeIn FCssPropertiesIn;
        public IHttpGUIStyleIO FUpstreamStyleIn;

        // Daten Liste und Objecte
        public SortedList<int,SortedList<string,string>> mCssPropertiesOwn  = new SortedList<int,SortedList<string,string>>();
        public SortedList<int, SortedList<string, string>> mCssPropertiesIn = new SortedList<int, SortedList<string, string>>();
        public SortedList<int, SortedList<string, string>> mCssList = new SortedList<int, SortedList<string, string>>();



        #endregion field Definition







        #region abstract Methods


        protected abstract void OnConfigurate(IPluginConfig Input);
        protected abstract void OnEvaluate(int SpreadMax);
        protected abstract void OnPluginHostSet();
        //abstract public void GetCssProperties(int Index, out SortedList<string,string> GuiDaten);
        //abstract public void GetInputChanged(out bool ChangedInput);

        #endregion abstract Methods








        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

           
            //Create Inputs
            FHost.CreateNodeInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FCssPropertiesIn);
            FCssPropertiesIn.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);

            //create outputs	    	
            FHost.CreateNodeOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FCssPropertieOut);
            FCssPropertieOut.SetSubType(new Guid[1] { HttpGUIStyleIO.GUID }, HttpGUIStyleIO.FriendlyName);
            FCssPropertieOut.SetInterface(this);

            this.OnPluginHostSet();
        }


        public virtual bool AutoEvaluate
        {
            get { return false; }
        }

        #endregion pin creation






        #region NodeIO


        public void GetCssProperties(int Index, int SpreadMax, out SortedList<string, string> CssList)
        {
                string NodePath;
                FHost.GetNodePath(false, out NodePath);
                Debug.WriteLine(String.Format("NOde {0}", NodePath));
                Debug.WriteLine(String.Format("Index: {0} / mCssPropertie.Count: {1}", Index, mCssPropertiesOwn.Count));

                if (Index < mCssPropertiesOwn.Count)
                {
                    SortedList<string, string> tCssList;
                    mCssPropertiesOwn.TryGetValue(mCssPropertiesOwn.Count - (SpreadMax % Index), out tCssList);
                    CssList = tCssList;
                }
                else
                {
                    SortedList<string, string> tCssList;
                    mCssPropertiesOwn.TryGetValue(Index, out tCssList);
                    CssList = tCssList;
                }

        }


        public void ConnectPin(IPluginIO Pin)
        {
            if (Pin == FCssPropertiesIn)
            {
                INodeIOBase usI;
                FCssPropertiesIn.GetUpstreamInterface(out usI);
                FUpstreamStyleIn = usI as IHttpGUIStyleIO;
                
            }
        }



        public void DisconnectPin(IPluginIO Pin)
        {
            if (Pin == FCssPropertiesIn)
            {
                FUpstreamStyleIn = null;
            }
        }

        #endregion NodeIO









        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            this.OnConfigurate(Input);
        }
        #endregion









        #region Evaluate

        public void Evaluate(int SpreadMax)
        {

            mCssList.Capacity = SpreadMax;

            this.OnEvaluate(SpreadMax);

            FCssPropertieOut.SliceCount = SpreadMax;

            int usS;

            if (FUpstreamStyleIn != null)
            {
                Debug.WriteLine("Enter Upstream");
                int tOwnCssLength = mCssPropertiesOwn.Count;

                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index

                    FCssPropertiesIn.GetUpsreamSlice(i, out usS);

                    SortedList<string, string> tStylePropertyIn = new SortedList<string, string>();
                    FUpstreamStyleIn.GetCssProperties(usS,SpreadMax, out tStylePropertyIn);


                    SortedList<string, string> tCssStyleSliceOwn = new SortedList<string, string>();
                    mCssPropertiesOwn.TryGetValue(i, out tCssStyleSliceOwn);


                    if (tCssStyleSliceOwn != null)
                    {
                        foreach (KeyValuePair<string, string> pKey in tStylePropertyIn)
                        {
                            if (tCssStyleSliceOwn.ContainsKey(pKey.Key) == false)
                            {
                                tCssStyleSliceOwn.Add(pKey.Key, pKey.Value);
                            }
                        }

                        mCssPropertiesOwn.Remove(i);
                        mCssPropertiesOwn.Add(i, tCssStyleSliceOwn);
                    }
                    else
                    {

                        ////???????????
                    }

                }
            }
        }

        #endregion Evaluate


    }
}
