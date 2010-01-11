using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class JavaScriptIfCondition : JavaScriptObject
    {

        private JavaScriptCondition<JavaScriptExpression> FCondition;
        private JavaScriptCodeBlock FIfStatement;
        private JavaScriptCodeBlock FElseStatement;

        public JavaScriptIfCondition(JavaScriptCondition<JavaScriptExpression> Condition, JavaScriptCodeBlock IfStatement, JavaScriptCodeBlock ElseStatement)
        {
            FCondition = Condition;
            FIfStatement = IfStatement;
            FElseStatement = ElseStatement;
        }


        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            
            string text = String.Empty;
            string Steps = String.Empty;

            for (int i = 0; i < indentSteps + 1; i++)
            {
                Steps += "\t";
            }
            
            text += "if( " + FCondition.GenerateScript(0, false, false) + " )" + Environment.NewLine;
            text += Steps + "{" + Environment.NewLine;
            text += FIfStatement.GenerateScript(indentSteps + 1 , breakInternalLines, breakAfter);
            text += Steps + "}" + Environment.NewLine;
            if(FElseStatement != null)
            {
                text += Steps + "else" + Environment.NewLine;
                text += Steps + "{" + Environment.NewLine;
                text += FElseStatement.GenerateScript(indentSteps + 1, breakInternalLines, breakAfter);
                text += Steps + "}";
            }

            return text;
        }
    }
}
