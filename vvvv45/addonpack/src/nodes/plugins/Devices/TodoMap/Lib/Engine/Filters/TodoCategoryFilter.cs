using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib.Engine.Filters
{
    public class TodoCategoryFilter
    {
        private TodoEngine engine;

        public TodoCategoryFilter(TodoEngine engine)
        {
            this.engine = engine;
        }


        public List<string> Categories
        {
            get
            {
                List<string> result = new List<string>();
                foreach (TodoVariable var in this.engine.Variables)
                {
                    if (!result.Contains(var.Category)) { result.Add(var.Category); }
                }
                return result;
            }
        }

        public List<TodoVariable> AllVariables()
        {
            return this.engine.Variables;
        }

        public List<TodoVariable> Filter(string category)
        {
            List<TodoVariable> result = new List<TodoVariable>();
            foreach (TodoVariable var in this.engine.Variables)
            {
                if (var.Category == category) { result.Add(var); }
            }
            return result;
        }
    }
}
