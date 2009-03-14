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

        public bool mChangedInputOut = false;
        public bool mChangedInputIn = false;

        // Daten Liste und Objecte
        public SortedList<int,SortedList<string,string>> mCssPropertiesOwn  = new SortedList<int,SortedList<string,string>>();
        public SortedList<int, SortedList<string, string>> mCssPropertiesIn = new SortedList<int, SortedList<string, string>>();





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


        public void GetCssProperties(int Index, out SortedList<string, string> GuiDaten)
        {
            SortedList<string, string> tCssProperty = new SortedList<string, string>();
            mCssPropertiesOwn.TryGetValue(Index, out tCssProperty);
            GuiDaten = tCssProperty;
        }

        public void GetInputChanged(out bool ChangedInput)
        {
            ChangedInput = mChangedInputOut;
        }

         


        public void ConnectPin(IPluginIO Pin)
        {
            if (Pin == FCssPropertiesIn)
            {
                INodeIOBase usI;
                FCssPropertiesIn.GetUpstreamInterface(out usI);
                FUpstreamStyleIn = usI as IHttpGUIStyleIO;
                mChangedInputOut = true;
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
            
            FCssPropertieOut.SliceCount = SpreadMax;
            mChangedInputOut = false;
            int usS;

            if (FUpstreamStyleIn != null)
            {
                mCssPropertiesIn.Clear();
                int tNillCounter = 0;

                for (int i = 0; i < SpreadMax; i++)
                {
                    //get upstream slice index

                    FCssPropertiesIn.GetUpsreamSlice(i, out usS);

                    SortedList<string, string> tStylePropertie = new SortedList<string, string>();
                    FUpstreamStyleIn.GetCssProperties(usS, out tStylePropertie);

                    if (tStylePropertie != null)
                    {
                        mCssPropertiesIn.Add(i, tStylePropertie);
                    }
                    else
                    {
                        SortedList<string, string> tDummyCssProperty;
                        mCssPropertiesIn.TryGetValue(tNillCounter, out tDummyCssProperty);
                        mCssPropertiesIn.Add(i, tDummyCssProperty);
                        if (tNillCounter < mCssPropertiesIn.Count)
                        {
                            tNillCounter++;
                        }
                        else
                        {
                            tNillCounter = 0;
                        }
                    }


                    bool tChangedValue;
                    FUpstreamStyleIn.GetInputChanged(out tChangedValue);
                    mChangedInputIn = tChangedValue;
                    if (mChangedInputIn == true)
                    {
                        mChangedInputOut = true;
                    }
                    else
                    {
                        mChangedInputOut = false;
                    }
                }
            }
            else
            {
                mChangedInputIn = false;
            }

            
            this.OnEvaluate(SpreadMax);
            
        }
        #endregion Evaluate





     
    }
}
