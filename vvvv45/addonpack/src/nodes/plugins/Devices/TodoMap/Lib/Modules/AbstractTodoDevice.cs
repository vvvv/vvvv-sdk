using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib.Modules
{
    public abstract class AbstractTodoDevice<T> where T: AbstractTodoInput
    {
        protected TodoEngine engine;

        protected List<T> inputvars = new List<T>();

        protected abstract void DoFeedBack(TodoVariable var, T source);

        public AbstractTodoDevice(TodoEngine engine)
        {
            this.engine = engine;
        }

        #region Manage Mappings
        public void ClearMappings()
        {
            this.inputvars.Clear();
        }

        public void RegisterInput(T input)
        {
            this.inputvars.Add(input);
        }

        public void RemoveInput(T input)
        {
            this.inputvars.Remove(input);
        }
        #endregion

        #region FeedBack
        public void FeedBack(TodoVariable var, AbstractTodoInput source)
        {
            foreach (AbstractTodoInput input in var.Inputs)
            {
                if (input.IsFeedBackAllowed && input != source)
                {
                    if (input is T)
                    {
                        this.DoFeedBack(var, input as T);
                    }
                }
            }
        }
        #endregion


    }
}
