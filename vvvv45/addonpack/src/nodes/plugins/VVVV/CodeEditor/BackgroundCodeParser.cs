using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

using VVVV.Core.Model;
using Dom = ICSharpCode.SharpDevelop.Dom;
using NRefactory = ICSharpCode.NRefactory;
using SD = ICSharpCode.TextEditor.Document;

namespace VVVV.HDE.CodeEditor
{
	public class BackgroundCodeParser
	{
		protected BackgroundWorker FBackgroundWorker;
		protected CodeParser FParser;
		
		public SD.IDocument Document { get; set; }
		public ITextDocument TextDocument { get; set; }
		
		public BackgroundCodeParser(IParseInfoProvider parseInfoProvider, ITextDocument textDocument, SD.IDocument document)
		{
			FParser = new CodeParser(parseInfoProvider);
			Document = document;
			TextDocument = textDocument;
			
			FBackgroundWorker = new BackgroundWorker();
			FBackgroundWorker.WorkerReportsProgress = false;
			FBackgroundWorker.WorkerSupportsCancellation = false;
			
			FBackgroundWorker.DoWork += new DoWorkEventHandler(DoWorkCB);
			FBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompletedCB);
		}
		
		public void RunParserAsync()
		{
			if (!FBackgroundWorker.IsBusy)
				FBackgroundWorker.RunWorkerAsync();
		}
		
		private void DoWorkCB(object sender, DoWorkEventArgs args)
		{
			var parseInfo = FParser.Parse(TextDocument);
			args.Result = parseInfo;
		}
		
		private void RunWorkerCompletedCB(object sender, RunWorkerCompletedEventArgs args)
		{
			var parseInfo = args.Result as Dom.ParseInformation;
			Document.FoldingManager.UpdateFoldings(TextDocument.Location.LocalPath, parseInfo);
		}
	}
}
