using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    public class JavaScriptCodeBlock: JavaScriptObject
    {


        private List<string> FLines = new List<string>();
        private List<Expression> FJavaScriptObjects = new List<Expression>();


        public JavaScriptCodeBlock(string[] Lines)
        {
            if (Lines != null)
            {
                foreach (string Line in Lines)
                {
                    FLines.Add(Line);
                }
            }
        }

        public JavaScriptCodeBlock(List<Expression> JavaScriptObjects)
        {
            FJavaScriptObjects.AddRange(JavaScriptObjects);
        }

        public JavaScriptCodeBlock(Expression JavaScriptObject)
        {
            if (JavaScriptObject != null)
            {
                FJavaScriptObjects.Add(JavaScriptObject);
            }
        }



        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            string text = String.Empty;

            foreach (Expression JavaObject in FJavaScriptObjects)
            {
                if (breakInternalLines)
                {
                    for (int i = 0; i < indentSteps + 1; i++)
                    {
                        text += "\t";
                    }
                }
                text += JavaObject.GenerateScript(indentSteps, breakInternalLines, breakAfter);

                if (breakAfter)
                {
                    text += ";" + Environment.NewLine;
                }
            }

            return text;
        }
    }


}
