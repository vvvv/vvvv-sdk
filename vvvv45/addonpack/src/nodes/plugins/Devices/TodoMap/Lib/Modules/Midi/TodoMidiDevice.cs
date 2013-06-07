using System;
using System.Collections.Generic;
using System.Text;
using Sanford.Multimedia.Midi;
using VVVV.TodoMap.Lib.Utils;

namespace VVVV.TodoMap.Lib.Modules.Midi
{
    public enum eTodoMidiType { Controller, Note, Unknown }

    public enum eTodoMidiStatus { Connected, Disconnected, Started, Error }

    public delegate void DeviceStatusChangedDelegate(int index, eTodoMidiStatus status);
    public delegate void DeviceAutoChangedDelegate(int index, bool auto);
    public delegate void ClockDeviceChangedDelegate();
    public delegate void ClockChangedDelegate(int ticks);

    public class TodoMidiDevice : AbstractTodoDevice<TodoMidiInput>
    {
        private List<string> inputdevname = new List<string>();

        private List<eTodoMidiStatus> inputstatus = new List<eTodoMidiStatus>();
        private Dictionary<int, InputDevice> indevs = new Dictionary<int, InputDevice>();
        private int clockdevice = -1;
        private List<string> inputauto = new List<string>();

        private List<eTodoMidiStatus> outputstatus = new List<eTodoMidiStatus>();
        private Dictionary<int, OutputDevice> outdevs = new Dictionary<int, OutputDevice>();
        private List<string> outputdevname = new List<string>();
        private List<string> outputauto = new List<string>();

        private UsbDetector usb;
        private int FTicks = 0;

        public event DeviceStatusChangedDelegate DeviceInputStatusChanged;
        public event DeviceStatusChangedDelegate DeviceOutputStatusChanged;
        public event ClockChangedDelegate ClockValueChanged;
        public event ClockDeviceChangedDelegate ClockDeviceChanged;

        public event DeviceAutoChangedDelegate DeviceInputAutoChanged;
        public event DeviceAutoChangedDelegate DeviceOutputAutoChanged;

        #region Clock
        public bool ClockEnabled
        {
            get
            {
                if (this.clockdevice > -1)
                {
                    return this.inputstatus[this.clockdevice] == eTodoMidiStatus.Started;
                }
                else
                {
                    return false;
                }
            }
        }

