/*
 *  Author: Athony La Forge (Validity Systems Inc.) 
 */

using System;
using System.Collections;
using System.Threading;
using System.IO;

namespace VVVV.Webinterface.Utilities
{

    /// <summary>
    /// class definition of the LogType. 
    /// Four Types of Messages for the Logger 
    /// </summary>
    public class LogType
    {
        private string mError = "ERROR";
        private string mDebug = "DEBUG";
        private string mInfo  = "INFO";
        private string mWarn  = "WARN";

        /// <summary>
        /// Logger message type error
        /// </summary>
        public string Error
        {
            get
            {
                return mError;
            }
        }

        /// <summary>
        /// logger message type warn
        /// </summary>
        public string Warn
        {
            get
            {
                return mWarn;
            }
        }

        /// <summary>
        /// logger message type debug
        /// </summary>
        public string Debug
        {
            get
            {
                return mDebug;
            }
        }

        /// <summary>
        /// logger message type info
        /// </summary>
        public string Info
        {
            get
            {
                return mInfo;
            }
        }
    }
    
    
    /// <summary>
    /// Logger class definition 
    /// </summary>
    public class Logger
	{

        StreamWriter stream = null;
		bool append = true;
        LogType mLogType = new LogType();
        

        /// <summary>
        /// logger message type
        /// </summary>
        public LogType LogType
        {
            get
            {
                return mLogType;
            }
        }

       	/// <summary>
		/// Flag to append the text file.  If this flag is not set it will overwrite.
		/// </summary>
		/// <param name="flag"></param>
		public void setAppend(bool flag)
		{
			this.append = flag;
		}

		/// <summary>
		/// Get's append flag
		/// </summary>
		/// <returns></returns>
		public bool getAppend()
		{
			return append;
		}

        /// <summary>
        /// Logger constructor
        /// </summary>
        /// <param name="filename">log file filename</param>
		public Logger(string filename)
		{

            if (filename != null)
            {
                FileMode fm;

                if (append) fm = FileMode.Append;
                else fm = FileMode.Create;

                FileStream fs = new FileStream(filename, fm, FileAccess.Write, FileShare.Read);
                stream = new StreamWriter(fs, System.Text.Encoding.UTF8, 4096);
            }
		}

        /// <summary>
        /// logs a message 
        /// </summary>
        /// <param name="level">Level form the Logtype instance</param>
        /// <param name="message">string message to write</param>
        public void log(string level, string message)
        {
            if (stream == null) return;
            string time = System.DateTime.Now.ToString();
            stream.Write("[" + time + " [" + level + "]: " + message + " \r\n");
            stream.Flush();
        }


        /// <summary>
        /// closes the filestream
        /// </summary>
        public void Shutdown()
        {
            if (stream != null)
            {

                stream.Close();
            }
        }


	}
}
