using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class BlankSelector: Selector
    {

        private string FName;

        public BlankSelector(string Name)
        {
            FName = Name;
        }

        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            return FName;
        }
    }
}
