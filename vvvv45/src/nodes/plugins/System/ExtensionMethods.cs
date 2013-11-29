using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace VVVV.Nodes.Input
{
    static class ExtensionMethods
    {
        /// <summary>
        /// Generates an edge (true, false) in the output sequence for each
        /// element received from the source sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <returns>
        /// An observable sequence containing an edge (true, false) for each 
        /// element from the source sequence.
        /// </returns>
        public static IObservable<bool> Edge<T>(this IObservable<T> source)
        {
            return source.SelectMany(_ => new[] { true, false });
        }

        public static List<Keys> ToKeyCodes(this string value)
        {
            return value.Split(',')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0)
                .Select(s =>
                    {
                        Keys keyCode;
                        if (Enum.TryParse<Keys>(s, true, out keyCode))
                            return keyCode;
                        else
                            return Keys.None;
                    }
                )
                .Where(keyCode => keyCode != Keys.None)
                .ToList();
        }
    }
}
