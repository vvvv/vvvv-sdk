using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib.Enums;
using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes
{
    [PluginInfo(Name="MidiTest",Category="Devices")]
    public class MidiTestNode : IPluginEvaluate
    {
        [Input("Input Device", EnumName = TodoMidiEnumManager.MIDI_INPUT_ENUM_NAME)]
        ISpread<EnumEntry> FInMidiDeviceIn;

        [Input("Input Device Enabled")]
        IDiffSpread<bool> FInMidiInEnabled;

        [Input("Output Device", EnumName = TodoMidiEnumManager.MIDI_OUTPUT_ENUM_NAME)]
        IDiffSpread<EnumEntry> FInMidiDeviceOut;

        [Input("Refresh Midi Devices",IsBang=true,IsSingle=true)]
        ISpread<bool> FInRefreshMidi;

        [Output("Device Active")]
        ISpread<bool> FOutMidiActive;

        IPluginHost FHost;

        [ImportingConstructor()]
        public MidiTestNode(IPluginHost host)
        {
            this.FHost = host;
            TodoMidiEnumManager.RefreshInputDevices(this.FHost);
            TodoMidiEnumManager.RefreshOutputDevices(this.FHost);
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInRefreshMidi[0])
            {
                TodoMidiEnumManager.RefreshInputDevices(this.FHost);
                TodoMidiEnumManager.RefreshOutputDevices(this.FHost);
            }

            if (this.FInRefreshMidi[0] || this.FInMidiInEnabled.IsChanged)
            {

            }

        }
    }
}
