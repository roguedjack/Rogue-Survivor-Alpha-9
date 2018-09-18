using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    class Achievement
    {
        #region IDs
        [Serializable]
        public enum IDs
        {
            _FIRST = 0,

            REACHED_DAY_07 = _FIRST,
            REACHED_DAY_14,
            REACHED_DAY_21,
            REACHED_DAY_28,

            CHAR_BROKE_INTO_OFFICE,
            CHAR_FOUND_UNDERGROUND_FACILITY,
            CHAR_POWER_UNDERGROUND_FACILITY,

            KILLED_THE_SEWERS_THING,

            _COUNT,
        }
        #endregion

        #region Properties
        public IDs ID { get; private set; }
        public string Name { get; private set; }
        public string TeaseName { get; private set; }
        public string[] Text { get; private set; }
        public string MusicID { get; private set; }
        public int ScoreValue { get; private set; }
        public bool IsDone { get; set; }
        #endregion

        #region Init
        public Achievement(IDs id, string name, string teaseName, string[] text, string musicID, int scoreValue)
        {
            this.ID = id;
            this.Name = name;
            this.TeaseName = teaseName;
            this.Text = text;
            this.MusicID = musicID;
            this.ScoreValue = scoreValue;
            this.IsDone = false;
        }
        #endregion

    }

    [Serializable]
    enum DifficultySide
    {
        FOR_SURVIVOR,
        FOR_UNDEAD
    }

    [Serializable]
    class Scoring
    {
        #region Types
        [Serializable]
        public class KillData
        {
            public int ActorModelID { get; set; }
            public int Amount { get; set; }
            public int FirstKillTurn { get; set; }

            public KillData(int actorModelID, int turn)
            {
                this.ActorModelID = actorModelID;
                this.Amount = 1;
                this.FirstKillTurn = turn;
            }
        }

        [Serializable]
        public class GameEventData
        {
            public int Turn { get; set; }
            public string Text { get; set; }

            public GameEventData(int turn, string text)
            {
                this.Turn = turn;
                this.Text = text;
            }
        }
        #endregion

        #region Constants
        public const int MAX_ACHIEVEMENTS = (int) Achievement.IDs._COUNT;

        public const int SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD = 12 * WorldTime.TURNS_PER_HOUR; 
        #endregion

        #region Fields
        int m_StartScoringTurn;
        int m_ReincarnationNumber;
        Dictionary<int, KillData> m_Kills = new Dictionary<int, KillData>();
        HashSet<int> m_Sightings = new HashSet<int>();
        List<GameEventData> m_Events = new List<GameEventData>();
        HashSet<Map> m_VisitedMaps = new HashSet<Map>();
        List<Actor> m_FollowersWhenDied = null;
        Actor m_Killer = null;
        Actor m_ZombifiedPlayer = null;
        int m_KillPoints;
        float m_DifficultyRating = 1;
        DifficultySide m_Side;
        #endregion

        #region Properties
        public DifficultySide Side
        {
            get { return m_Side; }
            set { m_Side = value; }
        }

        public int StartScoringTurn
        {
            get { return m_StartScoringTurn; }
            set { m_StartScoringTurn = value; }
        }

        /// <summary>
        /// Current reincarnation, 0 is the first life.
        /// </summary>
        public int ReincarnationNumber
        {
            get { return m_ReincarnationNumber; }
            set { m_ReincarnationNumber = value; }
        }

        public Achievement[] Achievements
        {
            get;
            private set;
        }

        /// <summary>
        /// obsolete
        /// </summary>
        public Skills.IDs StartingSkill
        {
            get;
            set;
        }

        public IEnumerable<GameEventData> Events
        {
            get { return m_Events; }
        }

        public bool HasNoEvents
        {
            get { return m_Events.Count == 0; }
        }

        public IEnumerable<KillData> Kills
        {
            get { return m_Kills.Values; }
        }

        public bool HasNoKills
        {
            get { return m_Kills.Count == 0; }
        }

        public IEnumerable<int> Sightings
        {
            get { return m_Sightings; }
        }

        public int TurnsSurvived
        {
            get;
            set;
        }

        public string DeathReason
        {
            get;
            set;
        }

        public string DeathPlace
        {
            get;
            set;
        }

        public List<Actor> FollowersWhendDied
        {
            get { return m_FollowersWhenDied; }
        }

        public Actor Killer
        {
            get { return m_Killer; }
        }

        public Actor ZombifiedPlayer
        {
            get { return m_ZombifiedPlayer; }
        }

        public int KillPoints
        {
            get { return m_KillPoints; }
        }

        public int SurvivalPoints
        {
            get { return 2 * (this.TurnsSurvived - m_StartScoringTurn); }
        }

        public int AchievementPoints
        {
            get
            {
                int bonus = 0;

                for (int i = (int)Achievement.IDs._FIRST; i < (int)Achievement.IDs._COUNT; i++)
                {
                    if (HasCompletedAchievement((Achievement.IDs)i))
                        bonus += GetAchievement((Achievement.IDs)i).ScoreValue;
                }

                return bonus;
            }
        }

        /// <summary>
        /// Difficulty as a float (0..1+), divided by current reincarnation number.
        /// </summary>
        public float DifficultyRating
        {
            get { return m_DifficultyRating / (1 + m_ReincarnationNumber); }
            set { m_DifficultyRating = value; }
        }

        /// <summary>
        /// (Difficulty * (SurvivalPoints + KillPoints + Achievement))
        /// </summary>
        public int TotalPoints
        {
            get { return (int)(DifficultyRating * (m_KillPoints + SurvivalPoints + AchievementPoints)); }
        }

        /// <summary>
        /// Reallife playing time in seconds.
        /// </summary>
        public TimeSpan RealLifePlayingTime
        {
            get;
            set;
        }

        #region Achievements
        public int CompletedAchievementsCount
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region Init
        public Scoring()
        {
            this.RealLifePlayingTime = new TimeSpan(0);

            ////////////////
            // Achievements
            ////////////////
            #region
            this.Achievements = new Achievement[(int)Achievement.IDs._COUNT];

            #region CHAR related
            InitAchievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE, 
                new Achievement(Achievement.IDs.CHAR_BROKE_INTO_OFFICE,
                    "Broke into a CHAR Office",
                    "Did not broke into XXX",
                    new string[] { "Now try not to die too soon..." },
                    GameMusics.HEYTHERE,
                    1000));

            InitAchievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY,
                new Achievement(Achievement.IDs.CHAR_FOUND_UNDERGROUND_FACILITY,
                    "Found the CHAR Underground Facility",
                    "Did not found XXX",
                    new string[] { "Now, where is the light switch?..." },
                    GameMusics.CHAR_UNDERGROUND_FACILITY,
                    2000));

            InitAchievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY,
                new Achievement(Achievement.IDs.CHAR_POWER_UNDERGROUND_FACILITY,
                    "Powered the CHAR Underground Facility",
                    "Did not XXX the XXX",
                    new string[] { "Personal message from the game developper : ",
                                   "Sorry, the rest of the plot is missing.",
                                   "For now its a dead end.",
                                   "Enjoy the rest of the game.",
                                   "See you in a next game version :)"},
                   GameMusics.CHAR_UNDERGROUND_FACILITY,
                   3000));
            #endregion

            #region Killing uniques
            InitAchievement(Achievement.IDs.KILLED_THE_SEWERS_THING,
                new Achievement(Achievement.IDs.KILLED_THE_SEWERS_THING,
                    "Killed The Sewers Thing",
                    "Did not kill the XXX",
                    new string[] { "One less Thing to worry about!" },
                    GameMusics.HEYTHERE,
                    1000));
            #endregion

            #region Reaching Day X
            InitAchievement(Achievement.IDs.REACHED_DAY_07,
                new Achievement(Achievement.IDs.REACHED_DAY_07,
                    "Reached Day 7",
                    "Did not reach XXX",
                    new string[] { "Keep staying alive!" },
                    GameMusics.HEYTHERE,
                    1000));

            InitAchievement(Achievement.IDs.REACHED_DAY_14,
                new Achievement(Achievement.IDs.REACHED_DAY_14,
                    "Reached Day 14",
                    "Did not reach XXX",
                    new string[] { "Keep staying alive!" },
                    GameMusics.HEYTHERE,
                    1000));

            InitAchievement(Achievement.IDs.REACHED_DAY_21,
                new Achievement(Achievement.IDs.REACHED_DAY_21,
                    "Reached Day 21",
                    "Did not reach XXX",
                    new string[] { "Keep staying alive!" },
                    GameMusics.HEYTHERE,
                    1000));

            InitAchievement(Achievement.IDs.REACHED_DAY_28,
                new Achievement(Achievement.IDs.REACHED_DAY_28,
                    "Reached Day 28",
                    "Did not reach XXX",
                    new string[] { "Is this the end?" },
                    GameMusics.HEYTHERE,
                    1000));
            #endregion
            #endregion
        }

        /// <summary>
        /// Setup scoring for a new life (reincarnation).
        /// </summary>
        /// <param name="gameTurn"></param>
        public void StartNewLife(int gameTurn)
        {
            // new life.
            ++m_ReincarnationNumber;

            // reset achievements.
            foreach (Achievement a in this.Achievements)
                a.IsDone = false;
            this.CompletedAchievementsCount = 0;

            // reset visited maps.
            m_VisitedMaps.Clear();

            // clear events.
            m_Events.Clear();

            // clear sightings.
            m_Sightings.Clear();

            // clear kills.
            m_Kills.Clear();

            // reset killer, followers & zombified form.
            m_Killer = null;
            m_FollowersWhenDied = null;
            m_ZombifiedPlayer = null;

            // reset points.
            m_KillPoints = 0;

            // start scoring at this turn.
            m_StartScoringTurn = gameTurn;
        }
        #endregion

        #region Achievements
        public bool HasCompletedAchievement(Achievement.IDs id)
        {
            return this.Achievements[(int)id].IsDone;
        }

        public void SetCompletedAchievement(Achievement.IDs id)
        {
            this.Achievements[(int)id].IsDone = true;
        }

        public Achievement GetAchievement(Achievement.IDs id)
        {
            return this.Achievements[(int)id];
        }

        void InitAchievement(Achievement.IDs id, Achievement a)
        {
            this.Achievements[(int)id] = a;
        }
        #endregion

        #region Computing difficulty rating
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <param name="side">side the difficulty is computed for.</param>
        /// <param name="reincarnationNumber">0 for the first life</param>
        /// <returns>[0..1+]</returns>
        public static float ComputeDifficultyRating(GameOptions options, DifficultySide side, int reincarnationNumber)
        {
            float rating = 1.0f;

            ///////////////////
            // Constant factors.
            // Harder:
            // - Don't reveal starting map : +10%
            // Survivor Easier/Undead Harder:
            // - Disable NPC starvation    : -10%/+10%
            // Harder/Easier:
            // - Nat Guards                : -50% -> +50%
            // - Supplies                  : -50% -> +50%
            // - Zombifieds UpDay 
            //////////////////
            #region
            // - Don't reveal starting map: +10%
            if (!options.RevealStartingDistrict)
                rating += 0.10f;

            // - Disable NPC starvation: -10%
            if (!options.NPCCanStarveToDeath)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating -= 0.10f;
                else
                    rating += 0.10f;
            }

            // - Nat Guards                : -50% -> +50%
            if (options.NatGuardFactor != GameOptions.DEFAULT_NATGUARD_FACTOR)
            {
                float k = (options.NatGuardFactor - GameOptions.DEFAULT_NATGUARD_FACTOR) / (float)GameOptions.DEFAULT_NATGUARD_FACTOR;
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating -= 0.50f * k;
                else
                    rating += 0.50f * k;
            }

            // - Supplies                  : -50% -> +50%
            if (options.SuppliesDropFactor != GameOptions.DEFAULT_SUPPLIESDROP_FACTOR)
            {
                float k = (options.SuppliesDropFactor - GameOptions.DEFAULT_SUPPLIESDROP_FACTOR) / (float)GameOptions.DEFAULT_SUPPLIESDROP_FACTOR;
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating -= 0.50f * k;
                else
                    rating += 0.50f * k;
            }

            // - Zombifieds UpDay
            if (options.ZombifiedsUpgradeDays != GameOptions.DEFAULT_ZOMBIFIEDS_UPGRADE_DAYS)
            {
                float k = 0;
                switch (options.ZombifiedsUpgradeDays)
                {
                    case GameOptions.ZupDays.OFF: k = -0.50f; break;
                    case GameOptions.ZupDays.ONE: k = 0.50f; break;
                    case GameOptions.ZupDays.TWO: k = 0.25f; break;
                    case GameOptions.ZupDays.THREE: break;
                    case GameOptions.ZupDays.FOUR: k -= 0.10f; break;
                    case GameOptions.ZupDays.FIVE: k -= 0.20f; break;
                    case GameOptions.ZupDays.SIX: k -= 0.30f; break;
                    case GameOptions.ZupDays.SEVEN: k -= 0.40f; break;
                    default: break;
                }
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating += k;
                else
                    rating -= k;
            }
            #endregion

            //////////////////
            // Dynamic factors:
            // !reversed for undeads!
            // - Density            : f(mapsize, civs+undeads), +/- 99%   // alpha10.1 removed citysize from difficulty formula
            // - Undeads            : f(undeads/civs, day0, invasion%), +/- 50%
            // - Civilians          : f(zombification%, canstarve&starvedzomb%), +/- 50%
            ////////////////////
            #region
            // - Density            : f(mapsize, civs+undeads), +/- 99%   // alpha10.1 removed citysize from difficulty formula
            float kDefaultDensity = (float)(Math.Sqrt(GameOptions.DEFAULT_MAX_CIVILIANS + GameOptions.DEFAULT_MAX_UNDEADS)) / (float)(GameOptions.DEFAULT_DISTRICT_SIZE * GameOptions.DEFAULT_DISTRICT_SIZE);
            float kDensity = (float)(Math.Sqrt(options.MaxCivilians + options.MaxUndeads)) / (float)(options.DistrictSize * options.DistrictSize);
            float rDensity = (kDensity - kDefaultDensity) / kDefaultDensity;
            if (side == DifficultySide.FOR_SURVIVOR)
                rating += 0.99f * rDensity;
            else
                rating -= 0.99f * rDensity;

            // - Undeads            : f(undeads/civs, day0, invasion%), +/- 50%
            float kDefaultUndeadsRatio = (float)GameOptions.DEFAULT_MAX_UNDEADS / (float)GameOptions.DEFAULT_MAX_CIVILIANS;
            float kUndeadsRatio = (float)options.MaxUndeads / (float)options.MaxCivilians;
            float kUndeads_Nb = (kUndeadsRatio - kDefaultUndeadsRatio) / kDefaultUndeadsRatio;
            float kUndeads_Day0 = (float)(options.DayZeroUndeadsPercent - GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT) / (float)GameOptions.DEFAULT_DAY_ZERO_UNDEADS_PERCENT;
            float kUndeads_Inv = (float)(options.ZombieInvasionDailyIncrease - GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE) / (float)GameOptions.DEFAULT_ZOMBIE_INVASION_DAILY_INCREASE;
            if (side == DifficultySide.FOR_SURVIVOR)
                rating += 0.30f * kUndeads_Nb + 0.05f * kUndeads_Day0 + 0.15f * kUndeads_Inv;
            else
                rating -= 0.30f * kUndeads_Nb + 0.05f * kUndeads_Day0 + 0.15f * kUndeads_Inv;

            // - Civilians          : f(zombification%, canstarve&starvedzomb%), +/- 50%
            float kDefaultCivZombification = (float)GameOptions.DEFAULT_MAX_CIVILIANS * (float)GameOptions.DEFAULT_ZOMBIFICATION_CHANCE;
            float kCivZombification = (float)(options.MaxCivilians * options.ZombificationChance - kDefaultCivZombification) / kDefaultCivZombification;

            float kDefaultCivStarvation = (float)GameOptions.DEFAULT_MAX_CIVILIANS * (float)GameOptions.DEFAULT_STARVED_ZOMBIFICATION_CHANCE;
            float kCivStarvation = (float)(options.MaxCivilians * options.StarvedZombificationChance - kDefaultCivStarvation) / kDefaultCivStarvation;
            if (!options.NPCCanStarveToDeath)
                kCivStarvation = -1;
            if (side == DifficultySide.FOR_SURVIVOR)
                rating += 0.30f * kCivZombification + 0.20f * kCivStarvation;
            else
                rating -= 0.30f * kCivZombification + 0.20f * kCivStarvation;
            #endregion


            /////////////
            // Scaling factors.
            // - Disable undeads evolution  : x0.5 / x2
            // - Enable Combat Assistant    : x0.75
            // - Enable permadeath          : x2
            // - Aggressive Hungry Civs     : x0.5 / x2
            // - Rats Upgrade               : x1.10 / x0.90
            // - Skeletons Upgrade          : x1.20 / x0.80
            // - Shamblers Upgrade          : x1.25 / x0.75
            ////////////
            #region
            // - Disable undeads evolution: x0.5 / x2
            if (!options.AllowUndeadsEvolution)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating *= 0.5f;
                else
                    rating *= 2f;
            }

            // - Enable Combat Assistant   : x0.75
            if (options.IsCombatAssistantOn)
                rating *= 0.75f;

            // - Enable permadeath          : x2
            if (options.IsPermadeathOn)
                rating *= 2.0f;

            // - Aggressive Hungry Civs     : x0.5 / x2
            if (!options.IsAggressiveHungryCiviliansOn)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating *= 0.5f;
                else
                    rating *= 2f;
            }
            // - Rats Upgrade   
            if (options.RatsUpgrade)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating *= 1.10f;
                else
                    rating *= 0.90f;
            }
            // - Skeletons Upgrade
            if (options.SkeletonsUpgrade)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating *= 1.20f;
                else
                    rating *= 0.80f;
            }
            // - Shamblers Upgrade  
            if (options.ShamblersUpgrade)
            {
                if (side == DifficultySide.FOR_SURVIVOR)
                    rating *= 1.25f;
                else
                    rating *= 0.75f;
            }
            #endregion

            /////////////////////////////
            // Divide by reincarnation.
            ////////////////////////////
            rating /= (1 + reincarnationNumber);

            // done.
            return Math.Max(rating, 0);
        }
        #endregion

        #region Kills & Sightings
        /// <summary>
        /// Add kill to record and increase kill points.
        /// Distinguish killing as living vs killing as undead.
        /// </summary>
        public void AddKill(Actor player, Actor victim, int turn)
        {
            int actorModelID = victim.Model.ID;

            // add kill.
            KillData data;
            if (m_Kills.TryGetValue(actorModelID, out data))
            {
                ++data.Amount;
            }
            else
            {
                // first kill!
                m_Kills.Add(actorModelID, new KillData(actorModelID, turn));
                m_Events.Add(new GameEventData(turn, String.Format("Killed first {0}.", Models.Actors[actorModelID].Name)));
            }

            // add to score.
            m_KillPoints += Models.Actors[actorModelID].ScoreValue;

            // killing livings as undead give bonuses.
            if (m_Side == DifficultySide.FOR_UNDEAD && !Models.Actors[actorModelID].Abilities.IsUndead)
                m_KillPoints += SCORE_BONUS_FOR_KILLING_LIVING_AS_UNDEAD;
        }

        public void AddSighting(int actorModelID, int turn)
        {
            // ignore if already sighted.
            if (m_Sightings.Contains(actorModelID))
                return;

            // add.
            m_Sightings.Add(actorModelID);
            m_Events.Add(new GameEventData(turn, String.Format("Sighted first {0}.", Models.Actors[actorModelID].Name)));
        }

        public bool HasSighted(int actorModelID)
        {
            return m_Sightings.Contains(actorModelID);
        }
        #endregion

        #region Map & zones visits
        public bool HasVisited(Map map)
        {
            return m_VisitedMaps.Contains(map);
        }

        public void AddVisit(int turn, Map map)
        {
            lock (m_VisitedMaps) // thread safe
            {
                m_VisitedMaps.Add(map);
            }
        }
        #endregion

        #region Dying : stuff to remember at death.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="k">can be null</param>
        public void SetKiller(Actor k)
        {
            m_Killer = k;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="z">can be null</param>
        public void SetZombifiedPlayer(Actor z)
        {
            m_ZombifiedPlayer = z;
        }

        /// <summary>
        /// Adds an actor that was a player follower at time of death.
        /// </summary>
        /// <param name="fo"></param>
        public void AddFollowerWhenDied(Actor fo)
        {
            if (m_FollowersWhenDied == null)
                m_FollowersWhenDied = new List<Actor>();
            m_FollowersWhenDied.Add(fo);
        }
        #endregion

        #region Misc events
        public void AddEvent(int turn, string text)
        {
            lock (m_Events) // thread safe.
            {
                m_Events.Add(new GameEventData(turn, text));
            }
        }
        #endregion
    }
}
