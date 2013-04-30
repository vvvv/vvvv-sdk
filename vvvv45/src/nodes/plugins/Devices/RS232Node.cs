using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Diagnostics;
using System.IO;
using System.ComponentModel.Composition;
using System.Linq;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Devices
{
    [PluginInfo(Name = "RS232", Category = "Devices", AutoEvaluate = true)]
    public class Rs232Node : IDisposable, IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        const string COM_PORT_ENUM_NAME = "Rs232Node.ComPort";

        [Input("Input")]
        public ISpread<Stream> DataIn;
        [Input("Do Send", IsBang = true)]
        public ISpread<bool> DoSendIn;
        [Input("Baudrate", MinValue = 9600, MaxValue = int.MaxValue, DefaultValue = 115200)]
        public ISpread<int> BaudRateIn;
        [Input("Data Bits", MinValue = 5, MaxValue = 8, DefaultValue = 8)]
        public ISpread<int> DataBitsIn;
        [Input("Stop Bits", DefaultEnumEntry = "One")]
        public ISpread<StopBits> StopBitsIn;
        [Input("Parity")]
        public ISpread<Parity> ParityIn;
        [Input("Handshake")]
        public ISpread<Handshake> HandshakeIn;
        [Input("DTR Enable")]
        public ISpread<bool> DtrEnableIn;
        [Input("RTS Enable")]
        public ISpread<bool> RtsEnableIn;
        [Input("Break State")]
        public ISpread<bool> BreakStateIn;
        [Input("Update Port List", IsSingle=true, IsBang=true, Visibility = PinVisibility.Hidden)]
        public IDiffSpread<bool> UpdatePortListIn;
        [Input("Port Name", EnumName = COM_PORT_ENUM_NAME)]
        public ISpread<EnumEntry> ComPortIn;
        [Input("Enabled")]
        public ISpread<bool> EnabledIn;

        [Output("Output")]
        public ISpread<Stream> DataOut;
        [Output("DSR State")]
        public ISpread<bool> DsrStateOut;
        [Output("DCD State")]
        public ISpread<bool> DcdStateOut;
        [Output("CTS State")]
        public ISpread<bool> CtsStateOut;
        [Output("RI State")]
        public ISpread<bool> RiStateOut;
        [Output("Break State")]
        public ISpread<bool> BreakStateOut;
        [Output("On Data", IsBang = true)]
        public ISpread<bool> OnDataOut;
        [Output("Connected")]
        public ISpread<bool> ConnectedOut;

        private readonly Spread<SerialPort> FPorts = new Spread<SerialPort>();

        static Rs232Node()
        {
            UpdatePortList();
        }

        static void UpdatePortList()
        {
            var portNames = SerialPort.GetPortNames()
                .Where(n => n.StartsWith("com", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            EnumManager.UpdateEnum(COM_PORT_ENUM_NAME, portNames.Length > 0 ? portNames[0] : string.Empty, portNames);
        }

        public void OnImportsSatisfied()
        {
            DataOut.SliceCount = 0;
        }

        public void Dispose()
        {
            FPorts.ResizeAndDispose(0);
        }

        public void Evaluate(int spreadMax)
        {
            DcdStateOut.SliceCount = spreadMax;
            CtsStateOut.SliceCount = spreadMax;
            DsrStateOut.SliceCount = spreadMax;
            RiStateOut.SliceCount = spreadMax;
            BreakStateOut.SliceCount = spreadMax;

            if (UpdatePortListIn.IsChanged && UpdatePortListIn[0]) UpdatePortList();

            FPorts.Resize(spreadMax, CreatePort, DestroyPort);
            DataOut.ResizeAndDispose(spreadMax, () => new MemoryStream());

            for (int i = 0; i < spreadMax; i++)
            {
                var port = FPorts[i];
                // Configure the port
                var portName = ComPortIn[i].Name;
                if (port.PortName != portName)
                    port.PortName = portName;
                if (port.BaudRate != BaudRateIn[i])
                    port.BaudRate = BaudRateIn[i];
                if (port.DataBits != DataBitsIn[i])
                    port.DataBits = DataBitsIn[i];
                if (port.StopBits != StopBitsIn[i])
                    port.StopBits = StopBitsIn[i];
                if (port.Parity != ParityIn[i])
                    port.Parity = ParityIn[i];
                if (port.Handshake != HandshakeIn[i])
                    port.Handshake = HandshakeIn[i];
                if (port.DtrEnable != DtrEnableIn[i])
                    port.DtrEnable = DtrEnableIn[i];
                if (port.RtsEnable != RtsEnableIn[i])
                    port.RtsEnable = RtsEnableIn[i];

                // Get the in and output streams
                var dataIn = DataIn[i];
                var dataOut = DataOut[i];
                // Set stream positions back to the beginning
                dataIn.Seek(0, SeekOrigin.Begin);
                dataOut.Seek(0, SeekOrigin.Begin);

                if (EnabledIn[i])
                {
                    // Open the port
                    if (!port.IsOpen)
                    {
                        port.Open();
                        SetStates(i);
                    }
                    // Set the break state
                    port.BreakState = BreakStateIn[i];

                    // Write data to the port
                    var totalBytesToWrite = dataIn.Length;
                    if (totalBytesToWrite > 0 && DoSendIn[i])
                    {
                        var buffer = new byte[1024];
                        while (totalBytesToWrite > 0)
                        {
                            var bytesToWrite = (int)Math.Min(buffer.Length, totalBytesToWrite);
                            var bytesRead = dataIn.Read(buffer, 0, bytesToWrite);
                            port.Write(buffer, 0, bytesRead);
                            totalBytesToWrite -= bytesRead;
                        }
                    }

                    // Read data from the port
                    var totalBytesToRead = port.BytesToRead;
                    if (totalBytesToRead > 0)
                    {
                        dataOut.SetLength(totalBytesToRead);
                        var buffer = new byte[1024];
                        while (totalBytesToRead > 0)
                        {
                            var bytesToRead = Math.Min(buffer.Length, totalBytesToRead);
                            var bytesRead = port.Read(buffer, 0, bytesToRead);
                            dataOut.Write(buffer, 0, bytesRead);
                            totalBytesToRead -= bytesRead;
                        }
                        // Marks the pin as changed
                        DataOut[i] = dataOut;
                        // Set the OnData flag
                        OnDataOut[i] = true;
                    }
                    else
                    {
                        // Clear output
                        if (dataOut.Length > 0)
                        {
                            dataOut.SetLength(0);
                            // Marks the pin as changed
                            DataOut[i] = dataOut;
                            // Reset the OnData flag
                            OnDataOut[i] = false;
                        }
                    }
                }
                else
                {
                    // Close the port
                    if (port.IsOpen)
                    {
                        port.Close();
                        UnsetStates(i);
                    }
                    // Clear output
                    if (dataOut.Length > 0)
                    {
                        dataOut.SetLength(0);
                        // Marks the pin as changed
                        DataOut[i] = dataOut;
                        // Reset the OnData flag
                        OnDataOut[i] = false;
                    }
                }

                // Read connection state
                ConnectedOut[i] = port.IsOpen;
            }
        }

        SerialPort CreatePort(int slice)
        {
            var port = new SerialPort();
            port.PinChanged += HandlePinChanged;
            return port;
        }

        void DestroyPort(SerialPort port)
        {
            port.PinChanged -= HandlePinChanged;
            port.Dispose();
        }

        void SetStates(int slice)
        {
            var port = FPorts[slice];
            BreakStateOut[slice] = port.BreakState;
            DcdStateOut[slice] = port.CDHolding;
            CtsStateOut[slice] = port.CtsHolding;
            DsrStateOut[slice] = port.DsrHolding;
            RiStateOut[slice] = false;
        }

        void UnsetStates(int slice)
        {
            BreakStateOut[slice] = false;
            DcdStateOut[slice] = false;
            CtsStateOut[slice] = false;
            DsrStateOut[slice] = false;
            RiStateOut[slice] = false;
        }

        void HandlePinChanged(object sender, SerialPinChangedEventArgs e)
        {
            var port = sender as SerialPort;
            var slice = FPorts.IndexOf(port);
            switch (e.EventType)
            {
                case SerialPinChange.Break:
                    BreakStateOut[slice] = !BreakStateOut[slice];
                    break;
                case SerialPinChange.CDChanged:
                    DcdStateOut[slice] = !DcdStateOut[slice];
                    break;
                case SerialPinChange.CtsChanged:
                    CtsStateOut[slice] = !CtsStateOut[slice];
                    break;
                case SerialPinChange.DsrChanged:
                    DsrStateOut[slice] = !DsrStateOut[slice];
                    break;
                case SerialPinChange.Ring:
                    RiStateOut[slice] = !RiStateOut[slice];
                    break;
                default:
                    break;
            }
        }
    }
}