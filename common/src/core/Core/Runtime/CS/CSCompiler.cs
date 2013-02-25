using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.CSharp;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;

namespace VVVV.Core.Runtime.CS
{
	/// <summary>
	/// Compiles a C# project in memory.
	/// </summary>
	public sealed class CSCompiler : ICompiler<CSProject>
	{
		private static readonly CSCompiler FInstance = new CSCompiler();
		private CSharpCodeProvider FProvider;
		
		private CSCompiler()
		{
			var options = new Dictionary<string, string>();
			options.Add("CompilerVersion", "v4.0");
			
			FProvider = new CSharpCodeProvider(options);
		}
		
		public static CSCompiler Instance
		{
			get
			{
				return FInstance;
			}
		}
		
		public CompilerResults Compile(CSProject project)
		{
			var files =
				from doc in project.Documents
				where doc is CSDocument
				select doc.LocalPath;
			
			var assemblyBaseDir = Path.GetDirectoryName(project.AssemblyLocation);
			
			if (!Directory.Exists(assemblyBaseDir))
				Directory.CreateDirectory(assemblyBaseDir);
			
			
			var assemblyLocation = project.AssemblyLocation;
			if (File.Exists(assemblyLocation))
				assemblyLocation = project.GenerateAssemblyLocation();
			
			var compilerParams = new CompilerParameters();
			compilerParams.OutputAssembly = assemblyLocation;
			compilerParams.GenerateExecutable = false;
			compilerParams.GenerateInMemory = false;
			
			switch (project.BuildConfiguration)
			{
				case BuildConfiguration.Release:
					compilerParams.IncludeDebugInformation = false;
					compilerParams.CompilerOptions += "/unsafe /optimize ";
					break;
				case BuildConfiguration.Debug:
					compilerParams.IncludeDebugInformation = true;
					compilerParams.CompilerOptions += "/unsafe";
					break;
			}
			
			foreach (var reference in project.References)
			{
				var location = reference.AssemblyLocation;
				if (Path.GetExtension(location) != ".dll")
					location = string.Format("{0}.dll", location);
				
				compilerParams.ReferencedAssemblies.Add(location);
			}
			
			return FProvider.CompileAssemblyFromFile(compilerParams, files.ToArray());
		}
	}
}
