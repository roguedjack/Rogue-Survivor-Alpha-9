using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;

using djack.RogueSurvivor;
using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Gameplay
{
    static class Skills
    {
        [Serializable]
        public enum IDs
        {
            _FIRST = 0,

            #region Living skills

            _FIRST_LIVING = _FIRST,

            /// <summary>
            /// Bonus to melee hit & defence.
            /// </summary>
            AGILE = _FIRST_LIVING,

            /// <summary>
            /// Bonus to max sleep.
            /// </summary>
            AWAKE,

            /// <summary>
            /// Bonus to bows attack.
            /// </summary>
            BOWS,

            /// <summary>
            /// Bonus to barricading points; can build fortifications, consume less material.
            /// </summary>
            CARPENTRY,

            /// <summary>
            /// Bonus to trust gain, trade and can steal followers.
            /// </summary>
            CHARISMATIC,

            /// <summary>
            /// Bonus to firearms attack.
            /// </summary>
            FIREARMS,

            /// <summary>
            /// Can sleep heal anywhere, increase sleep healing chance.
            /// </summary>
            HARDY,

            /// <summary>
            /// Bonus to inventory capacity.
            /// </summary>
            HAULER,

            /// <summary>
            /// Bonus to max stamina.
            /// </summary>
            HIGH_STAMINA,

            /// <summary>
            /// Bonus to max followers.
            /// </summary>
            LEADERSHIP,

            /// <summary>
            /// Bonus to max food.
            /// </summary>
            LIGHT_EATER,

            /// <summary>
            /// Avoid traps.
            /// </summary>
            LIGHT_FEET,

            /// <summary>
            /// Easier wake up.
            /// </summary>
            LIGHT_SLEEPER,

            /// <summary>
            /// Better unarmed fighting.
            /// </summary>
            MARTIAL_ARTS,

            /// <summary>
            /// Bonus to medecine effects.
            /// </summary>
            MEDIC,

            /// <summary>
            /// Dead things.
            /// </summary>
            NECROLOGY,

            /// <summary>
            /// Bonus to melee damage.
            /// </summary>
            STRONG,

            /// <summary>
            /// Sanity resistance.
            /// </summary>
            STRONG_PSYCHE,

            /// <summary>
            /// Bonus to max HPs
            /// </summary>
            TOUGH,

            /// <summary>
            /// Bonus to evade murders.
            /// </summary>
            UNSUSPICIOUS,

            _LAST_LIVING = UNSUSPICIOUS,

            #endregion

            #region Undead skills
            _FIRST_UNDEAD,
            Z_AGILE = _FIRST_UNDEAD,
            Z_EATER,
            Z_GRAB,
            Z_INFECTOR,
            Z_LIGHT_EATER,
            Z_LIGHT_FEET,
            Z_STRONG,
            Z_TOUGH,
            Z_TRACKER,
            _LAST_UNDEAD = Z_TRACKER,
            #endregion

            _COUNT
        }

        static string[] s_Names = new string[(int)IDs._COUNT];

        public static IDs[] UNDEAD_SKILLS = new IDs[]
        {
            IDs.Z_AGILE,
            IDs.Z_EATER,
            IDs.Z_GRAB,
            IDs.Z_INFECTOR,
            IDs.Z_LIGHT_EATER,
            IDs.Z_LIGHT_FEET,
            IDs.Z_STRONG,
            IDs.Z_TOUGH,
            IDs.Z_TRACKER
        };

        public static string Name(IDs id)
        {
            return s_Names[(int)id];
        }

        public static string Name(int id)
        {
            return Name((IDs)id);
        }

        public static int MaxSkillLevel(IDs id)
        {
            switch (id)
            {
                case IDs.HAULER:
                    return 3;

                default:
                    return 5;
            }
        }

        public static int MaxSkillLevel(int id)
        {
            return MaxSkillLevel((IDs)id);
        }

        public static IDs RollLiving(DiceRoller roller)
        {
            return (IDs)roller.Roll((int)IDs._FIRST_LIVING, (int)IDs._LAST_LIVING + 1);
        }

        public static IDs RollUndead(DiceRoller roller)
        {
            return (IDs)roller.Roll((int)IDs._FIRST_UNDEAD, (int)IDs._LAST_UNDEAD + 1);
        }

        #region Data
        struct SkillData
        {
            public const int COUNT_FIELDS = 6;

            public string NAME { get; set; }
            public float VALUE1 { get; set; }
            public float VALUE2 { get; set; }
            public float VALUE3 { get; set; }
            public float VALUE4 { get; set; }

            public static SkillData FromCSVLine(CSVLine line)
            {
                return new SkillData()
                {
                    NAME = line[1].ParseText(),
                    VALUE1 = line[2].ParseFloat(),
                    VALUE2 = line[3].ParseFloat(),
                    VALUE3 = line[4].ParseFloat(),
                    VALUE4 = line[5].ParseFloat()
                };
            }
        }

        #region Helpers
        static void Notify(IRogueUI ui, string what, string stage)
        {
            ui.UI_Clear(Color.Black);
            ui.UI_DrawStringBold(Color.White, "Loading " + what + " data : " + stage, 0, 0);
            ui.UI_Repaint();
        }

        static CSVLine FindLineForModel(CSVTable table, IDs skillID)
        {
            foreach (CSVLine l in table.Lines)
            {
                if (l[0].ParseText() == skillID.ToString())
                    return l;
            }

            return null;
        }

        static _DATA_TYPE_ GetDataFromCSVTable<_DATA_TYPE_>(IRogueUI ui, CSVTable table, Func<CSVLine, _DATA_TYPE_> fn, IDs skillID)
        {
            // get line for id in table.
            CSVLine line = FindLineForModel(table, skillID);
            if (line == null)
                throw new InvalidOperationException(String.Format("skill {0} not found", skillID.ToString()));

            // get data from line.
            _DATA_TYPE_ data;
            try
            {
                data = fn(line);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(String.Format("invalid data format for skill {0}; exception : {1}", skillID.ToString(), e.ToString()));
            }

            // ok.
            return data;
        }

        static bool LoadDataFromCSV<_DATA_TYPE_>(IRogueUI ui, string path, string kind, int fieldsCount, Func<CSVLine, _DATA_TYPE_> fn, IDs[] idsToRead, out _DATA_TYPE_[] data)
        {
            //////////////////////////
            // Read & parse csv file.
            //////////////////////////
            Notify(ui, kind, "loading file...");
            // read the whole file.
            List<string> allLines = new List<string>();
            bool ignoreHeader = true;
            using (StreamReader reader = File.OpenText(path))
            {
                while (!reader.EndOfStream)
                {
                    string inLine = reader.ReadLine();
                    if (ignoreHeader)
                    {
                        ignoreHeader = false;
                        continue;
                    }
                    allLines.Add(inLine);
                }
                reader.Close();
            }
            // parse all the lines read.
            Notify(ui, kind, "parsing CSV...");
            CSVParser parser = new CSVParser();
            CSVTable table = parser.ParseToTable(allLines.ToArray(), fieldsCount);

            /////////////
            // Set data.
            /////////////
            Notify(ui, kind, "reading data...");

            data = new _DATA_TYPE_[idsToRead.Length];
            for (int i = 0; i < idsToRead.Length; i++)
            {
                data[i] = GetDataFromCSVTable<_DATA_TYPE_>(ui, table, fn, idsToRead[i]);
            }

            //////////////
            // all fine.
            /////////////
            Notify(ui, kind, "done!");
            return true;
        }
        #endregion

        #region Loading
        public static bool LoadSkillsFromCSV(IRogueUI ui, string path)
        {
            SkillData[] data;

            LoadDataFromCSV<SkillData>(ui, path, "skills", SkillData.COUNT_FIELDS, SkillData.FromCSVLine,
                new IDs[] { IDs.AGILE, IDs.AWAKE, IDs.BOWS, IDs.CARPENTRY, IDs.CHARISMATIC, IDs.FIREARMS, IDs.HARDY, IDs.HAULER,
                            IDs.HIGH_STAMINA, IDs.LEADERSHIP, IDs.LIGHT_EATER, IDs.LIGHT_FEET, IDs.LIGHT_SLEEPER, IDs.MARTIAL_ARTS, IDs.MEDIC, 
                            IDs.NECROLOGY, IDs.STRONG, IDs.STRONG_PSYCHE, IDs.TOUGH, IDs.UNSUSPICIOUS,
                            IDs.Z_AGILE, IDs.Z_EATER, IDs.Z_GRAB, IDs.Z_INFECTOR, IDs.Z_LIGHT_EATER, IDs.Z_LIGHT_FEET, IDs.Z_STRONG, IDs.Z_TOUGH, IDs.Z_TRACKER },
                out data);

            // names.
            for (int i = (int)IDs._FIRST; i < (int)IDs._COUNT; i++)
                s_Names[i] = data[i].NAME;


            // then skills value.
            SkillData s;
            
            s = data[(int)IDs.AGILE];
            Rules.SKILL_AGILE_ATK_BONUS = (int)s.VALUE1;
            Rules.SKILL_AGILE_DEF_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.AWAKE];
            Rules.SKILL_AWAKE_SLEEP_BONUS = s.VALUE1;
            Rules.SKILL_AWAKE_SLEEP_REGEN_BONUS = s.VALUE2;

            s = data[(int)IDs.BOWS];
            Rules.SKILL_BOWS_ATK_BONUS = (int)s.VALUE1;
            Rules.SKILL_BOWS_DMG_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.CARPENTRY];
            Rules.SKILL_CARPENTRY_BARRICADING_BONUS = s.VALUE1;
            Rules.SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.CHARISMATIC];
            Rules.SKILL_CHARISMATIC_TRUST_BONUS = (int)s.VALUE1;
            Rules.SKILL_CHARISMATIC_TRADE_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.FIREARMS];
            Rules.SKILL_FIREARMS_ATK_BONUS = (int)s.VALUE1;
            Rules.SKILL_FIREARMS_DMG_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.HARDY];
            Rules.SKILL_HARDY_HEAL_CHANCE_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.HAULER];
            Rules.SKILL_HAULER_INV_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.HIGH_STAMINA];
            Rules.SKILL_HIGH_STAMINA_STA_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.LEADERSHIP];
            Rules.SKILL_LEADERSHIP_FOLLOWER_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.LIGHT_EATER];
            Rules.SKILL_LIGHT_EATER_FOOD_BONUS = s.VALUE1;
            Rules.SKILL_LIGHT_EATER_MAXFOOD_BONUS = s.VALUE2;

            s = data[(int)IDs.LIGHT_FEET];
            Rules.SKILL_LIGHT_FEET_TRAP_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.LIGHT_SLEEPER];
            Rules.SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.MARTIAL_ARTS];
            Rules.SKILL_MARTIAL_ARTS_ATK_BONUS = (int)s.VALUE1;
            Rules.SKILL_MARTIAL_ARTS_DMG_BONUS = (int)s.VALUE2;
            Rules.SKILL_MARTIAL_ARTS_DISARM_BONUS = (int)s.VALUE3;

            s = data[(int)IDs.MEDIC];
            Rules.SKILL_MEDIC_BONUS = s.VALUE1;
            Rules.SKILL_MEDIC_REVIVE_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.NECROLOGY];
            Rules.SKILL_NECROLOGY_UNDEAD_BONUS = (int)s.VALUE1;
            Rules.SKILL_NECROLOGY_CORPSE_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.STRONG];
            Rules.SKILL_STRONG_DMG_BONUS = (int)s.VALUE1;
            Rules.SKILL_STRONG_THROW_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.STRONG_PSYCHE];
            Rules.SKILL_STRONG_PSYCHE_LEVEL_BONUS = s.VALUE1;

            s = data[(int)IDs.TOUGH];
            Rules.SKILL_TOUGH_HP_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.UNSUSPICIOUS];
            Rules.SKILL_UNSUSPICIOUS_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.Z_AGILE];
            Rules.SKILL_ZAGILE_ATK_BONUS = (int)s.VALUE1;
            Rules.SKILL_ZAGILE_DEF_BONUS = (int)s.VALUE2;

            s = data[(int)IDs.Z_EATER];
            Rules.SKILL_ZEATER_REGEN_BONUS = s.VALUE1;

            s = data[(int)IDs.Z_INFECTOR];
            Rules.SKILL_ZINFECTOR_BONUS = s.VALUE1;

            s = data[(int)IDs.Z_GRAB];
            Rules.SKILL_ZGRAB_CHANCE = (int)s.VALUE1;

            s = data[(int)IDs.Z_LIGHT_EATER];
            Rules.SKILL_ZLIGHT_EATER_FOOD_BONUS = s.VALUE1;
            Rules.SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = s.VALUE2;

            s = data[(int)IDs.Z_LIGHT_FEET];
            Rules.SKILL_ZLIGHT_FEET_TRAP_BONUS = (int)s.VALUE1;
            
            s = data[(int)IDs.Z_STRONG];
            Rules.SKILL_ZSTRONG_DMG_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.Z_TOUGH];
            Rules.SKILL_ZTOUGH_HP_BONUS = (int)s.VALUE1;

            s = data[(int)IDs.Z_TRACKER];
            Rules.SKILL_ZTRACKER_SMELL_BONUS = s.VALUE1;

            return true;
        }
        #endregion

        #endregion
    }
}
