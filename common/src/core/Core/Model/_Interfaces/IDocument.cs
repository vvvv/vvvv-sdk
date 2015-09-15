using System;
using System.Collections.Generic;
using System.IO;
using VVVV.Utils;

namespace VVVV.Core.Model
{
    public interface IDocument : IIDItem, IProjectItem
    {
        string LocalPath { get; }
        void SaveTo(string path);
        Stream Content { get; set; }
        Stream ContentOnDisk { get; }
        /// <summary>
        /// This event occurs each time the content of this document changes.
        /// </summary>
        event EventHandler<ContentChangedEventArgs> ContentChanged;
        /// <summary>
        /// Raised when the file has been modified by another program.
        /// </summary>
        event EventHandler<EventArgs> FileChanged;
        bool IsDirty { get; }
        bool IsReadOnly { get; set; }
    }
    
    public static class DocumentExtensionMethods
    {
        /// <summary>
        /// Returns the relative path from the specified persistent to this IPersistent.
        /// Example: Foo\Bar\ThisDocument.txt
        /// </summary>
        public static string GetRelativePath(this Uri location1, Uri location2)
        {
            var persistent2Dir = location2.GetLocalDir() + "\\";
            var relativePath = new Uri(persistent2Dir).MakeRelativeUri(location1).ToString();
            return Uri.UnescapeDataString(relativePath).Replace('/', '\\');
        }

    	/// <summary>
    	/// Returns the relative path from the containing IProject to this IDocument.
    	/// Example: Foo\Bar\ThisDocument.txt
    	/// </summary>
    	public static string GetRelativePath(this IDocument doc)
        {
            return PathUtils.MakeRelativePath(Path.GetDirectoryName(doc.Project.LocalPath), doc.LocalPath);
        }
    	
    	/// <summary>
    	/// Returns the relative path from the containing IProject to the directory of this IDocument.
    	/// Example: Foo\Bar
    	/// </summary>
    	public static string GetRelativeDir(this IDocument doc)
        {
            var relativePath = doc.GetRelativePath();
            if (string.IsNullOrEmpty(relativePath))
            	return string.Empty;
            else
            	return Path.GetDirectoryName(relativePath);
        }

        public static void Save(this IDocument doc)
        {
            doc.SaveTo(doc.LocalPath);
        }

        public static void Rename(this IDocument doc, string filename)
        {
            var path = Path.Combine(Path.GetDirectoryName(doc.LocalPath), filename);
            doc.SaveTo(path);
            var project = doc.Project;
            if (project != null)
            {
                project.Documents.Remove(doc);
            }
            File.Delete(doc.LocalPath);
            doc.Dispose();
            if (project != null)
            {
                var document = DocumentFactory.CreateDocumentFromFile(path);
                project.Documents.Add(document);
                project.Save();
            }
        }
    }
}
