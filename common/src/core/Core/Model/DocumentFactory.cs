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

        protected static Dictionary<string, Type> FLoaders;

        private static void Initialize()
        {
            if (FLoaders == null)
            {
                FLoaders = new Dictionary<string, Type>();

                FLoaders.Add(".cs", typeof(CSDocument));
                FLoaders.Add(".fx", typeof(FXDocument));
                FLoaders.Add(".fxh", typeof(FXDocument));
                FLoaders.Add(".xx", typeof(FXDocument));
            }
        }

        public static void RegisterLoader(string ext, Type t)
        {
            Initialize();

            if (!FLoaders.ContainsKey(ext))
            {
                FLoaders[ext] = t;
            }
        }

        public static IDocument CreateDocumentFromFile(string filename)
        {
            Initialize();

            IDocument document;
            
            if (!FDocuments.TryGetValue(filename, out document))
            {
                var fileExtension = Path.GetExtension(filename);

                if (FLoaders.ContainsKey(fileExtension))
                {
                    object o = Activator.CreateInstance(FLoaders[fileExtension], filename, filename);
                    document = o as IDocument;
                }
                else
                {
                    document = new TextDocument(filename, filename);
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
