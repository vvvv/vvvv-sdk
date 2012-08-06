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
            using (Form1 tForm = new Form1())
            {
                try
                {
                    tForm.InitializeGraphics();
                }
                catch (Microsoft.DirectX.DirectXException ex)
                {
                    MessageBox.Show("Unable to initialize DirectX.");
                    tForm.Dispose();
                    return;
                }

                Application.Idle += new EventHandler(tForm.OnApplicationIdle);
                Application.Run(tForm);
            }
        }
    }
}