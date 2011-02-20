using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    public abstract class AbstractTodoInput
    {
        public TodoVariable Variable { get; protected set; }

        public abstract string InputType { get; }
        public abstract string InputMap { get; }


        public AbstractTodoInput(TodoVariable var)
        {
            this.Variable = var;
            this.Variable.Inputs.Add(this);
        }
    }
}
