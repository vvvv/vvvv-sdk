// CSharp Editor Example with Code Completion
// Copyright (c) 2006, Daniel Grunwald
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are
// permitted provided that the following conditions are met:
// 
// - Redistributions of source code must retain the above copyright notice, this list
//   of conditions and the following disclaimer.
// 
// - Redistributions in binary form must reproduce the above copyright notice, this list
//   of conditions and the following disclaimer in the documentation and/or other materials
//   provided with the distribution.
// 
// - Neither the name of the ICSharpCode team nor the names of its contributors may be used to
//   endorse or promote products derived from this software without specific prior written
//   permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS &AS IS& AND ANY EXPRESS
// OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
// IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT
// OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Gui.CompletionWindow;
using VVVV.Core.Model;
using VVVV.Core.Model.FX;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactoryResolver = ICSharpCode.SharpDevelop.Dom.NRefactoryResolver.NRefactoryResolver;

namespace VVVV.HDE.CodeEditor
{
    class CodeCompletionProvider : ICompletionDataProvider
    {
        protected ITextDocument FDocument;
        protected IParseInfoProvider FParseInfoProvider;
        protected Dictionary<string, string> FHLSLReference;
        protected Dictionary<string, string> FTypeReference;
        
        public CodeCompletionProvider(IParseInfoProvider parseInfoProvider, ITextDocument document, ImageList imageList, Dictionary<string, string> hlslReference, Dictionary<string, string> typeReference)
        {
            FParseInfoProvider = parseInfoProvider;
            FDocument = document;
            ImageList = imageList;
            FHLSLReference = hlslReference;
            FTypeReference = typeReference;
        }
        
        public ImageList ImageList
        {
            get;
            private set;
        }
        
        private string FPreSelection = null;
        public string PreSelection
        {
            get
            {
                return FPreSelection;
            }
            protected set
            {
                FPreSelection = value;
            }
        }
        
        private int FDefaultIndex = -1;
        public int DefaultIndex
        {
            get
            {
                return FDefaultIndex;
            }
            protected set
            {
                FDefaultIndex = value;
            }
        }
        
        public CompletionDataProviderKeyResult ProcessKey(char key)
        {
            if (char.IsLetterOrDigit(key) || key == '_')
            {
                return CompletionDataProviderKeyResult.NormalKey;
            }
            else
            {
                // key triggers insertion of selected items
                return CompletionDataProviderKeyResult.InsertionKey;
            }
        }
        
        /// <summary>
        /// Called when entry should be inserted. Forward to the insertion action of the completion data.
        /// </summary>
        public bool InsertAction(ICompletionData data, TextArea textArea, int insertionOffset, char key)
        {
            textArea.Caret.Position = textArea.Document.OffsetToPosition(insertionOffset);
            return data.InsertAction(textArea, key);
        }
        
