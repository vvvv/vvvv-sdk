using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    /// <summary>
    /// Delegate for when variable changes value
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="newvalue">New Value</param>
    public delegate void TodoVariableChangedDelegate(string name, double newvalue);

    /// <summary>
    /// Delegate when new variable is registered
    /// </summary>
    /// <param name="var">New Variable instance</param>
    public delegate void TodoVariableRegisteredDelegate(TodoVariable var);

    /// <summary>
    /// Variable class, contains all required data
    /// </summary>
    public class TodoVariable
    {
        public string Name { get; private set; }
        public TodoCategory Category { get; private set; }
       
        public double Default { get; set; }
        public bool ShowGui { get; set; }
        public bool AllowFeedBack { get; set; }
        public TodoTweenMapper Mapper { get; set; }

        public List<AbstractTodoInput> Inputs { get; set; }

        public event TodoVariableChangedDelegate ValueChanged;

        public TodoVariable()
        {
            this.Mapper = new TodoTweenMapper();
            this.Inputs = new List<AbstractTodoInput>();
        }

        private double val;

        public void Reset()
        {
            this.val = this.Default;
        }

        protected void OnValueChanged()
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this.Name, this.Value);
            }
        }

        public double ValueRaw
        {
            get { return this.val; }
        }

        public double Value
        {
            get { return this.Mapper.GetValue(this.val); }
        }
    }
}
