using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

using VVVV.Webinterface;
using VVVV.Webinterface.Utilities;



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
        public Dictionary<int,SortedList<string,string>> mCssPropertiesOwn  = new Dictionary<int,SortedList<string,string>>();
        public Dictionary<int, SortedList<string, string>> mCssPropertiesIn = new Dictionary<int, SortedList<string, string>>();
        public Dictionary<int, SortedList<string, string>> mCssPropertiesCombined = new Dictionary<int, SortedList<string, string>>();

        public string mPluginName = "";
        

        #endregion field Definition







        #region abstract Methods

        
        protected abstract void OnConfigurate(IPluginConfig Input);
        protected abstract void OnEvaluate(int SpreadMax);
        protected abstract void OnPluginHostSet();


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


        public void GetCssProperties(int Index, out SortedList<string, string> CssList)
        {
                //string NodePath;
                //FHost.GetNodePath(false, out NodePath);
                //////Debug.WriteLine(String.Format("NOde {0}", NodePath));
                //////Debug.WriteLine(String.Format("Index: {0} / mCssPropertie.Count: {1}", Index, mCssPropertiesOwn.Count));
            if (FCssPropertiesIn.IsConnected)
            {
                if (Index >= mCssPropertiesCombined.Count)
                {
                    mCssPropertiesCombined.TryGetValue(Index % mCssPropertiesCombined.Count, out CssList);
                }
                else
                {
                    mCssPropertiesCombined.TryGetValue(Index, out CssList);
                }
            }
            else
            {
                if (Index >= mCssPropertiesOwn.Count)
                {
                    mCssPropertiesOwn.TryGetValue(Index % mCssPropertiesOwn.Count, out CssList);
                }
                else
                {
                    mCssPropertiesOwn.TryGetValue(Index, out CssList);
                }
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


            try
            {
                this.OnEvaluate(SpreadMax);

                int usS;

                if (FUpstreamStyleIn != null)
                {
                    string NodePath;
                    FHost.GetNodePath(false, out NodePath);
                    mCssPropertiesCombined.Clear();
                    ////Debug.WriteLine("Enter Upstream: " + NodePath);

                    int SliceOffsetCounter = 0;

                    for (int i = 0; i < SpreadMax; i++)
                    {
                        //get upstream slice index

                        FCssPropertiesIn.GetUpsreamSlice(i, out usS);


                        SortedList<string, string> tStylePropertyIn;
                        FUpstreamStyleIn.GetCssProperties(i, out tStylePropertyIn);


                        SortedList<string, string> tCssSliceList;
                        mCssPropertiesOwn.TryGetValue(i, out tCssSliceList);



                        if (tCssSliceList == null)
                        {

                            mCssPropertiesOwn.TryGetValue(SliceOffsetCounter, out tCssSliceList);
                            SortedList<string, string> tWorkerList = new SortedList<string, string>(tCssSliceList);
                            SliceOffsetCounter++;
                            if (SliceOffsetCounter >= mCssPropertiesOwn.Count)
                            {
                                SliceOffsetCounter = 0;
                            }

                            foreach (KeyValuePair<string, string> pKey in tStylePropertyIn)
                            {
                                if (tWorkerList.ContainsKey(pKey.Key))
                                {
                                    tWorkerList.Remove(pKey.Key);
                                    tWorkerList.Add(pKey.Key, pKey.Value);
                                }
                                else
                                {
                                    tWorkerList.Add(pKey.Key, pKey.Value);
                                }
                            }

                            if (mCssPropertiesCombined.ContainsKey(i))
                            {
                                mCssPropertiesCombined.Remove(i);
                                mCssPropertiesCombined.Add(i, new SortedList<string, string>(tWorkerList));
                            }
                            else
                            {
                                mCssPropertiesCombined.Add(i, new SortedList<string, string>(tWorkerList));
                            }

                        }
                        else if (tStylePropertyIn != null)
                        {

                            SortedList<string, string> tWorkerList = new SortedList<string, string>(tCssSliceList);
                            foreach (KeyValuePair<string, string> pKey in tStylePropertyIn)
                            {
                                if (tWorkerList.ContainsKey(pKey.Key) == false)
                                {
                                    tWorkerList.Add(pKey.Key, pKey.Value);
                                }
                            }

                            mCssPropertiesCombined.Remove(i);
                            mCssPropertiesCombined.Add(i, new SortedList<string, string>(tWorkerList));
                        }
                    }
                }

                FCssPropertieOut.SliceCount = SpreadMax;
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, ex.Message);
            }
        }

        #endregion Evaluate




        public int GetSliceCount(IPluginIn[] pInputs)
        {
           
            List<int> tSliceCount = new List<int>(); 
            for (int i = 0; i < pInputs.Length; i++)
            {
                tSliceCount.Add(pInputs[i].SliceCount);
            }

            tSliceCount.Sort();

            return tSliceCount[tSliceCount.Count -1];
        }


		#region IHttpGUIStyleIO Members


		public bool PinIsChanged()
		{
			return true;
		}

		#endregion
	}
}