        public double ClockTime
        {
            get
            {
                if (this.ClockEnabled)
                {
                    return Convert.ToDouble(this.FTicks) / 24.0;
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion

        #region Constructor
        public TodoMidiDevice(TodoEngine engine) : base(engine)
        {
            this.usb = new UsbDetector();
            this.usb.UsbRemoved += usb_UsbRemoved;
            this.usb.UsbAdded += usb_UsbAdded;
            this.usb.Start();


            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                this.inputstatus.Add(eTodoMidiStatus.Connected);
                this.inputdevname.Add(InputDevice.GetDeviceCapabilities(i).name);             
            }

            for (int i = 0; i < OutputDevice.DeviceCount; i++)
            {
                this.outputstatus.Add(eTodoMidiStatus.Connected);
                this.outputdevname.Add(OutputDevice.GetDeviceCapabilities(i).name);
            }
        }

        private void usb_UsbAdded(object sender, EventArgs e)
        {
            for (int i = 0; i < this.inputstatus.Count; i++)
            {
                if (this.inputstatus[i] == eTodoMidiStatus.Error || this.inputstatus[i] == eTodoMidiStatus.Disconnected)
                {
                    bool found = false;
                    for (int j = 0; j < InputDevice.DeviceCount; j++)
                    {
                        if (InputDevice.GetDeviceCapabilities(j).name == this.inputdevname[i])
                        {
                            found = true;
                        }
                    }

                    if (found)
                    {
                        this.inputstatus[i] = eTodoMidiStatus.Connected;
                        this.OnMidiInputStatusChange(i, eTodoMidiStatus.Connected);
                    }
                }
            }

            for (int i = 0; i < this.outputstatus.Count; i++)
            {
                if (this.outputstatus[i] == eTodoMidiStatus.Error || this.outputstatus[i] == eTodoMidiStatus.Disconnected)
                {
                    bool found = false;
                    for (int j = 0; j < OutputDevice.DeviceCount; j++)
                    {
                        if (OutputDevice.GetDeviceCapabilities(j).name == this.outputdevname[i])
                        {
                            found = true;
                        }
                    }

                    if (found)
                    {
                        this.outputstatus[i] = eTodoMidiStatus.Connected;
                        this.OnMidiOutputStatusChange(i, eTodoMidiStatus.Connected);
                    }
                }

            }  
        }

        private void usb_UsbRemoved(object sender, EventArgs e)
        {
            for (int i = 0; i < this.inputstatus.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < InputDevice.DeviceCount; j++)
                {
                    if (InputDevice.GetDeviceCapabilities(j).name == this.inputdevname[i])
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    this.inputstatus[i] = eTodoMidiStatus.Disconnected;
                    this.OnMidiInputStatusChange(i, eTodoMidiStatus.Disconnected);
                    this.indevs[i].ChannelMessageReceived -= dev_ChannelMessageReceived;
                    this.indevs.Remove(i);
                }
            }

            for (int i = 0; i < this.outputstatus.Count; i++)
            {
                bool found = false;
                for (int j = 0; j < OutputDevice.DeviceCount; j++)
                {
                    if (OutputDevice.GetDeviceCapabilities(j).name == this.outputdevname[i])
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    this.outputstatus[i] = eTodoMidiStatus.Disconnected;
                    this.OnMidiOutputStatusChange(i, eTodoMidiStatus.Disconnected);
                    this.outdevs.Remove(i);
                }
            } 
        }

        #endregion

        #region Status Change
        private void OnMidiInputStatusChange(int index, eTodoMidiStatus status)
        {
            if (this.DeviceInputStatusChanged != null)
            {
                this.DeviceInputStatusChanged(index, status);
            }
        }

        private void OnMidiOutputStatusChange(int index, eTodoMidiStatus status)
        {
            if (this.DeviceOutputStatusChanged != null)
            {
                this.DeviceOutputStatusChanged(index, status);
            }
        }

        private void OnMidiInputAutoChange(int index, bool auto)
        {
            if (this.DeviceInputAutoChanged != null)
            {
                this.DeviceInputAutoChanged(index, auto);
            }
        }

        private void OnMidiOutputAutoChange(int index, bool auto)
        {
            if (this.DeviceOutputAutoChanged != null)
            {
                this.DeviceOutputAutoChanged(index, auto);
            }
        }
        #endregion

        #region Auto Start

        public List<string> InputAuto { get { return this.inputauto; } }
        public List<string> OutputAuto { get { return this.outputauto; } }

        public void SetInputAutoStart(int idx,bool auto)
        {
            string devname = this.inputdevname[idx];
            if (auto)
            {
                if (!this.inputauto.Contains(devname)) { this.inputauto.Add(devname); }
            }
            else
            {
                if (this.inputauto.Contains(devname)) { this.inputauto.Remove(devname); }
            }
        }

        public void SetOutputAutoStart(int idx, bool auto)
        {
            string devname = this.outputdevname[idx];
            if (auto)
            {
                if (!this.outputauto.Contains(devname)) { this.outputauto.Add(devname); }
            }
            else
            {
                if (this.outputauto.Contains(devname)) { this.outputauto.Remove(devname); }
            }
        }

        public void SetInputAutoStart(string devname, bool auto)
        {
            if (auto)
            {
                if (!this.inputauto.Contains(devname)) { this.inputauto.Add(devname); }
            }
            else
            {
                if (this.inputauto.Contains(devname)) { this.inputauto.Remove(devname); }
            }
            this.OnMidiInputAutoChange(this.inputdevname.IndexOf(devname), auto);


            int idx = this.inputdevname.IndexOf(devname);

            if (idx > -1)
            {
                if (this.inputstatus[idx] == eTodoMidiStatus.Connected)
                {
                    this.SetInputEnabled(idx, true);
                }
                this.OnMidiInputAutoChange(idx, auto);
            }
        }

        public void SetOutputAutoStart(string devname, bool auto)
        {
            if (auto)
            {
                if (!this.outputauto.Contains(devname)) { this.outputauto.Add(devname); }
            }
            else
            {
                if (this.outputauto.Contains(devname)) { this.outputauto.Remove(devname); }
            }

            int idx = this.outputdevname.IndexOf(devname);

            if (idx > -1)
            {
                if (this.outputstatus[idx] == eTodoMidiStatus.Connected)
                {
                    this.SetOutputEnabled(idx, true);
                }
                this.OnMidiOutputAutoChange(idx, auto);
            }

            

            
        }
        #endregion

        #region Set Input Enabled
        public void SetInputEnabled(int idx,bool enabled)
        {
            if (enabled)
            {
                if (this.inputstatus[idx] == eTodoMidiStatus.Connected ||
                    this.inputstatus[idx] == eTodoMidiStatus.Error)
                {
                    try
                    {
                        InputDevice dev = new InputDevice(idx);
                        dev.ChannelMessageReceived += dev_ChannelMessageReceived;
                        dev.StartRecording();

                        this.indevs[idx] = dev;
                        this.inputstatus[idx] = eTodoMidiStatus.Started;
                        this.OnMidiInputStatusChange(idx, eTodoMidiStatus.Started);
                    }
                    catch (Exception ex)
                    {
                        this.inputstatus[idx] = eTodoMidiStatus.Error;
                        this.OnMidiInputStatusChange(idx, eTodoMidiStatus.Error);
                    }
                }
            }
            else
            {
                if (this.inputstatus[idx] == eTodoMidiStatus.Started)
                {
                    try
                    {
                        this.indevs[idx].ChannelMessageReceived -= dev_ChannelMessageReceived;
                        this.indevs[idx].StopRecording();
                        //this.indevs[idx].Close();
                        //this.indevs[idx].Dispose();
                        this.inputstatus[idx] = eTodoMidiStatus.Connected;
                        this.OnMidiInputStatusChange(idx, eTodoMidiStatus.Connected);
                    }
                    catch
                    {
                        this.inputstatus[idx] = eTodoMidiStatus.Error;
                        this.OnMidiInputStatusChange(idx, eTodoMidiStatus.Error);
                    }
                    this.indevs.Remove(idx);

                }
            }
        }
        #endregion

        #region Set output Enabled
        public void SetOutputEnabled(int idx, bool enabled)
        {
            if (enabled)
            {
                if (this.outputstatus[idx] == eTodoMidiStatus.Connected ||
                    this.outputstatus[idx] == eTodoMidiStatus.Error)
                {
                    try
                    {
                        OutputDevice dev = new OutputDevice(idx);

                        this.outdevs[idx] = dev;
                        this.outputstatus[idx] = eTodoMidiStatus.Started;
                        this.OnMidiOutputStatusChange(idx, eTodoMidiStatus.Started);
                    }
                    catch
                    {
                        this.outputstatus[idx] = eTodoMidiStatus.Error;
                        this.OnMidiOutputStatusChange(idx, eTodoMidiStatus.Error);
                    }
                }
            }
            else
            {
                if (this.outputstatus[idx] == eTodoMidiStatus.Started)
                {
                    try
                    {
                        this.outdevs[idx].Close();
                        this.outdevs[idx].Dispose();
                        this.outputstatus[idx] = eTodoMidiStatus.Connected;
                        this.OnMidiOutputStatusChange(idx, eTodoMidiStatus.Connected);
                    }
                    catch
                    {
                        this.outputstatus[idx] = eTodoMidiStatus.Error;
                        this.OnMidiOutputStatusChange(idx, eTodoMidiStatus.Error);
                    }
                    this.outdevs.Remove(idx);

                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            foreach (InputDevice dev in this.indevs.Values)
            {
                try
                {
                    dev.StopRecording();
                    //try { dev.Dispose(); } catch {}
                }
                catch
                {

                }
            }

            foreach (OutputDevice dev in this.outdevs.Values)
            {
                try
                {
                    dev.Close();
                    try { dev.Dispose(); }
                    catch { }
                }
                catch
                {

                }
            }
        }
        #endregion

        #region Message Received
        private void dev_ChannelMessageReceived(object sender, ChannelMessageEventArgs e)
        {
             InputDevice dev = (InputDevice)sender;
             string devname = InputDevice.GetDeviceCapabilities(dev.DeviceID).name;

            #region Learn Mode
            if (this.engine.LearnMode && this.engine.SelectedVariable != null)
            {
                TodoMidiInput input = null;
                bool isnew = false;
                bool found = false;

                //Lookup to see if input already there
                eTodoMidiType controltype = eTodoMidiType.Unknown;
                if (e.Message.Command == ChannelCommand.Controller)
                {
                    controltype = eTodoMidiType.Controller;
                    
                }
                else
                {
                    if (e.Message.Command == ChannelCommand.NoteOn
                        || e.Message.Command == ChannelCommand.NoteOff)
                    {
                        controltype = eTodoMidiType.Note;
                    }
                }


                foreach (AbstractTodoInput ainput in this.engine.SelectedVariable.Inputs)
                {
                    if (ainput is TodoMidiInput)
                    {
                        TodoMidiInput midi = ainput as TodoMidiInput;

                        if (midi.ControlType == controltype
                            && midi.MidiChannel == e.Message.MidiChannel
                            && midi.ControlValue == e.Message.Data1
                            && midi.Device == devname)
                        {
                            input = midi;
                            found = true;
                        }
                    }
                }


                if (!found)
                {
                    if (this.engine.SelectedInput == null)
                    {
                        input = new TodoMidiInput(this.engine.SelectedVariable);
                        this.inputvars.Add(input);
                        isnew = true;
                    }
                    else
                    {
                        if (this.engine.SelectedInput is TodoMidiInput)
                        {
                            input = (TodoMidiInput)this.engine.SelectedInput;
                        }
                        else
                        {
                            input = new TodoMidiInput(this.engine.SelectedVariable);
                            this.inputvars.Add(input);
                            isnew = true;
                        }
                    }
                }
                this.engine.SelectInput(input);

                if (this.engine.AnyDevice)
                {
                    input.Device = "Any";
                }
                else
                {
                    input.Device = devname;
                }
                
                input.ControlType = controltype;
                input.ControlValue = e.Message.Data1;
                input.MidiChannel = e.Message.MidiChannel;

                this.engine.VarriableMappingAdded(input, isnew);
            }
            #endregion

            #region Set Value Mode
            if (!this.engine.LearnMode)
            {
                foreach (TodoMidiInput tmi in this.inputvars)
                {
                    eTodoMidiType type = eTodoMidiType.Unknown;
                    if (e.Message.Command == ChannelCommand.Controller)
                    {
                        type = eTodoMidiType.Controller;
                    }

                    double mul = 1.0;
                    if (e.Message.Command == ChannelCommand.NoteOn
                        || e.Message.Command == ChannelCommand.NoteOff)
                    {
                        type = eTodoMidiType.Note;

                        if (e.Message.Command == ChannelCommand.NoteOff)
                        {
                            mul = 0;
                        }
                    }

                    if (tmi.ControlType == type
                        && tmi.MidiChannel == e.Message.MidiChannel
                        && tmi.ControlValue == e.Message.Data1
                        && (tmi.Device == "Any" || tmi.Device == devname))
                    {
                        double dblval = ((double)e.Message.Data2 * mul) / 127.0;
                        tmi.UpdateValue(dblval);
                    }
                }
            }
            #endregion
        }
        #endregion

        #region Feedback
        protected override void DoFeedBack(TodoVariable var, TodoMidiInput source)
        {
            for (int i = 0; i < this.outputstatus.Count; i++)
            {
                if (this.outputstatus[i] == eTodoMidiStatus.Started)
                {
                    try
                    {
                        OutputDevice dev = this.outdevs[i];
                        ChannelCommand cmd = ChannelCommand.Controller;
                        if (source.ControlType == eTodoMidiType.Controller)
                        {
                            cmd = ChannelCommand.Controller;
                        }
                        else
                        {
                            if (source.ControlType == eTodoMidiType.Note)
                            {
                                if (var.ValueRaw == 0.0)
                                {
                                    cmd = ChannelCommand.NoteOff;
                                }
                                else
                                {
                                    cmd = ChannelCommand.NoteOn;
                                }
                            }
                        }

                        ChannelMessage msg = new ChannelMessage(cmd, source.MidiChannel, source.ControlValue, Convert.ToInt32(var.ValueRaw * 127.0));
                        dev.Send(msg);
                    }
                    catch
                    {

                    }
                }
            }          
        }
        #endregion

        #region Feedback
        public void CustomFeedBack(string devname,TodoVariable var, double value)
        {
            int idx = this.outputdevname.IndexOf(devname);

            //Need to find device and have it started
            if (idx > -1)
            {
                if (this.outputstatus[idx] == eTodoMidiStatus.Started)
                {
                    try
                    {
                        OutputDevice dev = this.outdevs[idx];
                        foreach (AbstractTodoInput ti in var.Inputs)
                        {
                            if (ti is TodoMidiInput)
                            {
                                TodoMidiInput mi = (TodoMidiInput)ti;

                                if (mi.Device == devname || mi.Device == "Any")
                                {

                                    ChannelCommand cmd = ChannelCommand.Controller;
                                    if (mi.ControlType == eTodoMidiType.Controller)
                                    {
                                        cmd = ChannelCommand.Controller;
                                    }
                                    else
                                    {
                                        if (mi.ControlType == eTodoMidiType.Note)
                                        {
                                            if (value == 0.0)
                                            {
                                                cmd = ChannelCommand.NoteOff;
                                            }
                                            else
                                            {
                                                cmd = ChannelCommand.NoteOn;
                                            }
                                        }
                                    }
                                    ChannelMessage msg = new ChannelMessage(cmd, mi.MidiChannel, mi.ControlValue, Convert.ToInt32(value * 127.0));
                                    dev.Send(msg);

                                }
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
        }
        #endregion

        #region Clock
        public void SetClockDevice(int index)
        {
            if (this.indevs.ContainsKey(this.clockdevice))
            {
                this.indevs[this.clockdevice].SysRealtimeMessageReceived -= TodoMidiDevice_SysRealtimeMessageReceived;
            }
            this.clockdevice = index;
            if (this.indevs.ContainsKey(this.clockdevice))
            {
                this.indevs[index].SysRealtimeMessageReceived += TodoMidiDevice_SysRealtimeMessageReceived;
            }

            if (this.ClockDeviceChanged != null)
            {
                this.ClockDeviceChanged();
            }
        }

        private void TodoMidiDevice_SysRealtimeMessageReceived(object sender, SysRealtimeMessageEventArgs e)
        {
            
            if (e.Message.SysRealtimeType == SysRealtimeType.Start)
            {
                this.FTicks = 0;
            }
            if (e.Message.SysRealtimeType == SysRealtimeType.Reset)
            {
                this.FTicks = 0;
            }
            if (e.Message.SysRealtimeType == SysRealtimeType.Clock)
            {
                this.FTicks++;
            }

            if (ClockValueChanged != null)
            {
                this.ClockValueChanged(this.FTicks);
            }
        }
        #endregion
    }
}
