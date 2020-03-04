using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;


namespace djack.RogueSurvivor
{
    static class Logger
    {
        public enum Stage
        {
            INIT_MAIN,
            RUN_MAIN,
            CLEAN_MAIN,

            INIT_GFX,
            RUN_GFX,
            CLEAN_GFX,

            INIT_SOUND,
            RUN_SOUND,
            CLEAN_SOUND
        };            

        static List<string> s_Lines = new List<string>();

        static readonly Object s_Mutex = new Object();  // alpha10 use lock() {} instead of Monitor

        public static IEnumerable<string> Lines
        {
            get { return s_Lines; }
        }

        public static void Clear()
        {
            lock (s_Mutex)
            { 

                // clear lines.
                s_Lines.Clear();
            }
        }

        public static void CreateFile()
        {
            lock (s_Mutex)
            { 
                // delete previous file.
                if (File.Exists(LogFilePath()))
                {
                    File.Delete(LogFilePath());
                }

                // create new one.
                Directory.CreateDirectory(SetupConfig.DirPath);
                using (StreamWriter emptyFile = File.CreateText(LogFilePath()))
                {
                    emptyFile.Close();
                }
            }
        }     
       

        public static void WriteLine(Stage stage, string text)
        {
            lock (s_Mutex)
            {
                // format.
                string s = String.Format("{0} {1} : {2}", s_Lines.Count, StageToString(stage), text);

                // add line.
                s_Lines.Add(s);

                // print to console.
                Console.Out.WriteLine(s);

                // write to log file.
                using (StreamWriter stream = File.AppendText(LogFilePath()))
                {
                    stream.WriteLine(s);
                    stream.Flush();
                    stream.Close();
                }
            }
        }

        static string LogFilePath()
        {
            return SetupConfig.DirPath + @"\log.txt";
        }

        static string StageToString(Stage s)
        {
            switch (s)
            {
                case Stage.CLEAN_GFX: return "clean gfx";
                case Stage.CLEAN_SOUND: return "clean sound";
                case Stage.CLEAN_MAIN: return "clean main";
                case Stage.INIT_GFX: return "init gfx";
                case Stage.INIT_SOUND: return "init sound";
                case Stage.INIT_MAIN: return "init main";
                case Stage.RUN_GFX: return "run gfx";
                case Stage.RUN_MAIN: return "run main";
                case Stage.RUN_SOUND: return "run sound";
                default: return "misc";
            }
        }
    }
}
