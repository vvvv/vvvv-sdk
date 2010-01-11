using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class JavaScriptCondition<T>:JavaScriptObject where T : JavaScriptExpression
    {
        private string FCondition;
        private T FVariable;
        private bool FTrueOrFalse;

        public JavaScriptCondition(string Condition)
        {
            FCondition = Condition;
        }

        public JavaScriptCondition(T Variable, string Condition, bool TrueOrFalse)
        {
            FVariable = Variable;
            FCondition = Condition;
            FTrueOrFalse = TrueOrFalse;
        }



        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            if(FVariable != null)
            {
                return FVariable.GenerateScript(0,false,false) + " " + FCondition + " " + FTrueOrFalse.ToString().ToLower();
            }
            else
            {
                return  FCondition;
            }
        }
    }
}
