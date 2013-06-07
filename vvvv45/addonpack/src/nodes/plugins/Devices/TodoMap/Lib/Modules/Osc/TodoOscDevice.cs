using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using VVVV.Utils.OSC;

namespace VVVV.TodoMap.Lib.Modules.Osc
{
    public enum eTodoOscStatus { Idle,Started,Error }

    public delegate void OscStatusChangedDelegate(eTodoOscStatus status);
    public delegate void OscOutputStatusChangedDelegate(bool enabled);
    public delegate void OscReceivedDelegate(OSCMessage msg);
    

    public class TodoOscDevice : AbstractTodoDevice<TodoOscInput>
    {
        private int localport = 6666;
        private int remoteport = 7777;
        private string remoteip = "255.255.255.255";

        private Thread thr;

        private eTodoOscStatus inputStatus = eTodoOscStatus.Idle;
        private bool enableOutput = false;

        private OSCReceiver receiver;

        public bool AutoStartInput { get; set; }
        public bool AutoStartOutput { get; set; }

        public event OscStatusChangedDelegate OscInputStatusChanged;
        public event OscOutputStatusChangedDelegate OscOutputStatusChanged;
        public event OscReceivedDelegate OscDataReceived;

        private void ChangeInputStatus(eTodoOscStatus status)
        {
            if (this.OscInputStatusChanged != null)
            {
                this.OscInputStatusChanged(status);
            }
        }

        public TodoOscDevice(TodoEngine engine) : base(engine)
		{

		}

        public int LocalPort
        {
            get { return this.localport; }
            set 
            {            
                if (this.inputStatus != eTodoOscStatus.Started)
                {
                    this.localport = value;
                }
            }
        }

        public int RemotePort
        {
            get { return this.remoteport; }
            set
            {
                if (!this.enableOutput)
                {
                    this.remoteport = value;
                }
            }
        }

        public eTodoOscStatus LocalStatus
        {
            get { return this.inputStatus; }
        }

        public bool RemoteEnabled
        {
            get { return this.enableOutput; }
        }

        public List<string> IgnoreList
        {
            get;
            set;
        }

     

        #region Enable/Disable
        public void SetEnabled(bool enabled)
        {
            if (enabled)
            {
                if (this.inputStatus == eTodoOscStatus.Idle || this.inputStatus == eTodoOscStatus.Error && enabled)
                {
                    try
                    {
                        this.receiver = new OSCReceiver(this.localport);
                        this.Start();
                        this.inputStatus = eTodoOscStatus.Started;
                        this.ChangeInputStatus(this.inputStatus);
                    }
                    catch
                    {
                        this.inputStatus = eTodoOscStatus.Error;
                        this.ChangeInputStatus(this.inputStatus);
                    }
                }
            }
            else
            {
                if (this.inputStatus == eTodoOscStatus.Started)
                {
                    this.Stop();
                    this.inputStatus = eTodoOscStatus.Idle;
                    this.ChangeInputStatus(this.inputStatus);

                }
            }
        }

        public void SetOutputEnabled(bool enabled)
        {
            if (enabled != this.enableOutput)
            {
                this.enableOutput = enabled;
                if (this.OscOutputStatusChanged != null)
                {
                    this.OscOutputStatusChanged(enabled);
                }
            }
        }
        #endregion

        private void Start()
        {
            this.thr = new Thread(new ThreadStart(this.Run));
            this.thr.Start();
        }

        private void Run()
        {
            while (true)
            {
                OSCPacket bundle = this.receiver.Receive();
                this.ProcessPacket(bundle);
            }
        }

        private void ProcessPacket(OSCPacket packet)
        {
            if (packet.IsBundle())
            {
                OSCBundle bundle = packet as OSCBundle;
                foreach (object o in bundle.Values)
                {
                    this.ProcessPacket(o as OSCPacket);
                }
            }
            else
            {
                this.ProcessMessage(packet as OSCMessage);
            }
        }

        private void Stop()
        {
            try
            {
                if (thr != null) { thr.Abort(); }
                
            }
            catch
            {

            }

            try
            {
                this.receiver.Close();
            }
            catch
            {

            }
        }

        public void Dispose()
        {
            this.Stop();

        }

        private void ProcessMessage(OSCMessage msg)
        {
            if (!this.IgnoreList.Contains(msg.Address))
            {
                if (this.engine.LearnMode && this.engine.SelectedVariable != null)
                {
                    TodoOscInput input = null;
                    bool isnew = false;
                    bool found = false;

                    foreach (AbstractTodoInput ainput in this.engine.SelectedVariable.Inputs)
                    {
                        if (ainput is TodoOscInput)
                        {
                            TodoOscInput osc = ainput as TodoOscInput;

                            if (osc.Message == msg.Address)
                            {
                                input = osc;
                                found = true;
                            }
                        }
                    }

                    if (!found)
                    {

                        if (this.engine.SelectedInput == null)
                        {
                            input = new TodoOscInput(this.engine.SelectedVariable);
                            this.inputvars.Add(input);
                            isnew = true;
                        }
                        else
                        {
                            if (this.engine.SelectedInput is TodoOscInput)
                            {
                                input = (TodoOscInput)this.engine.SelectedInput;
                            }
                            else
                            {
                                input = new TodoOscInput(this.engine.SelectedVariable);
                                this.inputvars.Add(input);
                                isnew = true;
                            }
                        }
                    }
                    this.engine.SelectInput(input);

                    input.Message = msg.Address;

                    this.engine.VarriableMappingAdded(input, isnew);
                }

                if (!this.engine.LearnMode)
                {
                    foreach (TodoOscInput toi in this.inputvars)
                    {
                        if (toi.Message == msg.Address)
                        {
                            double dblval = Convert.ToDouble(msg.Values[0]);
                            toi.UpdateValue(dblval);
                        }
                    }
                }
            }

            if (this.OscDataReceived != null)
            {
                this.OscDataReceived(msg);
            }
        }

        protected override void DoFeedBack(TodoVariable var, TodoOscInput source)
        {
            if (this.enableOutput)
            {
                OSCMessage msg = new OSCMessage(source.Message);
                msg.Append(Convert.ToSingle(var.Value));

                OSCTransmitter tr = new OSCTransmitter(this.remoteip, this.remoteport);
                tr.Send(msg);

            }        
        }
    }
}
