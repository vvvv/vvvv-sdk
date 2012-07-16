using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    public class TodoCategories : Dictionary<string, TodoCategory>
    {

    }

    /// <summary>
    /// Category to contain multiple variables
    /// </summary>
    public class TodoCategory
    {
        public TodoCategory(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }

        public List<TodoVariable> Variables { get; private set; }
    }
}
