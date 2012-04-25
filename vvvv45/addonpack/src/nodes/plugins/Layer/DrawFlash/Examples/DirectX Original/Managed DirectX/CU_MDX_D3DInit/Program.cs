using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CUnit
{
    class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using ( Form1 form = new Form1() )
            {
                if ( !form.InitializeGraphics() )
                {
                    MessageBox.Show( "Unable to initialize DirectX." );
                    form.Dispose();
                    return;
                }
                Application.Idle += new EventHandler( form.OnApplicationIdle );
                Application.Run( form );
            }
        }
    }
}