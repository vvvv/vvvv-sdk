using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Syslog.Utils
{
    internal class WordStream
    {
        public TextReader InternalTextReader { get; private set; }

        public int Position { get; set; }

        public WordStream(TextReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");

            InternalTextReader = reader;
        }

        public string ReadNextWord()
        {
            int c = InternalTextReader.Peek();

            while (c != -1 && char.IsWhiteSpace(Convert.ToChar(c)))
            {
                InternalTextReader.Read();
                c = InternalTextReader.Peek();
            }

            StringBuilder sb = new StringBuilder();

            while (c != -1 && !Char.IsWhiteSpace(Convert.ToChar(c)))
            {
                sb.Append(Convert.ToChar(c));
                InternalTextReader.Read();
                c = InternalTextReader.Peek();
            }

            if (sb.Length == 0)
                return null;
            else
                return sb.ToString();
        }

        public string[] ReadAllWords()
        {
            List<string> words = new List<string>();

            string w = ReadNextWord();

            while (w != null)
            {
                words.Add(w);
                w = ReadNextWord();
            }

            return words.ToArray();
        }
    }
}
