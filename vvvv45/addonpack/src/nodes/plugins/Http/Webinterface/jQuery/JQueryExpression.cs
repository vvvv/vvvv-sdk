using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JQueryExpression : JavaScriptObject
	{
        #region Static Methods
    
        public static JQueryExpression This()
        {
            return new JQueryExpression(new JavaScriptVariableObject("this"));
        }

        public static JQueryExpression Document()
        {
            return new JQueryExpression(new JavaScriptVariableObject("document"));
        }

        public static JQueryExpression Object(string objectName)
        {
            return new JQueryExpression(new JavaScriptVariableObject(objectName));
        }

        #endregion

        #region Fields
        protected JavaScriptObject FJQueryFunctionParameters;
        protected Queue<MethodCall> FMethodCalls; 
        #endregion

        #region Constructors
        public JQueryExpression(JavaScriptObject functionParameters)
        {
            FJQueryFunctionParameters = functionParameters;
            FMethodCalls = new Queue<MethodCall>();
        }

        public JQueryExpression(string functionParameters) : 
            this(JavaScriptObjectFactory.Create(functionParameters))
        {

        }

        public JQueryExpression(): this((JavaScriptObject)null)
        {

        } 
        #endregion

		public JQueryExpression ApplyMethodCall(String methodName, params object[] arguments)
		{
			FMethodCalls.Enqueue(new MethodCall(methodName, arguments));
			return this;
		}

        public JQuery AsJQuery()
        {
            return new JQuery(this);
        }

        public override string PScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            string text = "$";
            if (FJQueryFunctionParameters != null)
            {
                text += "(" + FJQueryFunctionParameters.PScript(indentSteps, breakInternalLines, breakAfter) + ")";
            }
            foreach (MethodCall methodCall in FMethodCalls)
            {
                text += methodCall.PScript(indentSteps, breakInternalLines, breakAfter);
            }
            return text;
        }

        #region JQueryAPIFunctions
        public JQueryExpression Append(object jsObject)
        {
            return ApplyMethodCall("append", jsObject);
        }

        public JQueryExpression Attr(object attribute, object value)
        {
            return ApplyMethodCall("attr", attribute, value);
        }

        public JQueryExpression Css(object property, object value)
        {
            return ApplyMethodCall("css", property, value);
        }

        public void Post(string url, JavaScriptObject data, string type, JavaScriptAnonymousFunction callback)
        {
            FMethodCalls.Enqueue(new MethodCall("post", url, data, type, callback));
        } 
        #endregion
	}
}
