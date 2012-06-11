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
        private readonly object FParseLock = new object();
        private readonly ManualResetEvent FParsingDone = new ManualResetEvent(true);
        private readonly SynchronizationContext FSyncContext;

        public event ParseCompletedEventHandler ParseCompleted;

        private ParseInformation FParseInfo;
        public ParseInformation ParseInfo
        {
            get
            {
                FParsingDone.WaitOne();

                lock (FParseLock)
                {
                    return FParseInfo;
                }
            }
        }

        public CSharpExpressionFinder ExpressionFinder
        {
            get;
            private set;
        }

        private CSParserResults FParserResults;
        public CSParserResults ParserResults
        {
            get
            {
                FParsingDone.WaitOne();

                lock (FParseLock)
                {
                    return FParserResults;
                }
            }
        }

        public override bool CanBeCompiled
        {
            get
            {
                return true;
            }
        }

        public CSDocument(string name, Uri location)
            : base(name, location)
        {
            FSyncContext = SynchronizationContext.Current;
            Debug.Assert(FSyncContext != null, "Current SynchronizationContext must be set.");

            FParseInfo = new ParseInformation();
            ExpressionFinder = new CSharpExpressionFinder(FParseInfo);
        }

        public override bool CanRenameTo(string value)
        {
            return base.CanRenameTo(value) && Path.GetExtension(value) == ".cs";
        }

        protected override void OnLoaded(EventArgs e)
        {
            ParseAsync(true);
            base.OnLoaded(e);
        }

        protected override void DoUnload()
        {
            FParsingDone.WaitOne();

            lock (FParseLock)
            {
                base.DoUnload();
            }
        }

        protected override void DisposeManaged()
        {
            if (IsLoaded)
                Unload();
            base.DisposeManaged();
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
            base.OnContentChanged(oldConent, content);
            ParseAsync(false);
        }

        public void Parse()
        {
            Parse(false);
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

        public void Parse(bool parseMethodBodies)
        {
            try
            {
                DebugHelpers.CatchAndLog(() =>
                {
                    lock (FParseLock)
                    {
                        FParserResults = CSParser.Parse(TextContent, parseMethodBodies);

                        var filename = Location.LocalPath;
                        var oldCompilationUnit = FParseInfo.MostRecentCompilationUnit;
                        var projectContent = GetProjectContent();

                        var visitor = new NRefactoryASTConvertVisitor(projectContent);
                        visitor.Specials = FParserResults.Specials;
                        visitor.VisitCompilationUnit(FParserResults.CompilationUnit, null);

                        var newCompilationUnit = visitor.Cu;
                        newCompilationUnit.ErrorsDuringCompile = FParserResults.HasErrors;
                        newCompilationUnit.FileName = filename;

                        UpdateFoldingRegions(newCompilationUnit, FParserResults);
                        AddCommentTags(newCompilationUnit, FParserResults);

                        // Remove information from lastCompilationUnit and add information from newCompilationUnit.
                        projectContent.UpdateCompilationUnit(oldCompilationUnit, newCompilationUnit, filename);
                        FParseInfo.SetCompilationUnit(newCompilationUnit);
                    }
                }, "CSDocument.Parse");
            }
            finally
            {
                FParsingDone.Set();
            }
        }

        public void ParseAsync()
        {
            ParseAsync(false);
        }

        public void ParseAsync(bool parseMethodBodies)
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Document is not loaded.");

            FParsingDone.Reset();
            ThreadPool.QueueUserWorkItem(DoParseAsync, parseMethodBodies);
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

        void DoParseAsync(object state)
        {
            Parse((bool)state);
            FSyncContext.Post((s) => OnParseCompleted(), null);
        }

        protected virtual void OnParseCompleted()
        {
            if (ParseCompleted != null)
            {
                ParseCompleted(this);
            }
        }
    }
}
