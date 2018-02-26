
using System;
using System.IO;

namespace VVVV.Core.Model.FX
{
	public class FXReference : ProjectItem, IReference
	{
		public FXReference(FXDocument referencedDocument, bool isLocal)
		    : base(Path.GetFileName(referencedDocument.LocalPath))
		{
			ReferencedDocument = referencedDocument;
			ReferencedDocument.Saved += HandleReferencedDocumentSaved;
			IsGlobal = !isLocal;
		}

		void HandleReferencedDocumentSaved(object sender, EventArgs e)
		{
			if (Project != null)
			{
				Project.CompileAsync();
			}
		}
		
		public string AssemblyLocation 
		{
			get 
			{
				return ReferencedDocument.LocalPath;
			}
		}
		
		public FXDocument ReferencedDocument
		{
			get;
			private set;
		}
		
		protected override void DisposeManaged()
		{
			ReferencedDocument.Saved -= HandleReferencedDocumentSaved;
			base.DisposeManaged();
		}
		
		public bool IsGlobal 
		{
			get;
			private set;
		}
	}
}
