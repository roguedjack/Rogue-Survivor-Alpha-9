using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logger.CreateFile();
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "starting program...");
            Logger.WriteLine(Logger.Stage.INIT_MAIN, String.Format("date : {0}.", DateTime.Now.ToString()));
            Logger.WriteLine(Logger.Stage.INIT_MAIN, String.Format("game version : {0}.", SetupConfig.GAME_VERSION));

            Application.CurrentCulture = CultureInfo.InvariantCulture;  // avoids nasty "," vs "." format confusion.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Logger.WriteLine(Logger.Stage.INIT_MAIN, "loading setup...");
            SetupConfig.Load();
            Logger.WriteLine(Logger.Stage.INIT_MAIN, "setup : " + SetupConfig.toString(SetupConfig.Video) + ", " + SetupConfig.toString(SetupConfig.Sound));

            using (RogueForm form = new RogueForm())
            {
                // Debug mode : don't catch exceptions, I want to debug them.
                // Release mode : catch exceptions cleanly and report.
#if DEBUG
                Application.Run(form);
#else
                try
                {
                    Application.Run(form);
                }
                catch (Exception e)
                {
                    using (Bugreport report = new Bugreport(e))
                    {
                        report.ShowDialog();
                    }
                    Application.Exit();
                }
#endif
            }
            Logger.WriteLine(Logger.Stage.CLEAN_MAIN, "exiting program...");
        }
    }
}
