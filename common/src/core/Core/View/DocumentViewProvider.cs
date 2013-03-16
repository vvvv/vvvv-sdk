using System;
using System.IO;
using System.Web;
using System.Linq;

using VVVV.Core.Model;
using VVVV.Core.View;
using VVVV.Utils;

namespace VVVV.Core
{
    // Acts on models of type IReference
    public class DocumentViewProvider : INamed, IRenameable, IDisposable
    {
        private readonly IDocument FDocument;

        public DocumentViewProvider(IDocument document)
        {
            FDocument = document;
            FDocument.Renamed += FDocument_Renamed;
        }

        void FDocument_Renamed(INamed sender, string newName)
        {
            OnRenamed(Path.GetFileName(newName));
        }

        #region INamed implementation
        public string Name 
        {
            get { return Path.GetFileName(FDocument.LocalPath); }
            set
            {
                if (value != Name)
                {
                    var path = Path.Combine(Path.GetDirectoryName(FDocument.LocalPath), value);
                    FDocument.SaveTo(path);
                    var project = FDocument.Project;
                    if (project != null)
                    {
                        project.Documents.Remove(FDocument);
                    }
                    File.Delete(FDocument.LocalPath);
                    FDocument.Dispose();
                    if (project != null)
                    {
                        var document = DocumentFactory.CreateDocumentFromFile(path);
                        project.Documents.Add(document);
                        project.Save();
                    }
                }
            }
        }

        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null)
                Renamed(this, newName);
        }
        #endregion

        public void Dispose()
        {
            FDocument.Renamed -= FDocument_Renamed;
        }

        public bool CanRenameTo(string value)
        {
            var invalidCharacters = Path.GetInvalidFileNameChars();
            return value.Length > 0 && value.ToCharArray().All(c => !invalidCharacters.Contains(c));
        }
    }
}

