using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.TodoMap.Lib
{
    /// <summary>
    /// Category to contain multiple variables
    /// </summary>
    public class TodoCategory
    {
        public string Name { get; private set; }

        public List<TodoVariable> Variables { get; private set; }
    }
}