        public ICompletionData[] GenerateCompletionData(string fileName, TextArea textArea, char charTyped)
        {
            // We can return code-completion items like this:
            
            //return new ICompletionData[] {
            //	new DefaultCompletionData("Text", "Description", 1)
            //};
            
            string ext = System.IO.Path.GetExtension(fileName);
            if ((ext == ".fx") || (ext == ".fxh"))
            {
                //types: everywhere
                //variable names: only inside of single {}
                //hlsl reference: only inside of single {}
                //semantics: only directly after :
                
                //parse ParameterDescription
                var inputs = (FDocument.Project as FXProject).ParameterDescription.Split(new char[1]{'|'});
                int i = 0;
                ICompletionData[] cData = new ICompletionData[FHLSLReference.Count + FTypeReference.Count + inputs.Length-1];
                foreach (var input in inputs)
                {
                    if (!string.IsNullOrEmpty(input))
                    {
                        var desc = input.Split(new char[1]{','});
                        string name = "";
                        if (desc[0] != desc[1])
                            name = desc[1] + "\n";
                        
                        string tooltip = name + desc[2] + "\n";
                        if (Convert.ToInt32(desc[3]) > 1)
                            tooltip += desc[6].Replace("(Rows)", "") + desc[3] + "x" + desc[4];
                        else
                            tooltip += desc[7] + desc[4].Replace("1", "").Replace("0", "");
                        cData[i] = new DefaultCompletionData(desc[0], tooltip, 3);
                        i++;
                    }
                }                
                
                foreach(var function in FHLSLReference)
                {
                    cData[i] = new DefaultCompletionData(function.Key, function.Value, 1);
                    i++;
                }
                
                foreach(var type in FTypeReference)
                {
                    cData[i] = new DefaultCompletionData(type.Key, type.Value, 0);
                    i++;
                }
                
                //set preselection to "" for popup to sort by first character
                FPreSelection = "";
                
                return cData;
            }
            
            var projectContent = FParseInfoProvider.GetProjectContent(FDocument.Project);
            var parseInfo = FParseInfoProvider.GetParseInfo(FDocument);
            
            var resolver = new NRefactoryResolver(projectContent.Language);
            
            var resultList = new List<ICompletionData>();
            var expressionResult = FindExpression(parseInfo, textArea);
            
            Debug.WriteLine(String.Format("Generating completion data for expression result {0}", expressionResult));

            ArrayList completionData = null;
            if (charTyped == '.')
            {
                FPreSelection = null;
                var rr = resolver.Resolve(expressionResult,
                                          parseInfo,
                                          textArea.MotherTextEditorControl.Text);
                
                if (rr != null)
                    completionData = rr.GetCompletionData(projectContent);
            }
            else
            {
                FPreSelection = "";
                completionData = resolver.CtrlSpace(textArea.Caret.Line + 1,
                                                    textArea.Caret.Column + 1,
                                                    parseInfo,
                                                    textArea.Document.TextContent,
                                                    expressionResult.Context);
            }
            
            if (completionData != null)
                AddCompletionData(ref resultList, completionData, expressionResult.Context);
            
            return resultList.ToArray();
        }
        
        /// <summary>
        /// Find the expression the cursor is at.
        /// Also determines the context (using statement, "new"-expression etc.) the
        /// cursor is at.
        /// </summary>
        Dom.ExpressionResult FindExpression(Dom.ParseInformation parseInfo, TextArea textArea)
        {
            var document = textArea.Document;
            var finder = new Dom.CSharp.CSharpExpressionFinder(parseInfo);

            var expression = finder.FindExpression(document.GetText(0, textArea.Caret.Offset), textArea.Caret.Offset);
            if (expression.Region.IsEmpty) {
                expression.Region = new Dom.DomRegion(textArea.Caret.Line + 1, textArea.Caret.Column + 1);
            }
            return expression;
        }

        void AddCompletionData(ref List<ICompletionData> resultList, ArrayList completionData, Dom.ExpressionContext context)
        {
            // used to store the method names for grouping overloads
            Dictionary<string, CodeCompletionData> nameDictionary = new Dictionary<string, CodeCompletionData>();

            // Add the completion data as returned by SharpDevelop.Dom to the
            // list for the text editor
            foreach (object obj in completionData) {
                if (!context.ShowEntry(obj))
                    continue;
                
                if (obj is string) {
                    // namespace names are returned as string
                    resultList.Add(new DefaultCompletionData((string)obj, "namespace " + obj, 5));
                } else if (obj is Dom.IClass) {
                    Dom.IClass c = (Dom.IClass)obj;
                    resultList.Add(new CodeCompletionData(c));
                } else if (obj is Dom.IMember) {
                    Dom.IMember m = (Dom.IMember)obj;
                    if (m is Dom.IMethod && ((m as Dom.IMethod).IsConstructor)) {
                        // Skip constructors
                        continue;
                    }
                    // Group results by name and add "(x Overloads)" to the
                    // description if there are multiple results with the same name.
                    
                    CodeCompletionData data;
                    if (nameDictionary.TryGetValue(m.Name, out data)) {
                        data.AddOverload();
                    } else {
                        nameDictionary[m.Name] = data = new CodeCompletionData(m);
                        resultList.Add(data);
                    }
                } else {
                    // Current ICSharpCode.SharpDevelop.Dom should never return anything else
                    throw new NotSupportedException();
                }
            }
        }
    }
}
