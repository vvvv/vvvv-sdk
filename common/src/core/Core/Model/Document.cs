using System;
using System.Collections.Generic;
using System.IO;

namespace VVVV.Core.Model
{
	public abstract class Document : PersistentIDContainer, IDocument, IRenameable
	{
		public Document(string name, Uri location)
			: base(name, location)
		{
		}
		
		protected override string CreateName(Uri location)
		{
			return Path.GetFileName(location.LocalPath);
		}
		
		protected override void OnRenamed(string newName)
		{
			base.OnRenamed(newName);
			
			if (Project != null)
				Project.Save();
		}
		
		public IProject Project
		{
			get;
			set;
		}
		
		public virtual bool CanBeCompiled
		{
			get
			{
				return false;
			}
		}
		
		public override string ToString()
		{
		    return string.Format("Document {0}", Name);
		}
	}
}
