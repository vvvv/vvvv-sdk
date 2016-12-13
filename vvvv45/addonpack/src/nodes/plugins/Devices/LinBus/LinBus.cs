using System;
using System.Collections.Generic;
using System.IO.Ports;
using VVVV.PluginInterfaces.V1;
using System.Diagnostics;
using System.Threading;
using System.Text;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of RS232.
    /// </summary>
    /// 
    public class LinBus : IDisposable, IPlugin
    {
        // PLUGIN HOST
        ///////////////////////
        private IPluginHost FHost;
        private bool FDisposed = false;

        //Input Pins
        private IStringIn FIdentifier;
        private IValueIn FEnableIn;
        private IValueIn FPortNumberIn;
        private IValueIn FBaudrateIn;
        private IValueIn FDataFieldSize;
        private IValueIn FTimeOut;

        // Output Pins
        private IStringOut FDataOut;
        private IValueOut FConnectedOut;
        private IValueOut FPortsOut;
        private Driver _Driver;
            

        

        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public LinBus()
        {
            
        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// runtime from inside the finalizer and you should not reference
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (FDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Message, "Image (Http Gui) Node is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }

            FDisposed = true;
        }


        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in WebTypes derived from this class.
        /// </summary>
        ~LinBus()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion constructor / desconstructor

        #region pluginInfo

        public bool AutoEvaluate
        {
            get { return false; }
        }

        /// <summary>
        /// PluginInfo
        /// </summary>
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    //fill out nodes info
                    //see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
                    FPluginInfo = new PluginInfo();

                    //the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "LinBus";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Devices";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "";
                    //specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "Lin Bus, Serial Data Protocol";

                    //give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    //any known problems?
                    FPluginInfo.Bugs = "";
                    //any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    //leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    //leave above as is
                }
                return FPluginInfo;
            }
        }

        #endregion pluginInfo

        public void SetPluginHost(IPluginHost Host)
        {
            FHost = Host;

            
            

            //INPUT-PINS
            FHost.CreateStringInput("Identifier", TSliceMode.Single, TPinVisibility.True, out FIdentifier);
            FIdentifier.SetSubType("", false);

            FHost.CreateValueInput("Enable", 1, null, TSliceMode.Single, TPinVisibility.True, out FEnableIn);
            FEnableIn.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateValueInput("Port", 1, null, TSliceMode.Single, TPinVisibility.True, out FPortNumberIn);
            FPortNumberIn.SetSubType(1, 15, 1, 1, false, false, true);

            FHost.CreateValueInput("Baudrate", 1, null, TSliceMode.Single, TPinVisibility.True, out FBaudrateIn);
            FBaudrateIn.SetSubType(Double.MinValue, Double.MaxValue, 1, 9600, false, false, true);

            FHost.CreateValueInput("Message Frame Size", 1, null, TSliceMode.Single, TPinVisibility.True, out FDataFieldSize);
            FDataFieldSize.SetSubType(0, double.MaxValue, 1, 8, false, false, true);

            FHost.CreateValueInput("Timeout", 1, null, TSliceMode.Single, TPinVisibility.True, out FTimeOut);
            FTimeOut.SetSubType(0, double.MaxValue, 1, 500, false, false, true);



            // OUTPUT-PINS
            FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FDataOut);
            FDataOut.SetSubType("", false);

            FHost.CreateValueOutput("Connected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnectedOut);
            FConnectedOut.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateValueOutput("Available Ports", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPortsOut);
            FPortsOut.SetSubType(1, 15, 1, 1, false, false, true);
        }

        public void Configurate(IPluginConfig pInput)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        private System.DateTime tStart;

        /// <summary>
        /// The Mainloop
        /// </summary>
        public void Evaluate(int SpreadMax)
        {

            string[] tAvailablePorts = SerialPort.GetPortNames();
            FPortsOut.SliceCount = tAvailablePorts.Length;
            FConnectedOut.SliceCount = tAvailablePorts.Length;

            for (int i = 0; i < tAvailablePorts.Length; i++)
                FPortsOut.SetValue(i, Convert.ToDouble(tAvailablePorts[i].Substring(3)));


            if (FPortNumberIn.PinIsChanged)
            {
                if (_Driver != null)
                {
                    _Driver.StopThread = false;

                    while (_Driver.ThreadIsAlive())
                        Thread.Sleep(1);

                    _Driver.Dispose();
                    _Driver = null;
                }
            }



            double currentSliceEnabled;
            FEnableIn.GetValue(0, out currentSliceEnabled);





            if (currentSliceEnabled > 0.5)
            {
                try
                {
                    if (_Driver == null)
                    {
                        double currentSlicePortNumber;
                        FPortNumberIn.GetValue(0, out currentSlicePortNumber);
                        if ((new List<string>(SerialPort.GetPortNames())).Contains(String.Format("COM{0}", currentSlicePortNumber)))
                        {

                            string currentSliceIndentifier;
                            double currentSliceBaudrate;
                            double currentSliceDataFieldSize;
                            double currentTimeOutTime;

                            FEnableIn.GetValue(0, out currentSliceEnabled);
                            FIdentifier.GetString(0, out currentSliceIndentifier);
                            FDataFieldSize.GetValue(0, out currentSliceDataFieldSize);
                            FTimeOut.GetValue(0, out currentTimeOutTime);
                            FBaudrateIn.GetValue(0, out currentSliceBaudrate);

                            _Driver = new Driver((int)currentSlicePortNumber, (int)currentSliceBaudrate, Parity.None, 8, StopBits.One);

                            _Driver.SetParameters(currentSliceIndentifier, (int)currentSliceDataFieldSize, (int)currentTimeOutTime);

                            _Driver.StartThread();
                        }
                    }
                    else
                    {

                        string currentSliceIndentifier;
                        double currentSliceBaudrate;
                        double currentSliceDataFieldSize;
                        double currentTimeOutTime;


                        FIdentifier.GetString(0, out currentSliceIndentifier);
                        FDataFieldSize.GetValue(0, out currentSliceDataFieldSize);
                        FTimeOut.GetValue(0, out currentTimeOutTime);
                        FBaudrateIn.GetValue(0, out currentSliceBaudrate);

                        if (FBaudrateIn.PinIsChanged)
                        {
                            _Driver.Baudrate = (int)currentSliceBaudrate;
                        }

                        _Driver.SetParameters(currentSliceIndentifier, (int)currentSliceDataFieldSize, (int)currentTimeOutTime);

                        List<string> tData = new List<string>();
                        tData = _Driver.getData();

                        if (tData.Count > 0)
                        {

                            FDataOut.SliceCount = tData.Count;
                            string tSlice = String.Empty;

                            for (int i = 0; i < tData.Count; i++)
                            {
                                System.TimeSpan tDiff = System.DateTime.Now - tStart;

                                Debug.WriteLine(tDiff.TotalMilliseconds);
                                tSlice += tData[i];
                            }

                            FDataOut.SetString(0, tSlice);
                            tStart = System.DateTime.Now;
                        }
                        else
                        {
                            FDataOut.SliceCount = 0;
                            //FDataOut.SetString(0, "");
                        }

                        tData = null;
                    }
                }
                catch (Exception ex)
                {
                    FHost.Log(TLogType.Error, ex.Message);
                }
            }
            else
            {


                if (_Driver != null)
                {
                    _Driver.StopThread = false;

                    while (_Driver.ThreadIsAlive())
                        Thread.Sleep(1);

                    _Driver.Dispose();
                    _Driver = null;
                }
            }
        }
    }


    #region Driver

    public class Driver : IDisposable
    {
        private SerialPort _Port;
        private Thread _Thread;

        private string _DataToSend = "";
        private int _BytesToWait = 0;
        private int _TimeOutTime = 1000;

        private List<string> _Data = new List<string>();
        private object _SyncRoot = new object();
        private bool _StopThread = true;
        private System.Diagnostics.Stopwatch _StopWatch;


        public List<string> Data { get { return _Data; } }
        public int Baudrate { get { return _Port.BaudRate; } set { _Port.BaudRate = value; } }
        public bool Connected { get { return _Port.IsOpen; } }

        public bool StopThread
        {
            set
            {
                _StopThread = value;
            }
        }

        public Driver(int pComPort, int pBaudrate, Parity pParity, int pDataBits, StopBits pStopBits)
        {
            _Port = new SerialPort("COM" + (pComPort).ToString(), pBaudrate, pParity, pDataBits, pStopBits);
            _Port.Encoding = System.Text.Encoding.Default;
            _Port.ReadTimeout = 200;
            _Port.WriteTimeout = 200;
            _StopThread = true;
            _Port.Open();
            _StopWatch= new System.Diagnostics.Stopwatch();
            _StopWatch.Start();

        }


        public void Dispose()
        {
            if (_Port.IsOpen)
                _Port.Close();

            _Port = null;
        }

        public void SetParameters(string pDataToSend, int pBytesToWait, int pTimeOutTime)
        {
            _DataToSend = pDataToSend;
            _BytesToWait = pBytesToWait;
            _TimeOutTime = pTimeOutTime;
        }

        public void StartThread()
        {
            _Thread = new Thread(SendAndWait);
            _Thread.Start();
        }

        public bool ThreadIsAlive()
        {
            if (_Thread != null)
                if (_Thread.IsAlive)
                    return true;
            return false;
        }

        private void SendAndWait()
        {
            while (_StopThread)
            {
                try
                {
                    int tBaudrate = _Port.BaudRate;

                    _Port.BaudRate = (int)(Convert.ToDouble(tBaudrate) * 8.0 / 13.0);

                    _Port.Write("\x00");
                    _Port.BaudRate = tBaudrate;
                    _Port.Write("\x55");

                    _Port.Write(_DataToSend);

                    while (_Port.BytesToRead < _BytesToWait + 4 && _StopWatch.ElapsedMilliseconds < _TimeOutTime)
                    {
                        Thread.Sleep(1);
                    }
                        

                    _StopWatch.Reset();
                    _StopWatch.Start();
                    
                    string tData = _Port.ReadExisting();
                    tData = tData.Remove(0, 3);
                    tData = tData.Substring(0,tData.Length - 1);
                    _Data.Add(tData);
                    

                }
                catch (Exception ex)
                {

                }

            } 
        }


        public List<string> getData()
        {
              lock(_SyncRoot)
              {
                  List<string> tData =  new List<string>(_Data);
                  _Data.Clear();

                  return tData;
              } 
        }
    }

    #endregion Driver
}
