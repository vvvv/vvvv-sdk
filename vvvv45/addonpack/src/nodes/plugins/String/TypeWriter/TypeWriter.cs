#region usings
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using System.Reactive.Linq;

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Core.Logging;
using VVVV.Utils.IO;
#endregion using

namespace VVVV.Nodes
{
    class TypeWriter : IDisposable
    {
        private Keyboard FKeyboard;
        private IDisposable FKeyboardSubscription;
        public Keyboard Keyboard
        {
            set
            {
                if (value != FKeyboard)
                {
                    if (FKeyboardSubscription != null)
                    {
                        FKeyboardSubscription.Dispose();
                        FKeyboardSubscription = null;
                    }
                    FKeyboard = value;
                    if (FKeyboard != null)
                    {
                        FKeyboardSubscription = FKeyboard.KeyNotifications
                            .Subscribe(n =>
                            {
                                switch (n.Kind)
                                {
                                    case KeyNotificationKind.KeyDown:
                                        FControlKeyPressed = (FKeyboard.Modifiers & Keys.Control) > 0;
                                        var altKeyPressed = (FKeyboard.Modifiers & Keys.Alt) > 0;
                                        var keyDown = n as KeyDownNotification;
                                        var keyCode = keyDown.KeyCode;
                                        if ((int)keyCode < 48)
                                        {
                                            if (!altKeyPressed)
                                                RunCommand(keyCode);
                                        }
                                        break;
                                    case KeyNotificationKind.KeyPress:
                                        var keyPress = n as KeyPressNotification;
                                        var chr = keyPress.KeyChar;
                                        if (!char.IsControl(chr))
                                            AddNewChar(chr.ToString());
                                        break;
                                }
                            }
                            );
                    }
                }
            }
        }

        public int CursorPosition
        {
            get { return FCursorCharPos; }
            set
            {
                FCursorCharPos = Math.Min(FText.Length, Math.Max(0, value));
            }
        }

        public bool IgnoreNavigationKeys { get; set; }

        public string Output { get { return FText; } }

        private bool FControlKeyPressed;
        private string FLastCapitalKey;
        private string FText = "";
        private int FCursorCharPos = 0;
        //defined as string but must be only one character:
        private string FNewlineSymbol = Environment.NewLine;
        private Dictionary<uint, double> FBufferedCommands = new Dictionary<uint, double>();
        private Dictionary<string, double> FBufferedKeys = new Dictionary<string, double>();

        #region Text navigation helper
        //todo:
        //text splitting is done here every call,
        //should be done globally
        int GetLineCount()
        {
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            return lines.Length;
        }

        int GetLineOfCursor()
        {
            var lineStart = 0;
            var lineEnd = 0;
            var lineIndex = 0;
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                lineEnd += line.Length + FNewlineSymbol.Length;

                if (FCursorCharPos >= lineStart && FCursorCharPos < lineEnd)
                    break;

                lineStart += line.Length + FNewlineSymbol.Length;
                lineIndex++;
            }

            return lineIndex;
        }

        int GetStartOfLine(int lineIndex)
        {
            var lineStart = 0;
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);

            for (int i = 0; i < lineIndex; i++)
                lineStart += lines[i].Length + FNewlineSymbol.Length;

