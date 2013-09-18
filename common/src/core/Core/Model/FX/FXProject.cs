using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Timers;

using VVVV.Core.Logging;
using VVVV.Core.Runtime;

namespace VVVV.Core.Model.FX
{
    /// <summary>
    /// </summary>
    public class FXProject : Project
    {
        private readonly string FIncludePath;
        
        public event EventHandler DoCompileEvent;
        
        protected virtual void OnDoCompileEvent(EventArgs e)
        {
            if (DoCompileEvent != null) {
                DoCompileEvent(this, e);
            }
        }
        
        public FXProject(string path, string includePath)
            : base(path)
        {
            //missusing a DotNetExecutable here..
            CompilerResults = new CompilerResults(null);
            FIncludePath = includePath;
            var document = DocumentFactory.CreateDocumentFromFile(LocalPath) as FXDocument;
            Documents.Add(document);
            UpdateReferences();
        }

        public FXDocument Document
        {
            get { return FDocuments[0] as FXDocument; }
        }
        
        public string Code
        {
            get
            {
                //vvvv only recompiles an fx if the .fx code has changed
                //if an fx has includes it will not check for those being changed
                //therefore if the fx has includes we'll have to fake a change in the main .fx file
                //like so:
                string fakechange = "";
                if (Documents.Count > 1)
                    fakechange = "//" + System.DateTime.Now.ToLongTimeString();
                return (Documents[0] as TextDocument).TextContent + fakechange;
            }
        }
        
        public string ParameterDescription { get; set; }

        protected override CompilerResults DoCompile()
        {
            //since the actual compilation is done inside vvvv
            OnDoCompileEvent(null);
            
            UpdateReferences();
            
            return CompilerResults;
        }
        
        protected override void OnRenamed(string newName)
        {
            Document.Name = newName;
            base.OnRenamed(newName);
        }
        
        public override void CompileAsync()
        {
            // Needs to be done synchronously. Otherwise cast exceptions with IAddonHost COM object (threads...)
            Compile();
        }

        public override void SaveTo(string projectPath)
        {
            var projectDir = Path.GetDirectoryName(projectPath);

            // Create the project directory if it doesn't exist yet.
            if (!Directory.Exists(projectDir))
                Directory.CreateDirectory(projectDir);

            Document.SaveTo(projectPath);
            CopyReferencesTo(projectDir);
        }
        
        private void UpdateReferences()
        {
            var projectPath = LocalPath;
            var refs = new List<FXReference>();
            var localPath = Path.GetDirectoryName(projectPath);
            var includePath = FIncludePath;
            
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (var sr = new StringReader(Document.TextContent))
            {
                var localIncludeRegExp = new Regex(@"^#include\s+""(.+?)""");
                var globalIncludeRegExp = new Regex(@"^#include\s+<(.+?)>");
                
                // Parse lines from the file until the end of
                // the file is reached.
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var match = localIncludeRegExp.Match(line);
                    if (match.Success)
                    {
                        TryToAddReference(localPath, match.Groups[1].Value, true, ref refs);
                    }
                    else
                    {
                        match = globalIncludeRegExp.Match(line);
                        if (match.Success)
                            TryToAddReference(includePath, match.Groups[1].Value, false, ref refs);
                    }
                }
            }
            
            References.BeginUpdate();
            
            foreach (var reference in References)
            {
                reference.Dispose();
            }
            
            References.Clear();
            
            foreach (var reference in refs)
            {
                References.Add(reference);
            }
            
            References.EndUpdate();
        }
        
        private void TryToAddReference(string path, string filename, bool isLocal, ref List<FXReference> refs)
        {
            var include = path.ConcatPath(filename.Replace("/", @"\"));
            if (File.Exists(include))
            {
                var doc = DocumentFactory.CreateDocumentFromFile(include) as FXDocument;
                if (doc != null)
                    refs.Add(new FXReference(doc, isLocal));
            }
        }
    }
}
