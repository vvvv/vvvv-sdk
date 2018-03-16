#region licence/info
//Copyright(C) 2011-2015 Gonçalo Lopes

//Permission is hereby granted, free of charge, to any person obtaining a copy of
//this software and associated documentation files (the "Software"), to deal in
//the Software without restriction, including without limitation the rights to
//use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
//of the Software, and to permit persons to whom the Software is furnished to do
//so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

//taken from: https://bitbucket.org/horizongir/bonsai
#endregion licence/info

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VVVV.Utils.OSC
{
    class MessagePattern
    {
        const char SingleWildcard = '?';
        const char MultipleWildcard = '*';
        const char CharacterSetOpen = '[';
        const char CharacterSetClose = ']';
        const char CharacterRangeSeparator = '-';
        const char CharacterSetNegation = '!';
        const char StringSetOpen = '{';
        const char StringSetClose = '}';
        const char StringSetSeparator = ',';
        bool matchRegex;
        string pattern;

        public MessagePattern(string pattern)
        {
            matchRegex = false;
            var characterSet = false;
            var regexBuilder = new StringBuilder();
            foreach (var c in pattern)
            {
                switch (c)
                {
                    case SingleWildcard: regexBuilder.Append('.'); matchRegex = true; break;
                    case MultipleWildcard: regexBuilder.Append(".*"); matchRegex = true; break;
                    case CharacterSetOpen: regexBuilder.Append('['); characterSet = true; matchRegex = true; break;
                    case CharacterSetClose: regexBuilder.Append(']'); characterSet = false; break;
                    case CharacterSetNegation: regexBuilder.Append(characterSet ? '^' : c); break;
                    case StringSetOpen: regexBuilder.Append('('); matchRegex = true; break;
                    case StringSetSeparator: regexBuilder.Append('|'); break;
                    case StringSetClose: regexBuilder.Append(')'); break;
                    default: regexBuilder.Append(c); break;
                }
            }

            this.pattern = regexBuilder.ToString();
        }

        public bool IsMatch(string part)
        {
            if (matchRegex) return Regex.IsMatch(part, pattern);
            else return part == pattern;
        }
    }
}