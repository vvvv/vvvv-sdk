using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class JavaScriptCondition:JavaScriptObject
    {
        private string FCondition;
        private Expression FVariable;
        private string FArgument;
        private bool FBool;

        public JavaScriptCondition(string Condition)
        {
            FCondition = Condition;
        }

        public JavaScriptCondition(Expression Variable, string Condition, string Argument)
        {
            FVariable = Variable;
            FCondition = Condition;
            FArgument = Argument;
        }

        public JavaScriptCondition(Expression Variable, string Condition, bool Bool)
        {
            FVariable = Variable;
            FCondition = Condition;
            FBool = Bool;
        }



        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            if(FVariable != null)
            {
                if (FArgument != null)
                {
                    return FVariable.GenerateScript(0, false, false) + " " + FCondition + " \"" + FArgument + "\"";
                }
                else
                {
                    return FVariable.GenerateScript(0, false, false) + " " + FCondition + " " + FBool.ToString().ToLower();
                }
            }
            else
            {
                return  FCondition;
            }
        }
    }
}
