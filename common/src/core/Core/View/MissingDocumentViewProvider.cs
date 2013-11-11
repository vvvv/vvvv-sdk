using System;
using System.IO;
using System.Web;
using System.Linq;

using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.Utils;

namespace VVVV.Core
{
    public class MissingDocumentViewProvider : INamed, IDescripted
    {
        private readonly MissingDocument FDocument;

        public MissingDocumentViewProvider(MissingDocument document)
        {
            FDocument = document;
        }

        #region INamed implementation
        public string Name 
        {
            get { return "!" + Path.GetFileName(FDocument.LocalPath); }
        }

        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null)
                Renamed(this, newName);
        }
        #endregion

        #region IDescripted
        public string Description
        {
            get { return "Document is missing!"; }
        }
        #endregion
    }
}

