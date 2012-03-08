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

using VVVV.Utils;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;

using VVVV.Core.Logging;

namespace TypeWriter
{
    /// <summary>
    /// Способ разбиения последовательности вводимых символов
    /// </summary>
    public enum eSeparate { None = 1, Return, Word, Character, Key, Text };

    /// <summary>
    /// Параметр удаления
    /// </summary>
    public enum eDelete {Character, Word, Slice, All};

    [PluginInfo(Name = "Typewriter", 
                Category = "String", 
                Help = "", 
                Tags = "")]
    public class TypeWriterE : IPluginEvaluate
    {
        [DllImport("user32.dll")]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
        
        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);

        [DllImport("user32.dll")]
        static extern IntPtr GetKeyboardLayout(uint idThread);

        [DllImport("user32.dll")]
        static extern short GetKeyState(Keys nVirtKey);

        public static bool IsKeyPressed(Keys testKey)
        {
            bool keyPressed = false;

            short result = GetKeyState(testKey);

            switch (result)
            {
                case 0:
                    // Not pressed and not toggled on.
                    keyPressed = false;
                    break;

                case 1:
                    // Not pressed, but toggled on
                    keyPressed = false;
                    break;

                default:
                    // Pressed (and may be toggled on)
                    keyPressed = true;
                    break;
            }

            return keyPressed;
        }
        public static string VKCodeToUnicode(uint VKCode)
        {
            System.Text.StringBuilder sbString = new System.Text.StringBuilder();

            byte[] bKeyState = new byte[255];
            bool bKeyStateStatus = GetKeyboardState(bKeyState);
            if (!bKeyStateStatus)
                return "";
            uint lScanCode = MapVirtualKey(VKCode, 0);
            IntPtr HKL = GetKeyboardLayout(0);

            ToUnicodeEx(VKCode, lScanCode, bKeyState, sbString, (int)5, (uint)0, HKL);
            return sbString.ToString();
        }

        #region field declaration
        //input pin declaration
        [Input("String")]
        IDiffSpread<string> FKeyboardInput;
        
        [Input("KeyCode", DefaultValue = 0)]
        IDiffSpread<double> FInputKey;
        
        [Input("Doubles Speed", MinValue = 0, MaxValue = 1, StepSize = 0.01d, DefaultValue = 0)]
        IDiffSpread<double> FDoublesSpeed;
        
        [Input("Text")]
        IDiffSpread<string> FText;
        
        [Input("Replace", IsSingle = true, IsBang = true, AsInt = true, DefaultValue = 0)]
        IDiffSpread<bool> FReplace;
        
        [Input("Cursor Position", IsSingle = true, MinValue = 0, MaxValue = double.MaxValue, StepSize = 1, DefaultValue = 0, AsInt = true)]
        IDiffSpread<int> FCursorPosition;
        
        [Input("Separate by")]
        protected IDiffSpread<eSeparate> FSeparateEnum;
        
        [Input("Cursor")]
        IDiffSpread<string> FCursor;
        
        [Input("Delete by")]
        IDiffSpread<eDelete> FDeleteEnum;
        
        [Input("Delete", IsSingle = true, IsBang = true, AsInt = true, DefaultValue = 0)]
        IDiffSpread<bool> FDelete;

        //output pin declaration
        [Output("Output")]
        ISpread<string> FUsedOutput;
        
        [Output("Output with Cursor")]
        ISpread<string> FViewOutput;
        
        [Output("Active Slice", IsSingle = true, MinValue = 0, MaxValue = double.MaxValue)]
        ISpread<int> FActiveSlice;
        
        [Output("Error", IsSingle = true, MinValue = 0, MaxValue = 1, DefaultValue = 0, IsBang = false, AsInt = true)]
        ISpread<bool> FError;

        [Import()]
        ILogger Flogger;
        
