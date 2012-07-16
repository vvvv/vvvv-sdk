using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

namespace VVVV.TodoMap.Lib.Modules.Midi
{
    public class MidiDevice
    {
        /// <summary>
        /// Device Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Device Index (if more than two with the same name)
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Device Index (In midi driver device list), -1 if doesnt not exist
        /// </summary>
        public int DeviceIndex { get; set; }

        /// <summary>
        /// Device Status
        /// </summary>
        public eTodoMidiStatus Status { get; set; }

        /// <summary>
        /// Warm starting for device
        /// </summary>
        public bool AutoStart { get; set; }
    }

    public class MidiDeviceList : List<MidiDevice>
    {
        public bool Contains(string name, int index)
        {
            foreach (MidiDevice dev in this)
            {
                if (dev.Name == name && dev.Index == index)
                {
                    return true;
                }
            }
            return false;
        }

        public MidiDevice GetDevice(string name, int index)
        {
            foreach (MidiDevice dev in this)
            {
                if (dev.Name == name && dev.Index == index)
                {
                    return dev;
                }
            }
            return null;
        }

        public int GetIndex(string name)
        {
            int idx = -1;
            foreach (MidiDevice dev in this)
            {
                if (dev.Name == name)
                {
                    idx++;
                }
            }
            return idx;
        }

    }

    public class TodoMidiDeviceManager
    {
        private MidiDeviceList inputs = new MidiDeviceList();

        private Dictionary<int, InputDevice> inputdevs = new Dictionary<int, InputDevice>();


        public void AddDevice(string name, int index,bool autostart)
        {
           
        }

        public void ReScan()
        {
            MidiDeviceList mdlinput = new MidiDeviceList();

            //Relist devices
            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                MidiDevice md = new MidiDevice();
                md.Name = InputDevice.GetDeviceCapabilities(i).name;
                md.DeviceIndex = i;
                md.Index = mdlinput.GetIndex(md.Name);
                mdlinput.Add(md);
            }

            MidiDeviceList newinput = new MidiDeviceList();

            //Now cross match
            foreach (MidiDevice input in this.inputs)
            {
                //Device been disconnected
                if (!mdlinput.Contains(input.Name, input.Index))
                {
                    input.DeviceIndex = -1;
                    input.Status = eTodoMidiStatus.Disconnected;
                    newinput.Add(input);
                }
            }

            foreach (MidiDevice input in mdlinput)
            {

            }
        }

        private int CountInputByName(string name)
        {
            int result = 0;
            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                if (InputDevice.GetDeviceCapabilities(i).name == name) { result++; }
            }

            return result;
        }
    }
}
