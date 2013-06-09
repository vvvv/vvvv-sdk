using System;
using System.Collections.Generic;
using System.IO.Ports;
using VVVV.PluginInterfaces.V1;
using System.Diagnostics;

namespace VVVV.Nodes
{
    /// <summary>
    /// Description of RS232.
    /// </summary>
    /// 
    public class RS232 : IDisposable, IPlugin
    {
        // PLUGIN HOST
        private IPluginHost FHost;

        // Disposed Flag
        private bool FDisposed = false;


        //Input Pins
        private IStringIn FDataIn;
        private IValueIn FDoSendIn;
        private IValueIn FEnableIn;
        private IValueIn FKeepLastDataIn;
        private IValueIn FPortNumberIn;
        private IValueIn FBaudrateIn;
        private IEnumIn FParityIn;
        private IEnumIn FDatabitsIn;
        private IEnumIn FStopbitsIn;
        private IEnumIn FHandShakeIn;
        private IValueIn FReadBufferIn;

        // Output Pins
        private IStringOut FDataOut;
        private IValueOut FOnDataOut;
        private IValueOut FConnectedOut;
        private IValueOut FPortsOut;
        private IValueOut FReadBufferOut;
        



        private List<Port> _Ports = new List<Port>();

        private string[] _AvailablePorts;



        public RS232()
        {
            List<string> tPorts = new List<string>(SerialPort.GetPortNames());

            tPorts.Sort();

            _AvailablePorts = tPorts.ToArray();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!FDisposed)
            {
                if (disposing)
                {
                    // close ComPorts
                    foreach (Port tPort in _Ports)
                        tPort.Dispose();
                }
            }
            FDisposed = true;
        }


        public bool AutoEvaluate
        {
            get { return true; }
        }

        /// <summary>
        /// PluginInfo
        /// </summary>
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();

                // PLUGIN INFORMATIONS
                ///////////////////////
                Info.Name = "RS232";
                Info.Category = "Devices";
                Info.Version = "Spreadable Legacy";
                Info.Help = "none";
                Info.Bugs = "none";

                Info.Credits = "Christian Moldenhauer";
                Info.Warnings = "none";

