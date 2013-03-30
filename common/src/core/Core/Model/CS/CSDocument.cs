using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Dom.CSharp;
using ICSharpCode.SharpDevelop.Dom.NRefactoryResolver;
using VVVV.Core.Model;
using VVVV.Core.Runtime.CS;
using VVVV.Core.Logging;

namespace VVVV.Core.Model.CS
{
    public delegate void ParseCompletedEventHandler(CSDocument document);
    
    public class CSDocument : TextDocument
    {
        private readonly ParseInformation FParseInfo;
        private bool FIsParseInfoValid;
        public ParseInformation ParseInfo
        {
            get 
            {
                if (!FIsParseInfoValid)
                {
                    FIsParseInfoValid = true;
                    var parserResults = Parse();
                    UpdateParseInfo(parserResults);
                }
                return FParseInfo; 
            }
        }
        
        public CSharpExpressionFinder ExpressionFinder
        {
            get;
            private set;
        }
        
        public override bool CanBeCompiled 
        {
            get 
            {
                return true;
            }
        }

        public CSDocument(string name, string path)
            : base(name, path)
        {
            FParseInfo = new ParseInformation();
            ExpressionFinder = new CSharpExpressionFinder(FParseInfo);
        }

        public override bool CanRenameTo(string value)
        {
            return base.CanRenameTo(value) && Path.GetExtension(value) == ".cs";
        }
        
        /// <summary>
        /// Finds an expression around the current offset.
        /// </summary>
        public ExpressionResult FindFullExpression(int offset)
        {
            return ExpressionFinder.FindFullExpression(TextContent, offset);
        }
        
        /// <summary>
        /// Finds an expression before the current offset.
        /// </summary>
        public ExpressionResult FindExpression(int offset)
        {
            return ExpressionFinder.FindExpression(TextContent, offset);
        }
        
        public ResolveResult Resolve(ExpressionResult expression)
        {
            var resolver = new NRefactoryResolver(LanguageProperties.CSharp);
            return resolver.Resolve(expression, ParseInfo, TextContent);
        }
        
        protected override void OnContentChanged(string oldConent, string content)
        {
            FIsParseInfoValid = false;
            base.OnContentChanged(oldConent, content);
        }

        public CSParserResults Parse()
        {
            return Parse(false);
        }
        
        private IProjectContent GetProjectContent()
        {
            var project = Project as CSProject;
            if (project != null)
                return project.ProjectContent;
            else
            {
                var projectContent = new DefaultProjectContent();
                projectContent.Language = LanguageProperties.CSharp;
                return projectContent;
            }
        }
        
        public CSParserResults Parse(bool parseMethodBodies)
        {
            return CSParser.Parse(TextContent, parseMethodBodies);
        }

        private void UpdateParseInfo(CSParserResults results)
        {
            var filename = LocalPath;
            var oldCompilationUnit = FParseInfo.MostRecentCompilationUnit;
            var projectContent = GetProjectContent();

            var visitor = new NRefactoryASTConvertVisitor(projectContent);
            visitor.Specials = results.Specials;
            visitor.VisitCompilationUnit(results.CompilationUnit, null);

            var newCompilationUnit = visitor.Cu;
            newCompilationUnit.ErrorsDuringCompile = results.HasErrors;
            newCompilationUnit.FileName = filename;

            UpdateFoldingRegions(newCompilationUnit, results);
            AddCommentTags(newCompilationUnit, results);

            projectContent.UpdateCompilationUnit(oldCompilationUnit, newCompilationUnit, filename);
            FParseInfo.SetCompilationUnit(newCompilationUnit);
        }
        
        private void UpdateFoldingRegions(ICompilationUnit compilationUnit, CSParserResults parserResults)
        {
            var directives = new Stack<PreprocessingDirective>();
            var foldingRegions = compilationUnit.FoldingRegions;
            
            // Collect all #region and #endregion directives and push them on a stack.
            foreach (var special in parserResults.Specials)
            {
                var directive = special as PreprocessingDirective;
                if (directive != null)
                {
                    if (directive.Cmd == "#region")
                        directives.Push(directive);
                    else if (directive.Cmd == "#endregion")
                    {
                        if (directives.Count > 0)
                        {
                            var o = directives.Pop();
                            var l = DomRegion.FromLocation(o.StartPosition, directive.EndPosition);
                            foldingRegions.Add(new FoldingRegion(o.Arg.Trim(), l));
                        }
                    }
                }
            }
        }
        
        private void AddCommentTags(ICompilationUnit compilationUnit, CSParserResults parserResults)
        {
            foreach (var tagComment in parserResults.TagComments)
            {
                var tagRegion = new DomRegion(tagComment.StartPosition.Y, tagComment.StartPosition.X);
                var tag = new TagComment(tagComment.Tag, tagRegion, tagComment.CommentText);
                compilationUnit.TagComments.Add(tag);
            }
        }
    }
}
