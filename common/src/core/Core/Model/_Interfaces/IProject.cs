using System;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using VVVV.Core.Runtime;

namespace VVVV.Core.Model
{
    public class CompilerEventArgs : EventArgs
    {
        public CompilerEventArgs(CompilerResults results)
        {
            CompilerResults = results;
        }
        
        public CompilerResults CompilerResults
        {
            get;
            private set;
        }
    }
    
    public delegate void CompiledEventHandler(object sender, CompilerEventArgs args);
    
    public interface IProject : IIDContainer, IDisposable
    {
        /// <summary>
        /// List of Documents which belong to this project.
        /// </summary>
        IEditableIDList<IDocument> Documents 
        {
            get; 
        }
        
        /// <summary>
        /// List of References needed to compile this project.
        /// </summary>
        IEditableIDList<IReference> References 
        {
            get;
        }
        
        /// <summary>
        /// The ISolution this project belongs to. This property is set by
        /// ISolution after a project has been added to it.
        /// </summary>
        ISolution Solution
        {
            get;
            set;
        }
        
        /// <summary>
        /// The results of the last compile.
        /// </summary>
        CompilerResults CompilerResults
        {
            get;
        }
        
        /// <summary>
        /// Compiles this project with the ICompiler stored in the Compiler property.
        /// </summary>
        void Compile();
        
        /// <summary>
        /// Compiles this project asynchronously. Fires CompileCompleted once compilation
        /// completed.
        /// </summary>
        void CompileAsync();

        /// <summary>
        /// The OnProjectCompiled event occurs when the project compiled successfully.
        /// </summary>
        event CompiledEventHandler ProjectCompiledSuccessfully;
        
        /// <summary>
        /// The OnCompileCompleted event occurs when the CompileAsync finished executing.
        /// </summary>
        event CompiledEventHandler CompileCompleted;

        string LocalPath { get; }

        void SaveTo(string path);
    }

    public static class ProjectExtensions
    {
        public static void Save(this IProject project)
        {
            project.SaveTo(project.LocalPath);
        }
    }
}
