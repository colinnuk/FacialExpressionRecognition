using System;
using System.Collections.Generic;
using System.Windows.Forms;


namespace FYP
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application. Sets visual styles and text rendering
        /// settings, and creates an instance of the main program form by calling 'Webcam()'.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();  //Enables Visual Styles, for use by Windows
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Webcam());  // Starts the main program
            //Application.Run(new TestGround());  //Runs Testing Ground
            //Application.Run(new TestHarness());  //Runs Test Harness
        }
    }
}
