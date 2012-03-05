using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.Lib.Modules.Osc
{
    public class TodoOscInput : AbstractTodoInput
    {
        private TodoOscDevice dev;

        public TodoOscInput()
            : base()
        {

        }

        public void SetDevice(TodoOscDevice dev)
        {
            this.dev = dev;
        }

        public override string Device
        {
            get
            {
                return "Any";
            }
            set
            {
                //base.Device = value;
            }
        }


        public string Message { get; set; }

        public override string InputType
        {
            get { return "OSC"; }
        }

        public override string InputMap
        {
            get
            {
                return this.Message;
            }
        }

        public TodoOscInput(TodoVariable var)
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
