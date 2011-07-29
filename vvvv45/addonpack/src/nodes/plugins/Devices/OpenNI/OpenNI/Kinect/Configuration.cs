#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using OpenNI;
#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "Configuration",
                Category = "Kinect",
                Help = "Configuration Node for the Kinect",
                Tags = "Kinect, OpenNI",
                Author = "Phlegma")]
    #endregion PluginInfo


    public class Configuration : IPluginEvaluate
    {
        #region fields & pins

        //vvvv
        [Input("Configuration File", DefaultString = "Data\\SamplesConfig.xml", StringType = StringType.Filename, IsSingle=true)]
        IDiffSpread<string> FConfigPinIn;

        [Input("Reload Configuration", IsSingle = true, IsBang = true)]
        IDiffSpread<bool> FReloadIn;

        [Input("Update", IsSingle = true, DefaultValue = 1)]
        IDiffSpread<bool> FUpdateIn;

        [Output("Context")]
        ISpread<Context> FContextOut;

        [Output("Default Values")]
        ISpread<bool> FDefaultValuesOut;

        [Output("Node List")]
        ISpread<string> FNodeListOut;

        [Output("Status")]
        ISpread<string> FStatus;

        [Import()]
        ILogger FLogger;


        //Kinect
        private Context FContext;
        private List<string> FErrors = new List<string>();
        private Thread Updater;
        private bool FInit = true;
        private Object FLockObject = new Object();

        #endregion fields & pins



        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            //Start
            if (FInit)
            {
                FStatus[0] = "Init OpenNI Node.";

                //Init the Context Object
                LoadContext(FConfigPinIn[0]);
                
                //Thread for updating the Generators
                Updater = new Thread(Update);
                Updater.Start();
            }
             

            //check every frame if the Input Pins are changed
            if(FContext != null && FInit == false)
            {

                //check if Filepath or Relaod Pin is changed 
                if (FReloadIn.IsChanged || FConfigPinIn.IsChanged)
                {
                    if (FContext != null)
                    {
                        FContextOut[0] = null;
                        CloseContext();
                    }
                    FInit = true;
                }

                //Spits out the Global Error Massage
                if (!String.IsNullOrEmpty(FContext.GlobalErrorState))
                {
                    FStatus[0] = FContext.GlobalErrorState;
                }

                //Close and restart the Update Thread
                if (FUpdateIn.IsChanged)
                {
                    if (FUpdateIn[0] == true)
                    {
                        if (Updater == null)
                        {
                            Updater = new Thread(Update);
                            Updater.Start();
                        }
                    }
                    else
                    {
                        Updater = null;
                    }
                }

                //writes the Context Object to the Output for 
                //is required for other generators
                FContextOut[0] = FContext;
            }

            FInit = false;
        }

       
        /// <summary>
        /// Aborts the Update Thread and disposes the Context Object
        /// </summary>
        private void CloseContext()
        {
            if (Updater.IsAlive)
            {
                try
                {
                    Updater.Abort();
                }
                catch (ThreadAbortException ex)
                {
                    FLogger.Log(ex);
                }
                finally
                {
                    Updater = null;
                }
            }

            if (FContext != null)
            {
                Thread.Sleep(100);
                //FContext.StopGeneratingAll();
                FContext.Shutdown();
                FContext.Dispose();
                FContext = null;
            }
        }

        /// <summary>
        /// Load the Context XML from File or from the OpenNI Libary
        /// </summary>
        /// <param name="FilePath"></param>
        private void LoadContext(string FilePath)
        {

            //ty to open Kinect Context with given ConfigFilePath
            try
            {
                FContext = new Context(FilePath);
                FDefaultValuesOut[0] = false;
            }
            catch (StatusException ex)
            {
                FLogger.Log(ex);
            }
            catch (GeneralException e)
            {
                FLogger.Log(e);
            }

            //try to OpenOpenNi Cofig File
            if (FContext == null)
            {
                try
                {
                    FContext = new Context();
                    FDefaultValuesOut[0] = true;
                }
                catch (StatusException ex)
                {
                    FLogger.Log(ex);
                }
                catch (GeneralException e)
                {
                    FLogger.Log(e);
                }
            }

            if (FContext == null)
                FStatus[0] = "No Kinect Context found.";
            else
            {
                FContext.ErrorStateChanged += new EventHandler<ErrorStateEventArgs>(FContext_ErrorStateChanged);

                FStatus[0] = "Kinect found.";

                //write all found Nodes in the config xml to the Output Pin
                List<string> NodeNames = ReadNodeList();
                FNodeListOut.SliceCount = NodeNames.Count;
                for (int i = 0; i < NodeNames.Count; i++)
                {
                    FNodeListOut[i] = NodeNames[i];
                }
            }
        }

        /// <summary>
        /// callback Function for the Error Changed Event 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void FContext_ErrorStateChanged(object sender, ErrorStateEventArgs e)
        {
            FLogger.Log(LogType.Error,"Global Kinect Error");
        }

        /// <summary>
        /// Reads all nodes form the Config XML file
        /// </summary>
        /// <returns>List of all Nodes</returns>
        private List<string> ReadNodeList()
        {
            NodeInfoList NodeInfos = FContext.EnumerateExistingNodes();
            IEnumerator<NodeInfo> Infos = NodeInfos.GetEnumerator();

            List<string> NodeNames = new List<string>();
            while (Infos.MoveNext())
            {
                NodeNames.Add(Infos.Current.InstanceName);
            }

            return NodeNames;
        }

        /// <summary>
        /// Thread for updating all Generators
        /// </summary>
        private void Update()
        {
            while (FUpdateIn[0] == true && FInit == false)
            {
                try
                {   
                    //The way how to update
                    if (FContext != null)
                        FContext.WaitAnyUpdateAll();
                }
                catch (StatusException ey)
                {
                    Debug.WriteLine(ey.Message);
                }
                catch (GeneralException ez)
                {
                    Debug.WriteLine(ez.Message);
                }
                catch (AccessViolationException ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }
    }
}
