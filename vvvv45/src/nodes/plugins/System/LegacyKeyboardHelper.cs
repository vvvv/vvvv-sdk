using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.Nodes.Input
{
    static class LegacyKeyboardHelper
    {
        public static string VirtualKeycodeToString(Keys key)
        {
            switch (key)
            {
                case Keys.LButton: return "<LBUTTON>";
                case Keys.RButton: return "<RBUTTON>";
                case Keys.Cancel: return "<CANCEL>";
                case Keys.MButton: return "<MBUTTON>";
                case Keys.Back: return "<BACK>";
                case Keys.Tab: return "<TAB>";
                case Keys.Clear: return "<CLEAR>";
                case Keys.Return: return "<RETURN>";
                case Keys.ShiftKey: return "<SHIFT>";
                case Keys.ControlKey: return "<CONTROL>";
                case Keys.Menu: return "<ALT>";
                case Keys.Pause: return "<PAUSE>";
                case Keys.Capital: return "<CAPITAL>";
                case Keys.HangulMode: return "<HANGULKANA>";   // Keys.KANA = Keys.HANGUL !!
                case Keys.JunjaMode: return "<JUNJA>";
                case Keys.FinalMode: return "<FINAL>";
                case Keys.KanjiMode: return "<KANJIHANJA>"; // Keys.HANJA = Keys.KANJI !!
                case Keys.IMEConvert: return "<CONVERT>";
                case Keys.IMENonconvert: return "<NONCONVERT>";
                case Keys.IMEAccept: return "<ACCEPT>";
                case Keys.IMEModeChange: return "<MODECHANGE>";
                case Keys.Escape: return "<ESCAPE>";
                case Keys.Space: return "<SPACE>";
                case Keys.Prior: return "<PRIOR>";
                case Keys.Next: return "<NEXT>";
                case Keys.End: return "<END>";
                case Keys.Home: return "<HOME>";
                case Keys.Left: return "<LEFT>";
                case Keys.Up: return "<UP>";
                case Keys.Right: return "<RIGHT>";
                case Keys.Down: return "<DOWN>";
                case Keys.Select: return "<SELECT>";
                case Keys.Print: return "<PRINT>";
                case Keys.Execute: return "<EXECUTE>";
                case Keys.Snapshot: return "<SNAPSHOT>";
                case Keys.Insert: return "<INSERT>";
                case Keys.Delete: return "<DELETE>";
                case Keys.Help: return "<HELP>";
                case Keys.LWin: return "<LWIN>";
                case Keys.RWin: return "<RWIN>";
                case Keys.Apps: return "<APPS>";
                case Keys.NumPad0: return "<NUMPAD0>";
                case Keys.NumPad1: return "<NUMPAD1>";
                case Keys.NumPad2: return "<NUMPAD2>";
                case Keys.NumPad3: return "<NUMPAD3>";
                case Keys.NumPad4: return "<NUMPAD4>";
                case Keys.NumPad5: return "<NUMPAD5>";
                case Keys.NumPad6: return "<NUMPAD6>";
                case Keys.NumPad7: return "<NUMPAD7>";
                case Keys.NumPad8: return "<NUMPAD8>";
                case Keys.NumPad9: return "<NUMPAD9>";
                case Keys.Multiply: return "<MULTIPLY>";
                case Keys.Add: return "<ADD>";
                case Keys.Separator: return "<SEPARATOR>";
                case Keys.Subtract: return "<SUBTRACT>";
                case Keys.Decimal: return "<DECIMAL>";
                case Keys.Divide: return "<DIVIDE>";
                case Keys.F1: return "<F1>";
                case Keys.F2: return "<F2>";
                case Keys.F3: return "<F3>";
                case Keys.F4: return "<F4>";
                case Keys.F5: return "<F5>";
                case Keys.F6: return "<F6>";
                case Keys.F7: return "<F7>";
                case Keys.F8: return "<F8>";
                case Keys.F9: return "<F9>";
                case Keys.F10: return "<F10>";
                case Keys.F11: return "<F11>";
                case Keys.F12: return "<F12>";
                case Keys.F13: return "<F13>";
                case Keys.F14: return "<F14>";
                case Keys.F15: return "<F15>";
                case Keys.F16: return "<F16>";
                case Keys.F17: return "<F17>";
                case Keys.F18: return "<F18>";
                case Keys.F19: return "<F19>";
                case Keys.F20: return "<F20>";
                case Keys.F21: return "<F21>";
                case Keys.F22: return "<F22>";
                case Keys.F23: return "<F23>";
                case Keys.F24: return "<F24>";
                case Keys.NumLock: return "<NUMLOCK>";
                case Keys.Scroll: return "<SCROLL>";
                case Keys.LShiftKey: return "<LSHIFT>";
                case Keys.RShiftKey: return "<RSHIFT>";
                case Keys.LControlKey: return "<LCONTROL>";
                case Keys.RControlKey: return "<RCONTROL>";
                case Keys.LMenu: return "<LMENU>";
                case Keys.RMenu: return "<RMENU>";
                case Keys.ProcessKey: return "<PROCESSKEY>";
                case Keys.Attn: return "<ATTN>";
                case Keys.Crsel: return "<CRSEL>";
                case Keys.Exsel: return "<EXSEL>";
                case Keys.EraseEof: return "<EREOF>";
                case Keys.Play: return "<PLAY>";
                case Keys.Zoom: return "<ZOOM>";
                case Keys.NoName: return "<NONAME>";
                case Keys.Pa1: return "<PA1>";
                case Keys.OemClear: return "<OEM_CLEAR>";
                case Keys.D0: return "0";
                case Keys.D1: return "1";
                case Keys.D2: return "2";
                case Keys.D3: return "3";
                case Keys.D4: return "4";
                case Keys.D5: return "5";
                case Keys.D6: return "6";
                case Keys.D7: return "7";
                case Keys.D8: return "8";
                case Keys.D9: return "9";
                default:
                    if (key >= Keys.A && key <= Keys.Z)
                        return char.ToString((char)(key));
                    return "<KEY" + (int)key + ">";
            }
        }

        public static Keys StringToVirtualKeycode(string key)
        {
            if (string.IsNullOrEmpty(key)) return Keys.None;
            var c = key[0];
            if (char.IsLetterOrDigit(c)) return (Keys)char.ToUpperInvariant(c);
            switch (key)
            {
                case "<LBUTTON>": return Keys.LButton;
                case "<RBUTTON>": return Keys.RButton;
                case "<CANCEL>": return Keys.Cancel;
                case "<MBUTTON>": return Keys.MButton;
                case "<BACK>": return Keys.Back;
                case "<TAB>": return Keys.Tab;
                case "<CLEAR>": return Keys.Clear;
                case "<RETURN>": return Keys.Return;
                case "<SHIFT>": return Keys.ShiftKey;
                case "<CONTROL>": return Keys.ControlKey;
                case "<ALT>": return Keys.Menu;
                case "<PAUSE>": return Keys.Pause;
                case "<CAPITAL>": return Keys.Capital;
                case "<HANGULKANA>": return Keys.HangulMode;
                case "<JUNJA>": return Keys.JunjaMode;
                case "<FINAL>": return Keys.FinalMode;
                case "<KANJIHANJA>": return Keys.KanjiMode;
                case "<CONVERT>": return Keys.IMEConvert;
                case "<NONCONVERT>": return Keys.IMENonconvert;
                case "<ACCEPT>": return Keys.IMEAccept;
                case "<MODECHANGE>": return Keys.IMEModeChange;
                case "<ESCAPE>": return Keys.Escape;
                case "<SPACE>": return Keys.Space;
                case "<PRIOR>": return Keys.Prior;
                case "<NEXT>": return Keys.Next;
                case "<END>": return Keys.End;
                case "<HOME>": return Keys.Home;
                case "<LEFT>": return Keys.Left;
                case "<UP>": return Keys.Up;
                case "<RIGHT>": return Keys.Right;
                case "<DOWN>": return Keys.Down;
                case "<SELECT>": return Keys.Select;
                case "<PRINT>": return Keys.Print;
                case "<EXECUTE>": return Keys.Execute;
                case "<SNAPSHOT>": return Keys.Snapshot;
                case "<INSERT>": return Keys.Insert;
                case "<DELETE>": return Keys.Delete;
                case "<HELP>": return Keys.Help;
                case "<LWIN>": return Keys.LWin;
                case "<RWIN>": return Keys.RWin;
                case "<APPS>": return Keys.Apps;
                case "<NUMPAD0>": return Keys.NumPad0;
                case "<NUMPAD1>": return Keys.NumPad1;
                case "<NUMPAD2>": return Keys.NumPad2;
                case "<NUMPAD3>": return Keys.NumPad3;
                case "<NUMPAD4>": return Keys.NumPad4;
                case "<NUMPAD5>": return Keys.NumPad5;
                case "<NUMPAD6>": return Keys.NumPad6;
                case "<NUMPAD7>": return Keys.NumPad7;
                case "<NUMPAD8>": return Keys.NumPad8;
                case "<NUMPAD9>": return Keys.NumPad9;
                case "<MULTIPLY>": return Keys.Multiply;
                case "<ADD>": return Keys.Add;
                case "<SEPARATOR>": return Keys.Separator;
                case "<SUBTRACT>": return Keys.Subtract;
                case "<DECIMAL>": return Keys.Decimal;
                case "<DIVIDE>": return Keys.Divide;
                case "<F1>": return Keys.F1;
                case "<F2>": return Keys.F2;
                case "<F3>": return Keys.F3;
                case "<F4>": return Keys.F4;
                case "<F5>": return Keys.F5;
                case "<F6>": return Keys.F6;
                case "<F7>": return Keys.F7;
                case "<F8>": return Keys.F8;
                case "<F9>": return Keys.F9;
                case "<F10>": return Keys.F10;
                case "<F11>": return Keys.F11;
                case "<F12>": return Keys.F12;
                case "<F13>": return Keys.F13;
                case "<F14>": return Keys.F14;
                case "<F15>": return Keys.F15;
                case "<F16>": return Keys.F16;
                case "<F17>": return Keys.F17;
                case "<F18>": return Keys.F18;
                case "<F19>": return Keys.F19;
                case "<F20>": return Keys.F20;
                case "<F21>": return Keys.F21;
                case "<F22>": return Keys.F22;
                case "<F23>": return Keys.F23;
                case "<F24>": return Keys.F24;
                case "<NUMLOCK>": return Keys.NumLock;
                case "<SCROLL>": return Keys.Scroll;
                case "<LSHIFT>": return Keys.LShiftKey;
                case "<RSHIFT>": return Keys.RShiftKey;
                case "<LCONTROL>": return Keys.LControlKey;
                case "<RCONTROL>": return Keys.RControlKey;
                case "<LMENU>": return Keys.LMenu;
                case "<RMENU>": return Keys.RMenu;
                case "<PROCESSKEY>": return Keys.ProcessKey;
                case "<ATTN>": return Keys.Attn;
                case "<CRSEL>": return Keys.Crsel;
                case "<EXSEL>": return Keys.Exsel;
                case "<EREOF>": return Keys.EraseEof;
                case "<PLAY>": return Keys.Play;
                case "<ZOOM>": return Keys.Zoom;
                case "<NONAME>": return Keys.NoName;
                case "<PA1>": return Keys.Pa1;
                case "<OEM_CLEAR>": return Keys.OemClear;
                default:
                    return (Keys)(int.Parse(key.Substring(5, key.Length - 5)));
            }
        }
    }
}