            return lineStart;
        }

        int GetLengthOfLine(int lineIndex)
        {
            var lines = FText.Split(new string[] { FNewlineSymbol }, StringSplitOptions.None);
            return lines[lineIndex].Length;
        }
        #endregion Text navigation helper

        #region Cursor
        void CursorToTextStart()
        {
            FCursorCharPos = 0;
        }

        void CursorToTextEnd()
        {
            FCursorCharPos = FText.Length;
        }

        void CursorOneLineUp()
        {
            int line = GetLineOfCursor();
            if (line > 0)
            {
                var startOfLine = GetStartOfLine(line);
                var posInLine = FCursorCharPos - startOfLine;
                var newLine = line - 1;

                var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));

                FCursorCharPos = EnsureCorrectCursorPlacement(newPos);
            }
        }

        void CursorOneLineDown()
        {
            int line = GetLineOfCursor();
            if (line < GetLineCount() - 1)
            {
                var startOfLine = GetStartOfLine(line);
                var posInLine = FCursorCharPos - startOfLine;
                var newLine = line + 1;

                var newPos = GetStartOfLine(newLine) + Math.Min(posInLine, GetLengthOfLine(newLine));

                FCursorCharPos = EnsureCorrectCursorPlacement(newPos);
            }
        }

        int EnsureCorrectCursorPlacement(int newPos)
        {
            try
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos -= 1;
            }
            catch
            {
                //FText[newPos] may access out of range char..nevermind
            }

            return Math.Max(0, Math.Min(FText.Length, newPos));
        }

        void CursorToLineStart()
        {
            int line = GetLineOfCursor();
            FCursorCharPos = GetStartOfLine(line);
        }

        void CursorToLineEnd()
        {
            int line = GetLineOfCursor();
            FCursorCharPos = GetStartOfLine(line) + GetLengthOfLine(line);
        }

        void CursorStepsRight(int steps = 1)
        {
            var newPos = Math.Min(FText.Length, FCursorCharPos + steps);
            //if cursor lands on an \r and the next symbol is an \n make sure to step one more
            if (newPos >= 0 && newPos < FText.Length)
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos += 1;
            }

            FCursorCharPos = Math.Min(FText.Length, newPos);
        }

        void CursorStepsLeft()
        {
            var newPos = Math.Max(0, FCursorCharPos - 1);
            //if cursor lands on an \n and the previouse symbol is an \r make sure to step one more
            if (newPos >= 0 && newPos < FText.Length)
            {
                if (FText[newPos] == '\n' && FText[newPos - 1] == '\r')
                    newPos -= 1;
            }

            FCursorCharPos = Math.Max(0, newPos);
        }

        void CursorStepsWordRight()
        {
            var nextSpace = FText.IndexOfAny(new char[] { ' ', FNewlineSymbol[0] }, FCursorCharPos) + 1;
            FCursorCharPos = Math.Min(FText.Length, nextSpace);
        }

        void CursorStepsWordLeft()
        {
            if (FCursorCharPos > 2)
            {
                var lastSpace = FText.LastIndexOfAny(new char[] { ' ', FNewlineSymbol[0] }, FCursorCharPos - 2) + 1;
                FCursorCharPos = Math.Max(0, lastSpace);
            }
        }
        #endregion

        #region Commands
        private void AddNewChar(string str)
        {
            FText = FText.Insert(FCursorCharPos, str);

            if (str.ToUpper() == str)
                FLastCapitalKey = str;

            CursorStepsRight(str.Length);
        }

        private void DeleteLeftChar()
        {
            if (FCursorCharPos > 0)
            {
                CursorStepsLeft();
                DeleteRightChar();
            }
        }

        private void DeleteRightChar()
        {
            var charCount = 1;
            if (FText[FCursorCharPos] == '\r' && FNewlineSymbol.Length > 1)
                charCount = FNewlineSymbol.Length;

            FText = FText.Remove(FCursorCharPos, charCount);
        }

        private bool RunCommand(Keys keyCode)
        {
            try
            {
                switch (keyCode)
                {
                    case Keys.None:
                        return true;
                    case Keys.Return:
                        AddNewChar(FNewlineSymbol);
                        return true;
                    case Keys.Back:
                        DeleteLeftChar();
                        return true;
                    case Keys.Delete:
                        DeleteRightChar();
                        return true;
                    case Keys.Tab:
                        AddNewChar(" ");
                        AddNewChar(" ");
                        AddNewChar(" ");
                        AddNewChar(" ");
                        AddNewChar(" ");
                        return true;
                    case Keys.Left:
                        if (!IgnoreNavigationKeys)
                            if (FControlKeyPressed)
                                CursorStepsWordLeft();
                            else
                                CursorStepsLeft();
                        return true;
                    case Keys.Right:
                        if (!IgnoreNavigationKeys)
                            if (FControlKeyPressed)
                                CursorStepsWordRight();
                            else
                                CursorStepsRight();
                        return true;
                    case Keys.Up:
                        if (!IgnoreNavigationKeys)
                            CursorOneLineUp();
                        return true;
                    case Keys.Down:
                        if (!IgnoreNavigationKeys)
                            CursorOneLineDown();
                        return true;
                    case Keys.Home:
                        if (!IgnoreNavigationKeys)
                            CursorToLineStart();
                        return true;
                    case Keys.End:
                        if (!IgnoreNavigationKeys)
                            CursorToLineEnd();
                        return true;
                    case Keys.Prior:
                        if (!IgnoreNavigationKeys)
                            CursorToTextStart();
                        return true;
                    case Keys.Next:
                        if (!IgnoreNavigationKeys)
                            CursorToTextEnd();
                        return true;
                }
            }
            catch { };
            return false;
        }
        #endregion

        public void Initialize(string text)
        {
            FText = text;
            CursorToTextEnd();
        }

        public void InsertText(string text)
        {
            AddNewChar(text);
        }

        public void Dispose()
        {
            if (FKeyboardSubscription != null)
            {
                FKeyboardSubscription.Dispose();
                FKeyboardSubscription = null;
            }
        }
    }
}