using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sanford.Multimedia.Midi;

namespace VVVV.TodoMap.Lib.Modules.Midi
{
    //Abstraction for a midi device, some can have same name so also provide an index
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
