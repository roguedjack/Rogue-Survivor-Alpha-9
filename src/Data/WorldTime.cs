using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    enum DayPhase
    {
        // 18h
        SUNSET,
        // 19h-20h-21h-22h-23h
        EVENING,
        // 24h
        MIDNIGHT,
        // 1h - 2h- 3h- 4h- 5h
        DEEP_NIGHT,

        // 6h
        SUNRISE,
        // 7h - 8h- 9h-10h-11h
        MORNING,
        // 12h
        MIDDAY,
        // 13h-14h-15h-16h-17h
        AFTERNOON       
    }

    [Serializable]
    class WorldTime
    {
        #region Constants
        public const int TURNS_PER_HOUR = 30;
        public const int TURNS_PER_DAY = TURNS_PER_HOUR * 24;
        #endregion

        #region Fields
        int m_TurnCounter;

        int m_Day;
        int m_Hour;
        DayPhase m_Phase;
        bool m_IsNight;

        bool m_IsStrikeOfMidnight;
        bool m_IsStrikeOfMidday;
        #endregion

        #region Properties
        public int TurnCounter
        {
            get { return m_TurnCounter; }
            set
            {
                DayPhase prevPhase = m_Phase;
                m_TurnCounter = value;
                RecomputeDate();
                DayPhase newPhase = m_Phase;

                m_IsStrikeOfMidnight = (newPhase == DayPhase.MIDNIGHT) && (prevPhase != DayPhase.MIDNIGHT);
                m_IsStrikeOfMidday = (newPhase == DayPhase.MIDDAY) && (prevPhase != DayPhase.MIDDAY);
            }
        }

        public int Day
        {
            get { return m_Day; }
        }

        public int Hour
        {
            get { return m_Hour; }
        }

        public bool IsNight
        {
            get { return m_IsNight; }
        }

        public DayPhase Phase
        {
            get { return m_Phase; }
        }

        /// <summary>
        /// Checks if it is just midnight.
        /// </summary>
        public bool IsStrikeOfMidnight
        {
            get { return m_IsStrikeOfMidnight; }
        }

        /// <summary>
        /// Checks if it is just midday.
        /// </summary>
        public bool IsStrikeOfMidday
        {
            get { return m_IsStrikeOfMidday; }
        }
        #endregion

        #region Init
        public WorldTime()
            : this(0)
        {
        }
        public WorldTime(WorldTime src)
            : this(src.TurnCounter)
        {
        }

        public WorldTime(int turnCounter)
        {
            if (turnCounter < 0)
                throw new ArgumentOutOfRangeException("turnCounter < 0");

            m_TurnCounter = turnCounter;
            RecomputeDate();
        }
        #endregion

        #region Date
        void RecomputeDate()
        {
            int counter = m_TurnCounter;

            m_Day = counter / TURNS_PER_DAY;
            counter -= m_Day * TURNS_PER_DAY;

            m_Hour = counter / TURNS_PER_HOUR;
            counter -= m_Hour * TURNS_PER_HOUR;

            switch (m_Hour)
            {
                case 0: m_Phase = DayPhase.MIDNIGHT; m_IsNight = true; break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5: m_Phase = DayPhase.DEEP_NIGHT; m_IsNight = true; break;
                case 6: m_Phase = DayPhase.SUNRISE; m_IsNight = false; break;
                case 7:
                case 8:
                case 9:
                case 10:
                case 11: m_Phase = DayPhase.MORNING; m_IsNight = false; break;
                case 12: m_Phase = DayPhase.MIDDAY; m_IsNight = false; break;
                case 13:
                case 14:
                case 15:
                case 16:
                case 17: m_Phase = DayPhase.AFTERNOON; m_IsNight = false; break;
                case 18: m_Phase = DayPhase.SUNSET; m_IsNight = true; break;
                case 19:
                case 20:
                case 21:
                case 22:
                case 23: m_Phase = DayPhase.EVENING; m_IsNight = true; break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled hour");

            }
        }
        #endregion

        #region Formating
        public override string ToString()
        {
            return String.Format("day {0} hour {1:D2}", this.Day, this.Hour);
        }

        public static string MakeTimeDurationMessage(int turns)
        {
            // less than a hour?
            if (turns < WorldTime.TURNS_PER_HOUR)
            {
                return "less than a hour";
            }

            // less than a day?
            if (turns < WorldTime.TURNS_PER_DAY)
            {
                int hours = turns / WorldTime.TURNS_PER_HOUR;
                if (hours == 1)
                    return "about 1 hour";
                else
                    return string.Format("about {0} hours", hours);
            }

            // day(s).
            WorldTime time = new WorldTime(turns);
            if (time.Day == 1)
                return "about 1 day";
            else
                return string.Format("about {0} days", time.Day);
        }

        #endregion
    }
}
