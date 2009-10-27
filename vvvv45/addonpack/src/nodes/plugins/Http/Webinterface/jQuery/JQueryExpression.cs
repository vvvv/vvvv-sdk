using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Nodes.jQuery
{
	public class JQueryExpression : Expression
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

        public static JQueryExpression Dollars(IJavaScriptObject functionParameters)
        {
            return new JQueryExpression(functionParameters);
        }

        public static JQueryExpression Dollars(string functionParameters)
        {
            return new JQueryExpression(functionParameters);
        }

        public static JQueryExpression Dollars()
        {
            return new JQueryExpression();
        }

        #endregion

        #region Fields
        protected IJavaScriptObject FJQueryFunctionParameters;
        #endregion

        #region Constructors
        public JQueryExpression(IJavaScriptObject functionParameters)
        {
            FJQueryFunctionParameters = functionParameters;
        }

        public JQueryExpression(string functionParameters) : 
            this(JavaScriptObjectFactory.Create(functionParameters))
        {

        }

        public JQueryExpression(): this((IJavaScriptObject)null)
        {

        } 
        #endregion

		public JQueryExpression ApplyMethodCall(String methodName, params object[] arguments)
		{
			AddMethodCall(methodName, arguments);
			return this;
		}

		protected override string GetObjectScript(int indentSteps, bool breakInternalLines, bool breakAfter)
        {
            string text = "$";
            if (FJQueryFunctionParameters != null)
            {
                text += "(" + FJQueryFunctionParameters.PScript(indentSteps, breakInternalLines, breakAfter) + ")";
            }
			return text;
        }

        public void Post(string url, IJavaScriptObject data, string type, JavaScriptAnonymousFunction callback)
        {
            FChainedOperations.Enqueue(new MethodCall("post", url, data, type, callback));
        }

        public JQueryExpression Append(object jsObject)
        {
            return ApplyMethodCall("append", jsObject);
        }

        public JQueryExpression Attr(object name, object value)
        {
            return ApplyMethodCall("attr", name, value);
        }

        public JQueryExpression Children()
        {
            return ApplyMethodCall("children");
        }
        
        public JQueryExpression Children(object selectorExpression)
        {
            return ApplyMethodCall("children", selectorExpression);
        }

        public JQueryExpression Css(object property, object value)
        {
            return ApplyMethodCall("css", property, value);
        }

        public JQueryExpression End()
        {
            return ApplyMethodCall("end");
        }

        public JQueryExpression Parent()
        {
            return ApplyMethodCall("parent");
        }

        public JQueryExpression Parent(object selectorExpression)
        {
            return ApplyMethodCall("parent", selectorExpression);
        }
    }
}
