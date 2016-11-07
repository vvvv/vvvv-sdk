using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.Lib.Modules.Midi
{

    public class TodoMidiInput : AbstractTodoInput
    {
        public int MidiChannel { get; set; }
        public eTodoMidiType ControlType { get; set; }
        public int ControlValue { get; set; }
        public override string Device { get; set; }

        public TodoMidiInput() : base() {}


        private TodoMidiDevice dev;

        public void SetDevice(TodoMidiDevice dev)
        {
            this.dev = dev;
        }


        public override string InputType
        {
            get { return "Midi"; }
        }

        public override string InputMap
        {
            get
            {

                string ctrl = "";
                   
                ctrl = " CH : " + this.MidiChannel.ToString();

                eTodoMidiType type = this.ControlType;
                if (type == eTodoMidiType.Controller)
                {
                    ctrl += " : " + "CC";
                }
                if (type == eTodoMidiType.Note)
                {
                    ctrl += " : " + "Note";
                }

                ctrl += " : " + this.ControlValue;

                return ctrl;
            }
        }

        public TodoMidiInput(TodoVariable var)
            : base(var)
        {

        }

        /*
        public void UpdateValue(double val)
        {
            this.Variable.SetValue(this, val);
        }*/
    }
}

