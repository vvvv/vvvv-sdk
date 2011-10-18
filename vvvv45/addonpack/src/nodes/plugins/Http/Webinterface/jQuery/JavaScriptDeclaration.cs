using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class JavaScriptDeclaration<T>: JavaScriptObject where T: JavaScriptExpression
    {

        private T FVariable;
        private string FValue;
        private bool FDeclare = true;
        private int FIntSteps = 0;
        private bool FBreakInternalLines = false;
        private bool FBreakAfter = false;



        public JavaScriptDeclaration()
        {
        }

        public JavaScriptDeclaration(T Variable, string Value)
        {
            FVariable = Variable;
            FValue = Value;
        }

        public JavaScriptDeclaration(T Variable, string Value, bool Declare)
        {
            FVariable = Variable;
            FValue = Value;
            FDeclare = Declare;
        }

        public JavaScriptDeclaration(T Variable, string Value, bool Declare, int indentSteps)
        {
            FVariable = Variable;
            FValue = Value;
            FDeclare = Declare;
            FIntSteps = indentSteps;
        }

        public JavaScriptDeclaration(T Variable, string Value, bool Declare, int indentSteps, bool breakInternalLines)
        {
            FVariable = Variable;
            FValue = Value;
            FDeclare = Declare;
            FIntSteps = indentSteps;
            FBreakInternalLines = breakInternalLines;
        }

        public JavaScriptDeclaration(T Variable, string Value, bool Declare, int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            FVariable = Variable;
            FValue = Value;
            FDeclare = Declare;
            FIntSteps = indentSteps;
            FBreakInternalLines = breakInternalLines;
            FBreakAfter = breakAfter;
        }


        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            String text = String.Empty;
            FIntSteps = indentSteps;

            if (FBreakInternalLines)
            {
                for (int i = 0; i < FIntSteps + 1; i++)
                {
                    text += "\t";
                }
            }

            if (FDeclare)
            {
                text += "var " +  FVariable.GenerateScript(indentSteps, breakInternalLines, breakAfter) + " = " + FValue;
            }
            else
            {
                text += FVariable.GenerateScript(indentSteps, breakInternalLines, breakAfter) + " = " + FValue;
            }
  
            if (FBreakAfter)
            {
                text += ";" + Environment.NewLine;
            }

            return text;
        }
    }
}
