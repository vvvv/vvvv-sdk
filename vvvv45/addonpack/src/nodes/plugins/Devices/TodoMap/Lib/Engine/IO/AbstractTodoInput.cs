using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    public delegate void TodoInputChangedEventDelegate(AbstractTodoInput input, bool isnew);

    public abstract class AbstractTodoInput
    {
        private bool first = false;
        private double previousval = 0;
        private double epsilon = 0.001;

        public TodoVariable Variable { get; protected set; }

        public eTodoLocalTakeOverMode TakeOverMode { get; set; }
        public eTodoLocalFeedBackMode FeedBackMode { get; set; }

        protected string device;

        public abstract string InputType { get; }
        public abstract string InputMap { get; }

        public AbstractTodoInput()
        {
            this.device = "Any";
        }

        public virtual string Device
        {
            get { return this.device; }
            set { this.device = value; }
        }

        public void UpdateValue(double val)
        {
            if (this.first)
            {
                //Set value if takeover immediate
                this.first = false;
                if (this.GlobalTakeOverMode == eTodoGlobalTakeOverMode.Immediate)
                {
                    this.Variable.SetValue(this, val);
                }
            }
            else
            {
                if (this.GlobalTakeOverMode == eTodoGlobalTakeOverMode.Immediate)
                {
                    this.Variable.SetValue(this, val);
                }
                else
                {
                    if (this.GlobalTakeOverMode == eTodoGlobalTakeOverMode.Scale)
                    {
                        double dt =val -  this.previousval;
                        
                        //Grab the amount of motion
                        double dtscaled;
                        if (dt > 0) 
                        {
                            if (1.0 - this.previousval == 0.0)
                            {
                                dtscaled = 0.0;
                            }
                            else
                            {
                                dtscaled = dt / (1.0 - this.previousval);
                            }
                        }
                        else 
                        {
                            if (this.previousval != 0.0)
                            {
                                dtscaled = dt / this.previousval;
                            }
                            else
                            {
                                dtscaled = 0.0;
                            }
                        }

                        //Map the amount of motion
                        double newval;
                        if (dt > 0) { newval = (1.0 - this.Variable.ValueRaw) * dtscaled; }
                        else { newval = this.Variable.ValueRaw * dtscaled; }

                        this.Variable.SetValue(this, this.Variable.ValueRaw + newval);
                    }
                    else
                    {
                        //Pickup mode
                        double currval = this.Variable.ValueRaw;


                        if ((val <= currval + this.epsilon && previousval >= currval - this.epsilon)
                        || (val >= currval - this.epsilon && previousval <= currval + this.epsilon))
                        {
                            this.Variable.SetValue(this, val);
                        }
                    }
                }
            }

            this.previousval = val;
        }

        public bool IsFeedBackAllowed
        {
            get
            {
                if (this.FeedBackMode == eTodoLocalFeedBackMode.Parent)
                {
                    return this.Variable.AllowFeedBack;
                }
                else
                {
                    return this.FeedBackMode == eTodoLocalFeedBackMode.True;
                }
            }
        }

        public eTodoGlobalTakeOverMode GlobalTakeOverMode
        {
            get
            {
                if (this.TakeOverMode == eTodoLocalTakeOverMode.Parent)
                {
                    return this.Variable.TakeOverMode;
                }
                else
                {
                    if (this.TakeOverMode == eTodoLocalTakeOverMode.Immediate) { return eTodoGlobalTakeOverMode.Immediate; }
                    else if (this.TakeOverMode == eTodoLocalTakeOverMode.Pickup) { return eTodoGlobalTakeOverMode.Pickup; }
                    else { return eTodoGlobalTakeOverMode.Scale; }
                }
            }
        }

        public AbstractTodoInput(TodoVariable var)
        {
            this.Variable = var;
            this.Variable.Inputs.Add(this);
            this.FeedBackMode = eTodoLocalFeedBackMode.Parent;
            this.TakeOverMode = eTodoLocalTakeOverMode.Parent;
        }

    }
}
