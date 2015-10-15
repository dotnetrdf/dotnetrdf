/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VDS.RDF.Utilities.StoreManager.Connections;
using VDS.RDF.Utilities.StoreManager.Forms;
using VDS.RDF.Utilities.StoreManager.Properties;

namespace VDS.RDF.Utilities.StoreManager
{
    internal static class Program
    {
        private static object _lock = new Object();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new ManagerForm();
            Application.Run(MainForm);
        }

        /// <summary>
        /// Gets the main form
        /// </summary>
        public static ManagerForm MainForm { get; private set; }

        /// <summary>
        /// Gets the active connections by examining the open store manager forms
        /// </summary>
        public static IEnumerable<Connection> ActiveConnections
        {
            get
            {
                if (MainForm != null)
                {
                    return (from managerForm in MainForm.MdiChildren.OfType<StoreManagerForm>()
                            select managerForm.Connection);
                }
                return Enumerable.Empty<Connection>();
            }
        }

        /// <summary>
        /// Generic internal error handler
        /// </summary>
        /// <param name="message">Friendly message to display to the user</param>
        /// <param name="ex">Exception</param>
        public static void HandleInternalError(String message, Exception ex)
        {
            HandleInternalError(message, ex, false);
        }

        /// <summary>
        /// Generic internal error handler
        /// </summary>
        /// <param name="message">Friendly message to display to the user</param>
        /// <param name="ex">Exception</param>
        /// <param name="exit">Whether the program should now exit</param>
        public static void HandleInternalError(String message, Exception ex, bool exit)
        {
            TryLogError(message, ex, exit);
            if (exit)
            {
                MessageBox.Show(String.Format(Resources.HandleInternalError_Exit, message), Resources.Internal_Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(1);
            }
            else
            {
                MessageBox.Show(String.Format(Resources.HandleInternalError_NonExit, message), Resources.Internal_Error, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Generic internal error handler
        /// </summary>
        /// <param name="message">Friendly message to display to the user</param>
        public static void HandleInternalError(string message)
        {
            HandleInternalError(message, null);
        }

        /// <summary>
        /// Tries to log an errors to the errors.log file
        /// </summary>
        /// <param name="message">Error Message</param>
        /// <param name="ex">Exception</param>
        /// <param name="exit">Whether the error required the program to exit</param>
        private static void TryLogError(String message, Exception ex, bool exit)
        {
            lock (_lock)
            {
                try
                {
                    String logPath = Path.Combine(GetApplicationDataDirectory(), "errors.log");
                    using (StreamWriter writer = new StreamWriter(logPath, true, Encoding.UTF8))
                    {
                        writer.Write(exit ? "FATAL" : "ERROR");
                        writer.Write(' ');
                        writer.Write(DateTime.Now.ToString(CultureInfo.CurrentUICulture));
                        writer.Write(' ');
                        writer.WriteLine(message);
                        while (ex != null)
                        {
                            writer.WriteLine(ex.Message);
                            writer.WriteLine(ex.StackTrace);
                            ex = ex.InnerException;
                        }
                        writer.Close();
                    }
                }
                catch
                {
                    // Ignore errors trying to log errors
                }
            }
        }

        /// <summary>
        /// Gets the application data directory used for the program
        /// </summary>
        /// <returns></returns>
        public static string GetApplicationDataDirectory()
        {
            String appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            String sepChar = new String(new char[] {Path.DirectorySeparatorChar});
            if (!appDataDir.EndsWith(sepChar)) appDataDir += sepChar;
            appDataDir = Path.Combine(appDataDir, "dotNetRDF" + sepChar);
            if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
            appDataDir = Path.Combine(appDataDir, "Store Manager" + sepChar);
            if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
            return appDataDir;
        }
    }
}