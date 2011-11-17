using System;
using System.IO;

// TODO: Work with URIs.

namespace VVVV.Core.Model
{
	public class TextDocument : Document, ITextDocument
	{
		private string FTextContent = string.Empty;
		private int FHashCodeOnDisk;
		
		public event TextDocumentHandler ContentChanged;
		
		public string TextContent
		{
			get
			{
				return FTextContent;
			}
			set
			{
				string oldValue = FTextContent;
				FTextContent = value;
				
				if (FHashCodeOnDisk != FTextContent.GetHashCode())
					IsDirty = true;
				else
					IsDirty = false;
				
				if (oldValue != value)
					OnContentChanged(oldValue, value);
			}
		}
		
		public TextDocument(string name, Uri location)
			:base(name, location)
		{
		}
		
		protected override void DoLoad()
		{
			string path = Location.LocalPath;

			FTextContent = File.ReadAllText(path);
			FHashCodeOnDisk = FTextContent.GetHashCode();
		}
		
		protected override void DoUnload()
		{
			FTextContent = string.Empty;
		}
		
		public override void SaveTo(Uri location)
		{
			string path = location.LocalPath;
			// Make sure the path exists.
			var documentDir = location.GetLocalDir();
			if (!Directory.Exists(documentDir))
				Directory.CreateDirectory(documentDir);
			
			File.WriteAllText(path, FTextContent);
			FHashCodeOnDisk = FTextContent.GetHashCode();
		}
		
		protected virtual void OnContentChanged(string oldConent, string content)
		{
			if (ContentChanged != null)
				ContentChanged(this, content);
		}
	}
}
