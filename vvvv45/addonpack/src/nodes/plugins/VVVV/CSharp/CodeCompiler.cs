using System;
using System.Text;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;

namespace dhx
{
	/// <summary>
	/// This class can be used to execute dynamic uncompiled code at runtime
	/// This class is not exception safe, all function calls should be exception handled.
	/// </summary>
	public class CodeCompiler
	{

		/// <summary>
		/// Default Constructor.
		/// It is necessary to have an instance of the class so that the reflection
		/// can get the namespace of this class dynamically.
		/// </summary>
		/// <remarks>
		/// This class could be static, but I wanted to make it easy for developers
		/// to use this class by allowing them to change the namespace without
		/// harming the code.
		/// </remarks>
		public CodeCompiler()
		{
		}

		/// <summary>
		/// This is a prebuilt template for calculating a value.
		/// i.e. this is like a calculator.
		/// </summary>
		/// <param name="code">C# code that calculates the desired value</param>
		/// <returns>the result of the calculations</returns>
		public double CalculateDouble(string code)
		{
			BuildCalulatorClass(ref code);
			Assembly calculator = BuildAssembly(code);
			Type type = calculator.GetType(GetType().Namespace + ".Calculator");
			MethodInfo calculate = type.GetMethod("Calculate");
			double val = (double)calculate.Invoke(null, null);
			return val;
		}

		/// <summary>
		/// This is the main code execution function.
		/// It allows for any compilable c# code to be executed.
		/// </summary>
		/// <param name="code">the code to be compiled then executed</param>
		/// <param name="namespacename">the name of the namespace to be executed</param>
		/// <param name="classname">the name of the class of the function in the code that you will execute</param>
		/// <param name="functionname">the name of the function that you will execute</param>
		/// <param name="isstatic">True if the function you will execute is static, otherwise false</param>
		/// <param name="args">any parameters that must be passed to the function</param>
		/// <returns>what ever the executed function returns, in object form</returns>
		public object ExecuteCode(string code, string namespacename, string classname,
			string functionname, bool isstatic, params object[] args)
		{
			object returnval = null;
			Assembly asm = BuildAssembly(code);
			object instance = null;
			Type type = null;
			if (isstatic)
			{
				type = asm.GetType(namespacename + "." + classname);
			}
			else
			{
				instance = asm.CreateInstance(namespacename + "." + classname);
				type = instance.GetType();
			}
			MethodInfo method = type.GetMethod(functionname);
			returnval = method.Invoke(instance, args);
			return returnval;
		}

		/// <summary>
		/// This private function builds the assembly file into memory after compiling the code
		/// </summary>
		/// <param name="code">C# code to be compiled</param>
		/// <returns>the compiled assembly object</returns>
		private Assembly BuildAssembly(string code)
		{
			Microsoft.CSharp.CSharpCodeProvider provider = new CSharpCodeProvider();
			ICodeCompiler compiler = provider.CreateCompiler();
			CompilerParameters compilerparams = new CompilerParameters();
			compilerparams.GenerateExecutable = false;
			compilerparams.GenerateInMemory = true;
			compilerparams.ReferencedAssemblies.Add("System.dll");
			compilerparams.ReferencedAssemblies.Add("System.Windows.Forms.dll");
			CompilerResults results = compiler.CompileAssemblyFromSource(compilerparams, code);
			if (results.Errors.HasErrors)
			{
				StringBuilder errors = new StringBuilder("Compiler Errors :\r\n");
				foreach (CompilerError error in results.Errors )
				{
					errors.AppendFormat("Line {0},{1}\t: {2}\n", error.Line, error.Column, error.ErrorText);
				}
				throw new Exception(errors.ToString());
			}
			else
			{
				return results.CompiledAssembly;
			}
		}

		/// <summary>
		/// This private function is used for building the calculator class
		/// </summary>
		/// <param name="code">inner calculator code</param>
		private void BuildCalulatorClass(ref string code)
		{
			StringBuilder classbuilder = new StringBuilder("using System;");
			
			classbuilder.Append("using System.Windows.Forms;");

			classbuilder.AppendFormat("namespace {0}\r\n{{\r\n", GetType().Namespace);
			classbuilder.Append("public class Calculator\r\n{\r\n");
			classbuilder.Append("public static double Calculate()\r\n{\r\n");
			classbuilder.Append(code);
			classbuilder.Append("\r\n}\r\n}\r\n}");
			code = classbuilder.ToString();
		}
	}
}
