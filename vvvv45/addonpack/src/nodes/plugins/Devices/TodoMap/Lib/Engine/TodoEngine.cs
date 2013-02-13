using System;
using System.Collections.Generic;
using System.Text;
using VVVV.TodoMap.Lib.Modules.Midi;
using VVVV.TodoMap.Lib.Modules.Osc;
using System.Linq;
using VVVV.TodoMap.Modules.OscTree;

namespace VVVV.TodoMap.Lib
{
    public class TodoEngine
    {
        //Dicitionary holding variables
        private Dictionary<string, TodoVariable> variables = new Dictionary<string, TodoVariable>();

        private TodoVariable selectedvar = null;
        private AbstractTodoInput selectedinput = null;

        private bool enabled;

        public event TodoVariableEventDelegate VariableRegistered;
        public event TodoVariableEventDelegate VariableDeleted;
        public event TodoVariableEventDelegate VariableChanged;
        public event TodoInputChangedEventDelegate VariableMappingChanged;
        
        public event TodoVariableChangedDelegate VariableValueChanged;
        public event TodoVariableExtendedChangedDelegate VariableValueChangedExtended;
        

        public event EventHandler OnReset;

        private TodoMidiDevice mididevice;
        private TodoOscDevice oscdevice;

        private TodoOscTreeController osctree;

        public string SavePath { get; set; }

        public TodoEngine()
        {
            this.enabled = false;
            this.selectedvar = null;
            this.LearnMode = false;
            this.AnyDevice = true;

            this.mididevice = new TodoMidiDevice(this);
            this.oscdevice = new TodoOscDevice(this);
            this.osctree = new TodoOscTreeController(this);
        }

        public TodoMidiDevice Midi
        {
            get { return this.mididevice; }
        }

        public TodoOscDevice Osc
        {
            get { return this.oscdevice; }
        }

        public bool Enabled
        {
            get { return this.enabled; }
            set { this.enabled = false; }
        }

        public bool LearnMode { get; set; }


        public bool AnyDevice { get; set; }

        public TodoVariable SelectedVariable
        {
            get { return this.selectedvar; }
        }

        public AbstractTodoInput SelectedInput
        {
            get { return this.selectedinput; }
        }

        public void SelectVariable(string name)
        {
            if (this.variables.ContainsKey(name))
            {
                this.selectedvar = this.variables[name];
                this.selectedinput = null;
            }
            else
            {
                this.selectedvar = null;
            }
        }

        public void SelectInput(int idx)
        {
            if (idx == -1)
            {
                this.selectedinput = null;
            }
            else
            {
                if (this.SelectedVariable != null)
                {
                    this.selectedinput = this.selectedvar.Inputs[idx];
                }
                else
                {

                }
            }
        }

        public void SelectInput(AbstractTodoInput input)
        {
            this.selectedinput = input;
            this.selectedvar = input.Variable;
        }

        public void DeselectVariable() 
        { 
            this.selectedvar = null;
            this.selectedinput = null;
        }

        public List<TodoVariable> Variables
        {
            get { return this.variables.Values.ToList(); }
        }

        public void RemoveInput(AbstractTodoInput input)
        {
            this.selectedinput = null;
            if (input is TodoMidiInput)
            {
                this.mididevice.RemoveInput(input as TodoMidiInput);
            }
            if (input is TodoOscInput)
            {
                this.oscdevice.RemoveInput(input as TodoOscInput);
            }
            input.Variable.Inputs.Remove(input);
        }

        public void RegisterVariable(TodoVariable var, bool gui)
        {
            if (!this.variables.ContainsKey(var.Name))
            {
                this.variables.Add(var.Name, var);
                var.ValueChanged += Variable_ValueChanged;
                var.VariableUpdated += var_VariableUpdated;
                if (this.VariableRegistered != null)
                {
                    this.VariableRegistered(var,gui);
                }
            }
        }

        void var_VariableUpdated(TodoVariable var, bool gui)
        {
            if (this.VariableChanged != null) { this.VariableChanged(var,gui); }
        }

        public void DeleteVariable(TodoVariable var,bool gui)
        {
            if (this.variables.ContainsKey(var.Name))
            {
                //Deselect if applicable
                if (this.SelectedVariable == var)
                {
                    this.DeselectVariable();
                }

                this.variables.Remove(var.Name);
                if (this.VariableDeleted != null)
                {
                    this.VariableDeleted(var,gui);
                }
            }
        }

        private void Variable_ValueChanged(TodoVariable var, AbstractTodoInput source)
        {
            if (this.VariableValueChanged != null)
            {
                this.VariableValueChanged(var.Name,var.Value);
            }

            if (this.VariableValueChangedExtended != null)
            {
                this.VariableValueChangedExtended(var, source);
            }

            if (var.AllowFeedBack)
            {
                this.Midi.FeedBack(var,source);
                this.Osc.FeedBack(var,source);
            }
        }

        public TodoVariable GetVariableByName(string name)
        {
            if (this.variables.ContainsKey(name))
            {
                return this.variables[name];
            }
            else
            {
                return null;
            }
        }

        public void VarriableMappingAdded(AbstractTodoInput input, bool isnew)
        {
            if (this.VariableMappingChanged != null)
            {
                //Auto select input
                this.selectedinput = input;
                this.VariableMappingChanged(input, isnew);
            }
        }

        public void Dispose()
        {
            this.mididevice.Dispose();
            this.oscdevice.Dispose();
        }

        public void ClearMappings()
        {
            this.Midi.ClearMappings();
            this.Osc.ClearMappings();
            foreach (TodoVariable v in this.variables.Values)
            {
                v.Inputs.Clear();
            }
            this.selectedinput = null;
        }

        public void ClearVariables()
        {
            this.ClearMappings();
            this.variables.Clear();

            if (this.OnReset != null)
            {
                this.OnReset(this, new EventArgs());
            }
        }
    }
}
