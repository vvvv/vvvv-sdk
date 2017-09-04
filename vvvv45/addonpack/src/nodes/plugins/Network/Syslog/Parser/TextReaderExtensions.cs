using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VVVV.Nodes.Syslog.Parser
{
    internal static class TextReaderExtensions
    {
        public static string ReadUntilSpace(this TextReader r)
        {
            //read until next space and then consume the spaces
            StringBuilder buffer = new StringBuilder();

            while (r.Peek() != ' ')
            {
                buffer.Append((char)r.Read());
            }

            //we are done reading, keep peeking until we have consumed all the consecutive spaces
            MaybeConsumeSpaces(r);

            return buffer.ToString();
        }

        public static string ReadUntilChar(this TextReader r, char c)
        {
            //read until next space and then consume the spaces
            StringBuilder buffer = new StringBuilder();

            while (r.Peek() != c)
            {
                buffer.Append((char)r.Read());
            }

            return buffer.ToString();
        }

        public static string ReadUntilCharAndThenConsume(this TextReader r, char c)
        {
            //read until next space and then consume the spaces
            StringBuilder buffer = new StringBuilder();

            while (r.Peek() != c)
            {
                buffer.Append((char)r.Read());
            }

            r.Read(); //consume

            return buffer.ToString();
        }

        public static void MaybeConsumeSpaces(this TextReader r)
        {
            while (r.Peek() == ' ')
                r.Read();
        }
    }
}
