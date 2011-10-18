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
		
		public override void Load()
		{
			string path = Location.LocalPath;
			try
			{
				FTextContent = File.ReadAllText(path);
				FHashCodeOnDisk = FTextContent.GetHashCode();
				IsDirty = false;
				
				base.Load();
			}
			catch (Exception)
			{
				FTextContent = "";
				IsDirty = true;
			}
		}
		
		public override void Unload()
		{
			FTextContent = string.Empty;
			IsDirty = false;
			
			base.Unload();
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
