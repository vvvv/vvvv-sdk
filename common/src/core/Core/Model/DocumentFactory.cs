using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using VVVV.Core.Model.CS;
using VVVV.Core.Model.FX;

namespace VVVV.Core.Model
{
	public class DocumentFactory
	{
		protected static Dictionary<string, IDocument> FDocuments = new Dictionary<string, IDocument>();
		
		public static IDocument CreateDocumentFromFile(string filename)
		{
			IDocument document;
			
			if (!FDocuments.TryGetValue(filename, out document))
			{
				var location = new Uri(filename);
				
				var fileExtension = Path.GetExtension(filename);
				switch (fileExtension)
				{
					case ".cs":
						document = new CSDocument(filename, location);
						break;
					case ".fx":
					case ".fxh":
						document = new FXDocument(filename, location);
						break;
					default:
						document = new TextDocument(filename, location);
						break;
				}
				
				FDocuments[filename] = document;
				
				document.Disposed += document_Disposed;
			}
			
			return document;
		}

		static void document_Disposed(object sender, EventArgs e)
		{
			var document = sender as IDocument;
			var key = FindDocumentKey(document);
			if (key != null)
				FDocuments.Remove(key);
			document.Disposed -= document_Disposed;
		}
		
		static string FindDocumentKey(IDocument document)
		{
			foreach (var entry in FDocuments)
			{
				if (entry.Value == document)
					return entry.Key;
			}
			return null;
		}
	}
}