                // STACK TRACES
                /////////////// 
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }

        public void SetPluginHost(IPluginHost Host)
        {
            FHost = Host;


            //INPUT-PINS
            FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FDataIn);
            FDataIn.SetSubType("", false);

            FHost.CreateValueInput("Do Send", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDoSendIn);
            FDoSendIn.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateValueInput("Keep Last Data", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FKeepLastDataIn);
            FKeepLastDataIn.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateValueInput("Baudrate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBaudrateIn);
            FBaudrateIn.SetSubType(Double.MinValue, Double.MaxValue, 1, 9600, false, false, true);
          
            FHost.CreateEnumInput("Data Bits", TSliceMode.Dynamic, TPinVisibility.True, out FDatabitsIn);
            FDatabitsIn.SetSubType("DataBits");

            FHost.CreateEnumInput("Stop Bits", TSliceMode.Dynamic, TPinVisibility.True, out FStopbitsIn);
            FStopbitsIn.SetSubType("StopBits");
            
            FHost.CreateEnumInput("Parity", TSliceMode.Dynamic, TPinVisibility.True, out FParityIn);
            FParityIn.SetSubType("Parity");

            FHost.CreateEnumInput("Hand Shake", TSliceMode.Dynamic, TPinVisibility.True, out FHandShakeIn);
            FHandShakeIn.SetSubType("Hand Shake");
            FHost.UpdateEnum("Hand Shake", "None", new string[] { "None", "RequestToSend", "RequestToSendXOnXOff", "XOnXOff" });

            FHost.CreateValueInput("ReadBuffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReadBufferIn);
            FReadBufferIn.SetSubType(0, Double.MaxValue,1, 4096, false, false, true);

            FHost.CreateValueInput("Enabled", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FEnableIn);
            FEnableIn.SetSubType(0, 1, 1, 0, false, false, true);

			FHost.CreateValueInput("ComPort", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPortNumberIn);
            FPortNumberIn.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

           
            // OUTPUT-PINS
            FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FDataOut);
            FDataOut.SetSubType("", false);

            FHost.CreateValueOutput("On Data", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FOnDataOut);
            FOnDataOut.SetSubType(0, 1, 1, 0, true, false, true);

            FHost.CreateValueOutput("IsConnected", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FConnectedOut);
            FConnectedOut.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateValueOutput("Available Ports", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FPortsOut);
            FPortsOut.SetSubType(1, 15, 1, 1, false, false, true);

            FHost.CreateValueOutput("ReadBuffer Size", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReadBufferOut);
            FReadBufferOut.SetSubType(0, Double.MaxValue, 1, 0, false, false, true);

            FPortsOut.SliceCount = _AvailablePorts.Length;
        }

        public void Configurate(IPluginConfig pInput)
        {
            //nothing to configure in this plugin
            //only used in conjunction with inputs of type cmpdConfigurate
        }

        /// <summary>
        /// The Mainloop
        /// </summary>
        public void Evaluate(int SpreadMax)
        {
            try
            {
                FDataOut.SliceCount = SpreadMax;
                FOnDataOut.SliceCount = SpreadMax;
                FConnectedOut.SliceCount = SpreadMax;
                                
                for (int i = 0; i < _AvailablePorts.Length; i++)
                    FPortsOut.SetValue(i, Convert.ToDouble(_AvailablePorts[i].Substring(3)));



                string currentSliceData;
                double currentSliceDoSend;
                double currentSliceEnabled;
                double currentSliceKeepLastData;
                double currentSlicePortNumber;
                double currentSliceBaudrate;
                string currentSliceParity;
                string currentSliceDatabits = "";
                string currentSliceStopbits;
                string currentSliceHandShake;
                double currentReadBufferSize;



                //loop for all slices
                for (int i = 0; i < SpreadMax; i++)
                {
                    FDoSendIn.GetValue(i, out currentSliceDoSend);
                    FEnableIn.GetValue(i, out currentSliceEnabled);
                    FKeepLastDataIn.GetValue(i, out currentSliceKeepLastData);
                    FPortNumberIn.GetValue(i, out currentSlicePortNumber);
                    FBaudrateIn.GetValue(i, out currentSliceBaudrate);
                    FParityIn.GetString(i, out currentSliceParity);
                    FDatabitsIn.GetString(i, out currentSliceDatabits);
                    FStopbitsIn.GetString(i, out currentSliceStopbits);
                    FHandShakeIn.GetString(i, out currentSliceHandShake);
                    FReadBufferIn.GetValue(i, out currentReadBufferSize);

                    Port tPort;

                    if (Convert.ToBoolean(currentSliceEnabled))
                    {
                        tPort = this.GetPort((int)currentSlicePortNumber);

                        if (tPort == null)
                        {
                            if ((new List<string>(SerialPort.GetPortNames())).Contains(String.Format("COM{0}", currentSlicePortNumber)))
                            {
                                tPort = new Port((int)currentSlicePortNumber, (int)currentSliceBaudrate,
                                                    currentSliceParity, Convert.ToInt32(currentSliceDatabits.Replace("Bits","")), currentSliceStopbits.Replace("Bits",""),(int) currentReadBufferSize);

                                _Ports.Add(tPort);

                                currentReadBufferSize = (double)tPort.BufferSize;
                                FReadBufferOut.SetValue(i, currentReadBufferSize);
                            }
                        }
                        else
                        {
                            //read data from inputs
                            FDataIn.GetString(i, out currentSliceData);
                            FOnDataOut.SetValue(i, Convert.ToDouble(tPort.OnData));
                            FConnectedOut.SetValue(i, Convert.ToDouble(tPort.Connected));

                            // write data to comport
                            if (currentSliceDoSend == 1.0 && !String.IsNullOrEmpty(currentSliceData))
                                tPort.Write(currentSliceData);

                            //read data from comport
                            if (tPort.OnData)
                                FDataOut.SetString(i, tPort.Read());
                            else
                            {
                                if (Convert.ToBoolean(currentSliceKeepLastData))
                                    FDataOut.SetString(i, tPort.Data);
                                else
                                    FDataOut.SetString(i, "");
                            }


                            if (FBaudrateIn.PinIsChanged)
                                tPort.Baudrate = (int)currentSliceBaudrate;

                            if (FParityIn.PinIsChanged)
                                tPort.Parity = currentSliceParity;

                            if (FDatabitsIn.PinIsChanged)
                                tPort.DataBits = Convert.ToInt32(currentSliceDatabits);

                            if (FStopbitsIn.PinIsChanged)
                                tPort.StopBits = currentSliceStopbits;

                            if (FHandShakeIn.PinIsChanged)
                                tPort.HandShake = currentSliceHandShake;
                            if (FReadBufferIn.PinIsChanged)
                            {
                                tPort.Dispose();
                                _Ports.Remove(tPort);
                            }
                        }
                    }
                    else
                    {
                        tPort = this.GetPort((int)currentSlicePortNumber);

                        if (tPort != null)
                        {
                            tPort.Dispose();
                            _Ports.Remove(tPort);
                        }

                        FOnDataOut.SetValue(i, Convert.ToDouble(0));
                        FConnectedOut.SetValue(i, Convert.ToDouble(0));
                        FDataOut.SetString(i, "");
                    }
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, ex.Message);
            }
        }

        private Port GetPort(int pPortNumber)
        {
            return _Ports.Find(delegate(Port P) { return P.PortNumber == pPortNumber; });
        }
    }




    /// <summary>
    /// Wrapper for RS232 SerialPort Class.
    /// </summary>
    public class Port : IDisposable
    {
        private SerialPort _Port;

        private string _Data = "";

        public int PortNumber { get { return Convert.ToInt32(_Port.PortName.Substring(3)); } }
        public bool OnData { get { return _Port == null ? false : _Port.BytesToRead > 0; } }
        public string Data { get { return _Data; } }
        public bool Connected { get { return _Port.IsOpen; } }

        public int Baudrate { get { return _Port.BaudRate; } set { _Port.BaudRate = value; } }
        public string Parity { get { return _Port.Parity.ToString(); } set { _Port.Parity = GetParity(value); } }
        public int DataBits { get { return _Port.DataBits; } set { _Port.DataBits = value; } }
        public string StopBits { get { return _Port.StopBits.ToString(); } set { _Port.StopBits = GetStopbits(value); } }
        public string HandShake { get { return _Port.Handshake.ToString(); } set { _Port.Handshake = GetHandShake(value); } }


        public Port(int pNumber, int pBaudrate, string pParity, int pDataBits, string pStopBits, int BufferSize)
        {
            _Port = new SerialPort("COM" + (pNumber).ToString(), pBaudrate, GetParity(pParity), pDataBits, GetStopbits(pStopBits));
            _Port.Encoding = System.Text.Encoding.Default;
            _Port.ReadTimeout = 500;
            _Port.WriteTimeout = 500;
            _Port.ReadBufferSize = BufferSize;

            try
            {
                _Port.Open();
            }
            catch (Exception ex)
            {

            }

        }

        public void Dispose()
        {
            if (_Port.IsOpen)
                _Port.Close();

            _Port = null;
        }

        public void Write(string tData)
        {
            if (!_Port.IsOpen)
                return;

            try
            {
                _Port.Write(tData);
            }
            catch (TimeoutException)
            { }
        }

        public string Read()
        {
            _Data = _Port.ReadExisting();
            return _Data;
        }

        public int BufferSize
        {
            get
            {
                return _Port.ReadBufferSize;
            }
        }


        private Parity GetParity(string pParity)
        {
            switch (pParity)
            {
                case "Even": return System.IO.Ports.Parity.Even;
                case "Mark": return System.IO.Ports.Parity.Mark;
                case "None": return System.IO.Ports.Parity.None;
                case "Odd": return System.IO.Ports.Parity.Odd;
                case "Space": return System.IO.Ports.Parity.Space;
            }

            return System.IO.Ports.Parity.None;
        }

        private StopBits GetStopbits(string pStopBits)
        {
            switch (pStopBits)
            {
                case "1": return System.IO.Ports.StopBits.One;
                case "1.5": return System.IO.Ports.StopBits.OnePointFive;
                case "2": return System.IO.Ports.StopBits.Two;
            }

            return System.IO.Ports.StopBits.None;
        }

        private Handshake GetHandShake(string pStopBits)
        {
            switch (pStopBits)
            {
                case "None": return System.IO.Ports.Handshake.None;
                case "RequestToSend": return System.IO.Ports.Handshake.RequestToSend;
                case "RequestToSendXOnXOff": return System.IO.Ports.Handshake.RequestToSendXOnXOff;
                case "XOnXOff": return System.IO.Ports.Handshake.XOnXOff;
            }

            return System.IO.Ports.Handshake.None;
        }
    }
}