        /// <summary>
        /// Рабочий список
        /// </summary>
        public List<List<string>> SlicesString = new List<List<string>>();
        /// <summary>
        /// Входная строка
        /// </summary>
        public string KbdInput = string.Empty;
        /// <summary>
        /// Скорость повтора
        /// </summary>
        public double DSpeed = 0;
        /// <summary>
        /// Разделитель
        /// </summary>
        public eSeparate SeparateEnum;
        
        /// <summary>
        /// Контрольный ключ
        /// </summary>
        public string PKey = string.Empty;
        /// <summary>
        /// Курсор
        /// </summary>
        public string Cursor = string.Empty;
        /// <summary>
        /// Позиция курсора в строке
        /// </summary>
        public int xCur = 0;
        /// <summary>
        /// Номер строки курсора
        /// </summary>
        public int yCur = 0;
        /// <summary>
        /// Номер текущего слайса
        /// </summary>
        public int ActiveSlice = 0;
        /// <summary>
        /// Способ удаления
        /// </summary>
        public eDelete DeleteEnum;
        /// <summary>
        /// запоминаем введенный символ
        /// </summary>
        private string LastChar;
        /// <summary>
        /// последняя команда
        /// </summary>
        private uint LastScanCode;
        private uint DrebezgScanCode;
        private bool Invalidate = false;
        private bool FlagTextConnected = false;
        private bool FlagStringConnected = false;

        // Делегат, вызывающий конец паузы по таймеру  
        TimerCallback timerDelegate;
        /// <summary>
        /// Переинициализация модема
        /// </summary>
        private System.Threading.Timer RepeatTimer;
        private int timerdelay = 0;
        
        // Track whether Dispose has been called.
        private bool FDisposed = false;
        #endregion field declaration

        #region constructor/destructor

        [ImportingConstructor]
        public TypeWriterE(IPluginHost pluginHost)
        {
            pluginHost.UpdateEnum("Separate", "None", Enum.GetNames(typeof(eSeparate)));
            pluginHost.UpdateEnum("Delete", "Character", Enum.GetNames(typeof(eDelete)));
        }

        public TypeWriterE()
        {
            /// Добавляем пустую строку на старте
            List<string> ls = new List<string>();
            ls.Add(string.Empty);
            SlicesString.Add(ls);
            timerDelegate = new TimerCallback(RepeatChar);
            RepeatTimer = new System.Threading.Timer(timerDelegate, 1, Timeout.Infinite, Timeout.Infinite);
        }

        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
            RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
            RepeatTimer.Dispose();
            for (int i = 0; i < SlicesString.Count; i++)
                SlicesString[i].Clear();
            SlicesString.Clear();

