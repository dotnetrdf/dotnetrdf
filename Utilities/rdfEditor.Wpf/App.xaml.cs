using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows;
using VDS.RDF.Utilities.Editor.Wpf.Properties;

namespace VDS.RDF.Utilities.Editor.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static MruList _recentFiles;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //Try and get the MRU List
            try
            {
                String appDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                String sepChar = new String(new char[] { Path.DirectorySeparatorChar });
                if (!appDataDir.EndsWith(sepChar)) appDataDir += sepChar;
                appDataDir = Path.Combine(appDataDir, "dotNetRDF" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);
                appDataDir = Path.Combine(appDataDir, "rdfEditor" + sepChar);
                if (!Directory.Exists(appDataDir)) Directory.CreateDirectory(appDataDir);

                String mruFile = Path.Combine(appDataDir, "mru.txt");
                _recentFiles = new MruList(mruFile);
            }
            catch
            {
                //Ignore errors here, if this fails then we just won't have a MRU list
            }

            //Try and upgrade user settings if required
            try
            {
                if (Settings.Default.UpgradeRequired)
                {
                    Settings.Default.Upgrade();
                    Settings.Default.UpgradeRequired = false;
                    Settings.Default.Save();
                    Settings.Default.Reload();
                }
            }
            catch
            {
                //Ignore errors here, if this fails then we couldn't upgrade user settings for whatever reason
            }
        }

        public static MruList RecentFiles
        {
            get
            {
                return _recentFiles;
            }
        }

    }
}
