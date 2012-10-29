using System;
using System.IO;
using System.Text.RegularExpressions;

namespace VVVV.Core.Runtime
{
	public class RuntimeError
	{
		static Regex FRegex = new Regex(@"([a-zA-z]:\\.+):[\sa-zA-z]+([\d]+)\.$");
		
		public string ErrorText
		{
			get;
			private set;
		}
		
		public string FileName
		{
			get;
			private set;
		}
		
		public int Line
		{
			get;
			private set;
		}
		
		public RuntimeError(Exception exception)
			: this(exception.ToString())
		{
		}
		
		public RuntimeError(string message)
		{
			bool first = true;
			using (var reader = new StringReader(message))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					if (first)
						ErrorText = line;
					else
					{
						var match = FRegex.Match(line);
						if (match.Success)
						{
							if (match.Groups.Count > 1)
								FileName = match.Groups[1].Value;
							if (match.Groups.Count > 2)
								Line = int.Parse(match.Groups[2].Value);
						}
					}
					
					first = false;
				}
			}
		}
		
		public RuntimeError(string errorText, string fileName, int line)
		{
			ErrorText = errorText;
			FileName = fileName;
			Line = line;
		}
	}
}
