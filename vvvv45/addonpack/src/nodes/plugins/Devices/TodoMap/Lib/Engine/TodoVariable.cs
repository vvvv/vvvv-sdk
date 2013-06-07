using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    /// <summary>
    /// Delegate for when variable changes value
    /// </summary>
    /// <param name="name">Variable name</param>
    /// <param name="newvalue">New Value</param>
    public delegate void TodoVariableChangedDelegate(string name, double newvalue);

    public delegate void TodoVariableExtendedChangedDelegate(TodoVariable var, AbstractTodoInput source);

    /// <summary>
    /// Delegate when new variable is registered
    /// </summary>
    /// <param name="var">New Variable instance</param>
    public delegate void TodoVariableEventDelegate(TodoVariable var, bool gui);

    public delegate void TodoVariableCategoryChangedDelegate(TodoVariable var, string oldcat);

    /// <summary>
    /// Variable class, contains all required data
    /// </summary>
    public class TodoVariable
    {
        private string category;

        public string Name { get; set; }

       
        public double Default { get; set; }
        public bool ShowGui { get; set; } // For later
        public eTodoGlobalTakeOverMode TakeOverMode { get; set; }
        public bool AllowFeedBack { get; set; }

        public TodoTweenMapper Mapper { get; set; }

        public List<AbstractTodoInput> Inputs { get; set; }

        public AbstractTodoInput LastActiveControl { get; private set; }

        public event TodoVariableExtendedChangedDelegate ValueChanged;
        public event TodoVariableEventDelegate VariableUpdated;
        public event TodoVariableCategoryChangedDelegate VariableCategoryChanged;

        public TodoVariable()
        {
            //this.Name = name;
            this.Mapper = new TodoTweenMapper();
            this.Inputs = new List<AbstractTodoInput>();
            this.AllowFeedBack = true;
        }


        public TodoVariable(string name)
        {
            this.Name = name;
            this.Mapper = new TodoTweenMapper();
            this.Inputs = new List<AbstractTodoInput>();
            this.AllowFeedBack = true;
        }

        private double val;

        public string Category
        {
            get { return this.category; }
            set
            {
                string old = this.category;
                this.category = value;
                if (this.VariableCategoryChanged != null)
                {
                    this.VariableCategoryChanged(this, old);
                }
            }
        }


        public void Reset()
        {
            this.val = this.Default;
        }

        protected void OnValueChanged(AbstractTodoInput source)
        {
            if (this.ValueChanged != null)
            {
                this.ValueChanged(this, source);
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

        public void SetValue(AbstractTodoInput src, double value)
        {
            bool changed = this.val != value;
            this.val = value;
            if (changed)
            {
                this.LastActiveControl = src;
                this.OnValueChanged(src);
            }
        }

        public void SetDefault()
        {
            this.val = this.Default;
            this.OnValueChanged(null);
        }

        public void MarkForUpdate(bool gui)
        {
            if (this.VariableUpdated != null)
            {
                this.VariableUpdated(this,gui);
            }
        }

    }
}
