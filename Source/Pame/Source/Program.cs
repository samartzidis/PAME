//
// PAME Copyright (c) George Samartzidis <samartzidis@gmail.com>. All rights reserved.
// You are not allowed to redistribute, modify or sell any part of this file in either 
// compiled or non-compiled form without the author's written permission.
//

using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace Pame
{    
    public static class Program
    {
        const string guid = "F448548C-8E13-4abd-8530-0917CB444298";
        static SingleInstanceManager im = new SingleInstanceManager(guid);
        static SplashForm splash = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (!im.InitInstance(args))
                return;

            if (Properties.Settings.Default.Language == "Greek")         
                System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("el-GR");

            //Register file type
            Register(".pas", "Pame", "Pascal Source File", Globals.GetExePath(), Globals.GetExePath(), 1);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Properties.Settings.Default.ShowSplashScreen)
            {
                splash = new SplashForm();
                splash.Show();
            }

            MainForm mainForm = new MainForm(args);
            im.OnNextInstance += new SingleInstanceManager.NextInstanceHandler(mainForm.OnNextInstance);

            if (splash != null)
                splash.Close();
            
            Application.Run(mainForm);            
        } 
            
        /// <summary>
        /// Registers a file type via it's extension.
        /// </summary>
        /// <param name="extension">The extension to register</param>
        /// <param name="progId">A unique identifier for the program to work with the file type</param>
        /// <param name="description">A brief description of the file type</param>
        /// <param name="executeable">Where to find the executeable.</param>
        /// <param name="iconFile">Location of the icon.</param>
        /// <param name="iconIdx">Selects the icon within <paramref name="iconFile"/></param>
        public static void Register(string extension,
            string progId,
            string description,
            string executeable,
            string iconFile,
            int iconIdx)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + extension);
                key.SetValue(string.Empty, progId);

                RegistryKey key2 = Registry.CurrentUser.CreateSubKey(@"Software\Classes\" + progId);
                RegistryKey key3 = key2.CreateSubKey("DefaultIcon");
                key3.SetValue(string.Empty, String.Format("\"{0}\",{1}", iconFile, iconIdx));

                RegistryKey key4 = key2.CreateSubKey(@"shell\Open\command");
                key4.SetValue(string.Empty, String.Format("\"{0}\" \"%1\"", executeable));
            }
            catch (Exception)
            {
                Logging.Logger.Instance.Error("Failed to register file extension " + extension);
            }
        }
    }    
}
