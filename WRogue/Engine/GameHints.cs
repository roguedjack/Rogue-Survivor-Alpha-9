using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    class GameHintsStatus
    {
        #region Fields
        bool[] m_AdvisorHints = new bool[(int)AdvisorHint._COUNT];
        #endregion

        #region Status
        public void ResetAllHints()
        {
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
                m_AdvisorHints[i] = false;
        }

        public bool IsAdvisorHintGiven(AdvisorHint hint)
        {
            return m_AdvisorHints[(int)hint];
        }

        public void SetAdvisorHintAsGiven(AdvisorHint hint)
        {
            m_AdvisorHints[(int)hint] = true;
        }

        public int CountAdvisorHintsGiven()
        {
            int count = 0;
            for (int i = (int)AdvisorHint._FIRST; i < (int)AdvisorHint._COUNT; i++)
                if (m_AdvisorHints[i])
                    ++count;

            return count;
        }

        public bool HasAdvisorGivenAllHints()
        {
            return CountAdvisorHintsGiven() >= (int)AdvisorHint._COUNT;
        }
        #endregion

        #region Saving & Loading
        public static void Save(GameHintsStatus hints, string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hints...");

            IFormatter formatter = CreateFormatter();
            Stream stream = CreateStream(filepath, true);

            formatter.Serialize(stream, hints);
            stream.Flush();
            stream.Close();

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hints... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        public static GameHintsStatus Load(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hints...");

            GameHintsStatus hints;
            try
            {
                IFormatter formatter = CreateFormatter();
                Stream stream = CreateStream(filepath, false);

                hints = (GameHintsStatus)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load hints (first run?).");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "resetting.");
                hints = new GameHintsStatus();
                hints.ResetAllHints();
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading options... done!");
            return hints;
        }

        static IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        static Stream CreateStream(string saveFileName, bool save)
        {
            return new FileStream(saveFileName,
                save ? FileMode.Create : FileMode.Open,
                save ? FileAccess.Write : FileAccess.Read,
                FileShare.None);
        }
        #endregion
    }
}
