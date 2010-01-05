using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Webinterface.jQuery
{
    class JavaScriptSnippet:JavaScriptObject
    {

        private string FSnippet;

        public JavaScriptSnippet(string Snippet)
        {
            FSnippet = Snippet;
        }

        protected override string GenerateObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            return FSnippet;
        }
    }
}