            // Check to see if Dispose has already been called.
            if (!FDisposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                Flogger.Log(LogType.Message, "TypeWriter is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~TypeWriterE()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }
        #endregion constructor/destructor

        #region Ошибки
        /// <summary>
        /// Список проверки ошибок
        /// </summary>
        /// <returns></returns>
        public bool ErrorSlice()
        {
            try
            {
                if (ValidateEnum == eValidate.None) return false;
                if (ValidateEnum == eValidate.E_mail)
                    return ErrorEmail(SlicesString[ActiveSlice][yCur]);
                if (ValidateEnum == eValidate.FileName)
                    return ErrorFileName(SlicesString[ActiveSlice][yCur]);
                if (ValidateEnum == eValidate.HTTP)
                    return ErrorHTTP(SlicesString[ActiveSlice][yCur]);
                if (ValidateEnum == eValidate.IP_Address)
                    return ErrorIP(SlicesString[ActiveSlice][yCur]);
                if (ValidateEnum == eValidate.Key)
                    return ErrorKey(SlicesString[ActiveSlice][yCur]);
            }
            catch { };
            return false;
        }

        /// <summary>
        /// Проверка адреса почты на ошибки
        /// </summary>
        /// <param name="str"></param>
        public bool ErrorEmail(string str)
        {
            if (str == null || str == "") return false;
            string[] errstr = str.Split('@');
            if (errstr.Length > 2) return true;
            if (errstr[0].Length > 80) return true;
            if (str.Contains("..")) return true;
            if (str.Contains("--")) return true;
            if (str.Contains("__")) return true;
            if (str.Contains(" ")) return true;
            if (str.Contains("~")) return true;
            if (str.Contains("`")) return true;
            if (str.Contains("$")) return true;
            if (str.Contains("^")) return true;
            if (str.Contains("=")) return true;
            if (str.Contains("|")) return true;
            if (str.Contains("<")) return true;
            if (str.Contains(">")) return true;
            if (str.Contains("#")) return true;
            if (str.Length > 0 && str[0] == '@') return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsControl(str[i]) && (str[i] != '@' && str[i] != '.' && str[i] != '-' && str[i] != '_')) return true;
                if (char.IsPunctuation(str[i]) && (str[i] != '@' && str[i] != '.' && str[i] != '-' && str[i] != '_')) return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка адреса IP
        /// </summary>
        /// <param name="str"></param>
        public bool ErrorIP(string str)
        {
            if (str == null || str == "") return false;
            if (str.Contains("..")) return true;
            if (str.Length > 0 && !char.IsDigit(str[0])) return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (!char.IsDigit(str[i]) && str[i] != '.') return true;
            }
            string[] errstr = str.Split('.');
            for (int i = 0; i < errstr.Length; i++)
            {
                if (errstr[i].Length > 3) return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка адреса HTTP
        /// </summary>
        /// <param name="str"></param>
        public bool ErrorHTTP(string str)
        {
            if (str == null || str == "") return false;
            /// адрес не может содержать несколько разделительных символов подряд
            if (str.Contains("--")) return true;
            if (str.Contains("..")) return true;
            if (str.Contains("\\")) return true;
            if (str.Contains(" ")) return true;
            if (str.Contains("~")) return true;
            if (str.Contains("`")) return true;
            if (str.Contains("$")) return true;
            if (str.Contains("^")) return true;
            if (str.Contains("=")) return true;
            if (str.Contains("|")) return true;
            if (str.Contains("<")) return true;
            if (str.Contains(">")) return true;
            if (str.Contains("!")) return true;
            if (str.Contains("%")) return true;
            if (str.Contains("@")) return true;
            if (str.Contains("#")) return true;
            if (str.Contains("::")) return true;
            if (str.Contains(":"))
            {
                string[] subs = str.Split(':');
                /// проверяем префикс
                if (subs[0] != "ftp" && subs[0] != "FTP" &&
                    subs[0] != "http" && subs[0] != "HTTP") return true;
            }

            if (str.Length > 0 && !char.IsLetter(str[0])) return true;

            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsControl(str[i]) && str[i] != '.' && str[i] != '-' && str[i] != '/') return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка имени файла
        /// </summary>
        /// <param name="str"></param>
        public bool ErrorFileName(string str)
        {
            if (str == null || str == "") return false;
            if (str.Contains("..")) return true;
            if (str.Contains("/")) return true;
            if (str.Contains("\\\\")) return true;
            if (str.Contains(" ")) return true;
            if (str.Contains("`")) return true;
            if (str.Contains("$")) return true;
            if (str.Contains("@")) return true;
            if (str.Contains("!")) return true;
            if (str.Contains("&")) return true;
            if (str.Contains("*")) return true;
            if (str.Contains("+")) return true;
            if (str.Contains("^")) return true;
            if (str.Contains("=")) return true;
            if (str.Contains("|")) return true;
            if (str.Contains("<")) return true;
            if (str.Contains(">")) return true;
            if (str.Contains("#")) return true;
            if (str.Split(':').Length > 2) return true;
            if (!char.IsLetterOrDigit(str[0])) return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (char.IsControl(str[i]) && str[i] != '.' && str[i] != '\\') return true;
            }
            return false;
        }

        /// <summary>
        /// Проверка ключа
        /// </summary>
        /// <param name="str"></param>
        public bool ErrorKey(string str)
        {
            if (str == null || str == "") return false;
            if (str.Length > PKey.Length) return true;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] != PKey[i]) return true;
            }
            return false;
        }
        #endregion

        #region навигация курсора | Cursor
        /// <summary>
        /// Курсор на начало текста
        /// </summary>
        void StartTextCursor()
        {
            ActiveSlice = xCur = yCur = 0;
        }

        /// <summary>
        /// Курсор в конец текста
        /// </summary>
        void EndTextCursor()
        {
            try
            {
                ActiveSlice = SlicesString.Count - 1;
                yCur = SlicesString[ActiveSlice].Count - 1;
                if (yCur < 0) yCur = 0;
                xCur = SlicesString[ActiveSlice][yCur].Length;
                if (xCur < 0) xCur = 0;
            }
            catch { };
        }

        /// <summary>
        /// Перемещение курсора вверх по строкам
        /// </summary>
        void UpCursor()
        {
            try
            {
                yCur--;
                if (yCur < 0)
                {
                    if (ActiveSlice > 0)
                    {
                        ActiveSlice--;
                        yCur = SlicesString[ActiveSlice].Count - 1;
                    }
                    else
                        yCur = 0;
                }
                if (xCur > SlicesString[ActiveSlice][yCur].Length)
                    xCur = SlicesString[ActiveSlice][yCur].Length;
                if (xCur < 0) xCur = 0;
            }
            catch { };
        }

        /// <summary>
        /// Перемещение курсора вниз по строкам
        /// </summary>
        void DownCursor()
        {
            try
            {
                yCur++;
                if (yCur >= SlicesString[ActiveSlice].Count)
                {
                    if (ActiveSlice < SlicesString.Count - 1)
                    {
                        ActiveSlice++;
                        yCur = 0;
                    }
                    else
                        yCur = SlicesString[ActiveSlice].Count - 1;
                }
                if (xCur > SlicesString[ActiveSlice][yCur].Length)
                    xCur = SlicesString[ActiveSlice][yCur].Length;
                if (xCur < 0) xCur = 0;
            }
            catch { };
        }
        /// <summary>
        /// Курсор в начало строки
        /// </summary>
        void StartStrCursor()
        {
            xCur = 0;
        }
        /// <summary>
        /// Курсор в конец строки
        /// </summary>
        void EndStrCursor()
        {
            try
            {
                xCur = SlicesString[ActiveSlice][yCur].Length;
            }
            catch { };
        }
        /// <summary>
        /// Перемещение курсора вправо
        /// </summary>
        void NextCursor()
        {
            try
            {
                xCur++;
                /// если курсор дальше длины строки пробуем перенести на новую строку
                if (xCur > SlicesString[ActiveSlice][yCur].Length)
                {
                    /// если есть следующая строка
                    if (yCur + 1 < SlicesString[ActiveSlice].Count)
                    {
                        xCur = 0;
                        yCur++;
                    }
                    else if (ActiveSlice < SlicesString.Count - 1)
                    {
                        ActiveSlice++;
                        xCur = yCur = 0;
                    }
                    else
                    {
                        /// если нет то не даем сдвигать курсор
                        xCur = SlicesString[ActiveSlice][yCur].Length;
                    }
                }
            }
            catch { };
        }
        /// <summary>
        /// Перемещение курсора влево
        /// </summary>
        void PrevCursor()
        {
            try
            {
                xCur--;
                /// если курсор меньше 0 пробуем перенести на строку выше
                if (xCur < 0)
                {
                    /// если есть следующая строка
                    if (yCur > 0)
                    {
                        yCur--;
                        xCur = SlicesString[ActiveSlice][yCur].Length;
                    }
                    else if (ActiveSlice > 0)
                    {
                        ActiveSlice--;
                        yCur = SlicesString[ActiveSlice].Count - 1;
                        xCur = SlicesString[ActiveSlice][yCur].Length;
                    }
                    else
                    {
                        /// если нет то не даем сдвигать курсор
                        xCur = 0;
                    }
                }
            }
            catch { };
        }
        #endregion

        #region таймеры дребезга и повтора
        /// <summary>
        /// Повтор символа
        /// </summary>
        /// <param name="stateInfo"></param>
        public void RepeatChar(Object stateInfo)
        {
            try
            {
                int sInfo = 0;
                sInfo = (int)stateInfo;

                if (sInfo == 1)
                {
                    if (LastChar != null)
                    {
                        NewCommand(LastChar);
                        Invalidate = true;
                    }
                    if (LastScanCode != 0)
                    {
                        RunCommand(LastScanCode);
                        Invalidate = true;
                    }
                }
            }
            catch { };
        }

        #endregion

        #region обработка входного текста
        /// <summary>
        /// Создание нового слайса
        /// </summary>
        private void NewSlice()
        {
            try
            {
                List<string> st = new List<string>();
                st.Add(string.Empty);
                SlicesString.Add(st);
                EndTextCursor();
            }
            catch { };
        }

        /// <summary>
        /// Вставка нового слайса текста
        /// </summary>
        private void InsertSliceText(int cur)
        {
            try
            {
                if (cur < 0) cur = 0;
                if (cur > SlicesString.Count - 1) cur = SlicesString.Count - 1;
                // новый слайс
                List<string> st = new List<string>();
                st.Add(string.Empty);
                if (cur + 1 == SlicesString.Count)
                    SlicesString.Add(st);
                else
                    SlicesString.Insert(cur + 1, st);
                ActiveSlice++;
                StartStrCursor();
            }
            catch { };
        }

        /// <summary>
        /// Удаление символа слева от курсора
        /// </summary>
        private void DeleteLeftChar()
        {
            try
            {
                // удаление слева от курсора
                if (xCur != 0)
                {
                    SlicesString[ActiveSlice][yCur] = SlicesString[ActiveSlice][yCur].Remove(xCur - 1, 1);
                    PrevCursor();
                }
            }
            catch { };
        }
        /// <summary>
        /// Удаление символа справа от курсора
        /// </summary>
        private void DeleteRightChar()
        {
            try
            {
                // удаление справа от курсора
                if (xCur != SlicesString[ActiveSlice][yCur].Length)
                {
                    SlicesString[ActiveSlice][yCur] = SlicesString[ActiveSlice][yCur].Remove(xCur, 1);
                }
            }
            catch { };
            //OutPins();
        }
        /// <summary>
        /// Удаление слова слева
        /// </summary>
        private void DeleteLeftWord()
        {
            try
            {
                while (xCur > 0)
                {
                    if (SlicesString[ActiveSlice][yCur][xCur - 1] == ' ') break;
                    string str1 = SlicesString[ActiveSlice][yCur].Remove(xCur - 1);
                    string str2 = SlicesString[ActiveSlice][yCur].Substring(xCur);
                    SlicesString[ActiveSlice][yCur] = str1 + str2;
                    xCur--;
                }
            }
            catch { };
            //OutPins();
        }
        /// <summary>
        /// Удаление слова справа
        /// </summary>
        private void DeleteRightWord()
        {
            try
            {
                while (xCur < SlicesString[ActiveSlice][yCur].Length)
                {
                    if (SlicesString[ActiveSlice][yCur][xCur] == ' ') break;
                    string str1 = SlicesString[ActiveSlice][yCur].Remove(xCur);
                    string str2 = SlicesString[ActiveSlice][yCur].Substring(xCur + 1);
                    SlicesString[ActiveSlice][yCur] = str1 + str2;
                }
            }
            catch { };
            //OutPins();
        }
        /// <summary>
        /// Удаление активного слайса
        /// </summary>
        private void DeleteActiveSlice()
        {
            try
            {
                SlicesString.RemoveAt(ActiveSlice);
                if (SlicesString.Count == 0)
                    NewSlice();
                if (ActiveSlice >= SlicesString.Count)
                    ActiveSlice = SlicesString.Count - 1;
                yCur = xCur = 0;
            }
            catch { };
            //OutPins();
        }

        /// <summary>
        /// Удаление всех слайсов
        /// </summary>
        private void DeleteAllSlices()
        {
            try
            {
                SlicesString.Clear();
                NewSlice();
                ActiveSlice = yCur = xCur = 0;
            }
            catch { };
            //OutPins();
        }

        /// <summary>
        /// Удаление заданного слова в строке
        /// </summary>
        /// <param name="str"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        private string DeleteKeyWord(string str, string word)
        {
            try
            {
                if (str != null)
                {
                    if (str.Contains(word))
                    {
                        str = str.Replace(word, "");
                    }
                }
            }
            catch { };
            return str;
        }

        /// <summary>
        /// Поступление новой команды и ее разбор
        /// </summary>
        /// <param name="str"></param>
        private void NewCommand(string str)
        {
            try
            {
                if (str == null || str == "") return;
                /// добавляем новый текст
                AddNewChar(str);
                /// проверка на разделяющий ключ из управляющих кнопок
                if (SeparateEnum == eSeparate.Key)
                {
                    if (SlicesString[ActiveSlice][yCur].Contains(PKey))
                        NewSlice();
                }
                LastChar = str;
            }
            catch { };
        }

        /// <summary>
        /// Обработка новой команды
        /// </summary>
        /// <param name="ScanCode"></param>
        private bool RunCommand(uint ScanCode)
        {
            try
            {
                /// обработка командных строк
                switch ((Keys)ScanCode)
                {
                    case Keys.Return:
                        if (SeparateEnum == eSeparate.None || SeparateEnum == eSeparate.Key)
                        {
                            // новая строка  в текущем слайсе
                            SlicesString[ActiveSlice].Add(string.Empty);
                            EndTextCursor();
                        }
                        if (SeparateEnum == eSeparate.Return || SeparateEnum == eSeparate.Word)
                            NewSlice();
                        if (SeparateEnum == eSeparate.Text)
                        {
                            DownCursor();
                            StartStrCursor();
                        }
                        return true;
                    case Keys.Back:
                        LastScanCode = ScanCode;
                        DeleteLeftChar();
                        return true;
                    case Keys.Delete:
                        LastScanCode = ScanCode;
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
                        PrevCursor();
                        return true;
                    case Keys.Right:
                        NextCursor();
                        return true;
                    case Keys.Up:
                        UpCursor();
                        return true;
                    case Keys.Down:
                        DownCursor();
                        return true;
                    case Keys.Home:
                        StartStrCursor();
                        return true;
                    case Keys.End:
                        EndStrCursor();
                        return true;
                    case Keys.Prior:
                        StartTextCursor();
                        return true;
                    case Keys.Next:
                        EndTextCursor();
                        return true;
                }
            }
            catch { };
            return false;
        }

        /// <summary>
        /// Добавление символа в текущую позицию курсора
        /// курсор смещаем автоматически вправо
        /// </summary>
        /// <param name="str"></param>
        private void AddNewChar(string str)
        {
            try
            {
                if (str == null || str == "") return;

                if (str == " ")
                {
                    if (SeparateEnum == eSeparate.Word)
                    {
                        NewSlice();
                        return;
                    }
                }
                if (SeparateEnum == eSeparate.Character)
                    NewSlice();
                /// вставляем символ
                if (xCur == SlicesString[ActiveSlice][yCur].Length)
                    SlicesString[ActiveSlice][yCur] += str;
                else
                    SlicesString[ActiveSlice][yCur] = SlicesString[ActiveSlice][yCur].Insert(xCur, str);
                NextCursor();
                /// производим проверку на разделение по ключу в тексте
                if (SeparateEnum == eSeparate.Key)
                {
                    /// да ключ содержится удаляем его в тексте и делаем новый слайс
                    if (SlicesString[ActiveSlice][yCur].Contains(PKey))
                    {
                        SlicesString[ActiveSlice][yCur] = DeleteKeyWord(SlicesString[ActiveSlice][yCur], PKey);
                        NewSlice();
                    }
                }
            }
            catch { };
        }
        #endregion

        #region ВЫВОД
        /// <summary>
        /// Вывод данных
        /// </summary>
        private void OutPins()
        {
            string cr = "\r\n";
            FViewOutput.SliceCount = SlicesString.Count;
            FUsedOutput.SliceCount = SlicesString.Count;

            for (int i = 0; i < SlicesString.Count; i++)
            {
                string viewstr = string.Empty;
                string usedstr = string.Empty;
                for (int j = 0; j < SlicesString[i].Count; j++)
                {
                    if (ActiveSlice == i && yCur == j)
                    {
                        string st = SlicesString[i][j];
                        if (xCur == st.Length)
                            st += Cursor;
                        else
                            st = st.Insert(xCur, Cursor);
                        viewstr += st;
                    }
                    else
                        viewstr += SlicesString[i][j];
                    usedstr += SlicesString[i][j];
                    if (j + 1 != SlicesString[i].Count)
                    {

                        viewstr += cr;
                        usedstr += cr;
                    }
                }

                FViewOutput[i] = viewstr;
                FUsedOutput[i] = usedstr;
            }
            FActiveSlice.SliceCount = 1;
            FActiveSlice[0] = ActiveSlice;

            /// вывод ошибок
            if (ErrorSlice())
                FError[0] = true;
            else
                FError[0] = false;
        }
        #endregion

        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            #region входные параметры
            if (FCursorPosition.IsChanged)
            {
                double a;
                a = FCursorPosition[0];
                if (a < 0) a = 0;
                if (a >= SlicesString.Count)
                    a = SlicesString.Count - 1;
                ActiveSlice = Convert.ToInt32(a);
                xCur = yCur = 0;
                OutPins();
            }

            if (FDoublesSpeed.IsChanged)
            {
                DSpeed = FDoublesSpeed[0];
                // Сбрасываем сторожевой таймер
                if (RepeatTimer != null)
                {
                    if (DSpeed == 0)
                    {
                        RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        timerdelay = 0;
                    }
                    else
                    {
                        timerdelay = Convert.ToInt32(DSpeed * 1000);
                        if (timerdelay > 0)
                        {
                            if (timerdelay < 200) timerdelay = 200;
                            RepeatTimer.Change(timerdelay, timerdelay);
                        }
                    }
                }
            }

            if (FSeparateEnum.IsChanged)
            {
                SeparateEnum = (eSeparate)FSeparateEnum[0];
            }

            if (FCursor.IsChanged)
            {
                Cursor = FCursor[0];
                Cursor = Cursor.Trim();
                OutPins();
            }

            if (FDeleteEnum.IsChanged)
            {
                DeleteEnum = (eDelete)FDeleteEnum[0];
            }
            #endregion

            #region входные данные
            /// входной поток
            //if ((FKeyboardInput.IsChanged && FKeyboardInput.IsConnected) || (FKeyboardInput.IsConnected && FlagStringConnected == false))
            if (FKeyboardInput.IsChanged  || FlagStringConnected == false)
            {
                try
                {
                    if (FKeyboardInput.SliceCount > 0)
                    {
                        string str = FKeyboardInput[0];
                        if (str != null)
                        {
                            for (int i = 0; i < str.Length; i++)
                            {
                                NewCommand(Convert.ToString(str[i]));
                            }
                            OutPins();
                            LastChar = null;
                            LastScanCode = 0;
                            DrebezgScanCode = 0;
                            RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        }
                    }
                }
                catch
                {
                    FlagStringConnected = true;
                    LastChar = null;
                    LastScanCode = 0;
                    DrebezgScanCode = 0;
                    RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                };
            }

            /// входной текст
            //if ((FText.IsChanged && FText.IsConnected) || (FText.IsConnected && FlagTextConnected == false))
            if (FText.IsChanged || FlagTextConnected == false)
            {
                try
                {
                    bool f = FReplace[0];
                    if (f)
                        DeleteAllSlices();

                    string str;
                    for (int i = 0; i < FText.SliceCount; i++)
                    {
                        str = FText[i];
                        InsertSliceText(ActiveSlice);
                        if (str != null)
                        {
                            for (int j = 0; j < str.Length; j++)
                            {
                                /// вставляем символ
                                if (xCur == SlicesString[ActiveSlice][yCur].Length)
                                    SlicesString[ActiveSlice][yCur] += str[j];
                                else
                                    SlicesString[ActiveSlice][yCur] = SlicesString[ActiveSlice][yCur].Insert(xCur, Convert.ToString(str[j]));
                                NextCursor();
                            }
                        }
                    }
                    /// удаляем первый слайс если все замещаем
                    if (Convert.ToBoolean(f) == true)
                    {
                        StartTextCursor();
                        DeleteActiveSlice();
                        EndTextCursor();
                    }
                    OutPins();
                    LastChar = null;
                    LastScanCode = 0;
                    DrebezgScanCode = 0;
                    RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
                catch
                {
                    FlagTextConnected = true;
                };
            }

            if (FInputKey.IsChanged)
            {
                try
                {
                    if (FInputKey.SliceCount > 0)
                    {
                        double a;
                        /// проверка сканкода
                        if (LastScanCode != 0 || LastChar != null)
                        {
                            bool flagScanCode = false;
                            bool flagLastChar = false;
                            for (int i = 0; i < FInputKey.SliceCount; i++)
                            {
                                a = FInputKey[i];
                                uint ScanCode = Convert.ToUInt16(a);
                                if (ScanCode == LastScanCode)
                                    flagScanCode = true;
                                string newchar = VKCodeToUnicode(ScanCode);
                                if (newchar == LastChar)
                                    flagLastChar = true;
                            }
                            if (flagScanCode == false) LastScanCode = 0;
                            if (flagLastChar == false) LastChar = null;
                        }
                        for (int i = 0; i < FInputKey.SliceCount; i++)
                        {
                            a = FInputKey[i];

                            uint ScanCode = Convert.ToUInt16(a);
                            /// все кнопки отпущены
                            if (FInputKey.SliceCount == 1 && ScanCode == 0)
                            {
                                LastChar = null;
                                LastScanCode = 0;
                                DrebezgScanCode = 0;
                                RepeatTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            }
                            string newchar = VKCodeToUnicode(ScanCode);
                            if (DrebezgScanCode == 0)
                            {
                                if (newchar != "" && IsKeyPressed((Keys)ScanCode))
                                    DrebezgScanCode = ScanCode;
                            }
                            else
                            {
                                if (newchar != "" && DrebezgScanCode == ScanCode) return;
                            }
                            if (timerdelay != 0)
                            {
                                if (IsKeyPressed((Keys)ScanCode))
                                    RepeatTimer.Change(timerdelay, timerdelay);
                            }
                            if (RunCommand(ScanCode))
                                continue;
                            NewCommand(newchar);
                            OutPins();
                        }
                    }
                }
                catch { };
            }
            /// команда на удаление
            if (FDelete.IsChanged)
            {
                try
                {
                    bool a = FDelete[0];
                    if (a)
                    {
                        if (DeleteEnum == eDelete.Character)
                            DeleteLeftChar();
                        if (DeleteEnum == eDelete.Word)
                            DeleteLeftWord();
                        if (DeleteEnum == eDelete.Slice)
                            DeleteActiveSlice();
                        if (DeleteEnum == eDelete.All)
                            DeleteAllSlices();
                        OutPins();
                    }
                }
                catch { };
            }
            #endregion
            
            if (Invalidate)
            {
                OutPins();
                Invalidate = false;
            }
        }
    }
}
