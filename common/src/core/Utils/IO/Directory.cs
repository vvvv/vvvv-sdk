//from: http://dotnetperls.com/Content/Recursively-Find-Files.aspx

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VVVV.Utils.IO
{
	/// <summary>
	/// Use this method to explore a directory and all of its files. Then, it
	/// recurses into the next level of directories, and collects a listing
	/// of all the file names you want.
	/// </summary>
	public static class DirectoryHelper
	{
		/// <summary>
		/// Find all files in a directory, and all files within every nested
		/// directory.
		/// </summary>
		/// <param name="baseDir">The starting directory you want to use.</param>
		/// <returns>A string array containing all the file names.</returns>
		static public string[] GetAllFileNames(string BaseDir, string Mask)
		{
			//
			// Store results in the file results list.
			//
			List<string> fileResults = new List<string>();

			//
			// Store a stack of our directories.
			//
			Stack<string> directoryStack = new Stack<string>();
			directoryStack.Push(BaseDir);

			//
			// While there are directories to process
			//
			while (directoryStack.Count > 0)
			{
				string currentDir = directoryStack.Pop();

				//
				// Add all files at this directory.
				//
				foreach (string fileName in Directory.GetFiles(currentDir, Mask))
				{
					fileResults.Add(fileName);
				}

				//
				// Add all directories at this directory.
				//
				foreach (string directoryName in Directory.GetDirectories(currentDir))
				{
					directoryStack.Push(directoryName);
				}
			}
			return fileResults.ToArray();
		}
	}
}
