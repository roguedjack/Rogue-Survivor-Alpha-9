using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    class HiScore
    {
        public string Name { get; set; }
        public int TotalPoints { get; set; }
        public int DifficultyPercent { get; set; }
        public int SurvivalPoints { get; set; }
        public int KillPoints { get; set; }
        public int AchievementPoints { get; set; }
        public int TurnSurvived { get; set; }
        public TimeSpan PlayingTime { get; set; }
        public string SkillsDescription { get; set; }
        public string Death { get; set; }

        public static HiScore FromScoring(string name, Scoring sc, string skillsDescription)
        {
            if (sc == null)
                throw new ArgumentNullException("scoring");

            HiScore hi = new HiScore()
            {
                AchievementPoints = sc.AchievementPoints,
                Death = sc.DeathReason,
                DifficultyPercent = (int)(100 * sc.DifficultyRating),
                KillPoints = sc.KillPoints,
                Name = name,
                PlayingTime = sc.RealLifePlayingTime,
                SkillsDescription = skillsDescription,
                SurvivalPoints = sc.SurvivalPoints,
                TotalPoints = sc.TotalPoints,
                TurnSurvived = sc.TurnsSurvived
            };

            return hi;
        }
    }

    [Serializable]
    class HiScoreTable
    {
        #region Constants
        public const int DEFAULT_MAX_ENTRIES = 12;
        #endregion

        #region Fields
        List<HiScore> m_Table;
        int m_MaxEntries;
        #endregion

        #region Properties
        public int Count
        {
            get { return m_Table.Count; }
        }

        public HiScore this[int index]
        {
            get { return Get(index); }
        }
        #endregion

        #region Init
        public HiScoreTable(int maxEntries)
        {
            if (maxEntries < 1)
                throw new ArgumentOutOfRangeException("maxEntries < 1");

            m_Table = new List<HiScore>(maxEntries);
            m_MaxEntries = maxEntries;
        }

        public void Clear()
        {
            for (int i = 0; i < m_MaxEntries; i++)
                m_Table.Add(new HiScore()
                {
                    Death = "no death",
                    DifficultyPercent = 0,
                    KillPoints = 0,
                    Name = "no one",
                    PlayingTime = TimeSpan.Zero,
                    SurvivalPoints = 0,
                    TotalPoints = 0,
                    TurnSurvived = 0,
                    SkillsDescription = "no skills"
                });
        }
        #endregion

        #region Storing & Retrieving
        public bool Register(HiScore hi)
        {
            int i = 0;
            while (i < m_Table.Count && m_Table[i].TotalPoints >= hi.TotalPoints)
            {
                ++i;
            }

            if (i > m_MaxEntries)
                return false;

            m_Table.Insert(i, hi);
            while (m_Table.Count > m_MaxEntries)
            {
                m_Table.RemoveAt(m_Table.Count - 1);
            }
            return true;
        }

        public HiScore Get(int index)
        {
            if (index < 0 || index >= m_Table.Count)
                throw new ArgumentOutOfRangeException("index");
            return m_Table[index];
        }
        #endregion

        #region Saving & Loading
        public static void Save(HiScoreTable table, string filepath)
        {
            if (table == null)
                throw new ArgumentNullException("table");
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table...");

            IFormatter formatter = CreateFormatter();
            Stream stream = CreateStream(filepath, true);

            formatter.Serialize(stream, table);
            stream.Flush();
            stream.Close();

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving hiscore table... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        public static HiScoreTable Load(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hiscore table...");

            HiScoreTable table;
            try
            {
                IFormatter formatter = CreateFormatter();
                Stream stream = CreateStream(filepath, false);

                table = (HiScoreTable)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load hiscore table (no hiscores?).");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                return null;
            }


            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading hiscore table... done!");
            return table;
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
