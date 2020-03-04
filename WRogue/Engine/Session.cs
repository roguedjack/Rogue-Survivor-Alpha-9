using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Drawing;
using System.Xml;
using System.Xml.Serialization;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    enum GameMode
    {
        GM_STANDARD,
        GM_CORPSES_INFECTION,
        GM_VINTAGE
    }

    [Serializable]
    enum ScriptStage
    {
        STAGE_0,
        STAGE_1,
        STAGE_2,
        STAGE_3,
        STAGE_4,
        STAGE_5
    }

    [Serializable]
    enum RaidType
    {
        _FIRST = 0,

        BIKERS = _FIRST,
        GANGSTA,
        BLACKOPS,
        SURVIVORS,

        /// <summary>
        /// "Fake" raid for AIs.
        /// </summary>
        NATGUARD,

        /// <summary>
        /// "Fake" raid for AIs.
        /// </summary>
        ARMY_SUPLLIES,

        _COUNT
    }

    [Serializable]
    enum AdvisorHint
    {
        _FIRST = 0,

        /// <summary>
        /// Basic movement directions.
        /// </summary>
        MOVE_BASIC = _FIRST,

        /// <summary>
        /// Looking with the mouse.
        /// </summary>
        MOUSE_LOOK,

        /// <summary>
        /// Redefining keys & options.
        /// </summary>
        KEYS_OPTIONS,

        /// <summary>
        /// Night effects.
        /// </summary>
        NIGHT,

        /// <summary>
        /// Rainy weather effects.
        /// </summary>
        RAIN,

        /// <summary>
        /// Attacking in melee.
        /// </summary>
        ACTOR_MELEE,

        /// <summary>
        /// Running.
        /// </summary>
        MOVE_RUN,

        /// <summary>
        /// Resting.
        /// </summary>
        MOVE_RESTING,

        /// <summary>
        /// Jumping.
        /// </summary>
        MOVE_JUMP,

        /// <summary>
        /// Grabbing an item from a container.
        /// </summary>
        ITEM_GRAB_CONTAINER,

        /// <summary>
        /// Grabbing an item from the floor.
        /// </summary>
        ITEM_GRAB_FLOOR,

        /// <summary>
        /// Unequiping an item.
        /// </summary>
        ITEM_UNEQUIP,

        /// <summary>
        /// Equiping an item.
        /// </summary>
        ITEM_EQUIP,

        /// <summary>
        /// Barricading material.
        /// </summary>
        ITEM_TYPE_BARRICADING,

        /// <summary>
        /// Dropping an item.
        /// </summary>
        ITEM_DROP,

        /// <summary>
        /// Using an item.
        /// </summary>
        ITEM_USE,

        /// <summary>
        /// Flashlights.
        /// </summary>
        FLASHLIGHT,

        /// <summary>
        /// Cellphones.
        /// </summary>
        CELLPHONES,

        /// <summary>
        /// Using spraypaint.
        /// </summary>
        SPRAYS_PAINT,

        /// <summary>
        /// Using scent sprays.
        /// </summary>
        SPRAYS_SCENT,

        /// <summary>
        /// Firing a weapon.
        /// </summary>
        WEAPON_FIRE,

        /// <summary>
        /// Reloading a weapon.
        /// </summary>
        WEAPON_RELOAD,

        /// <summary>
        /// Using grenades.
        /// </summary>
        GRENADE,

        /// <summary>
        /// Opening a door/window.
        /// </summary>
        DOORWINDOW_OPEN,

        /// <summary>
        /// Closing a door/window.
        /// </summary>
        DOORWINDOW_CLOSE,

        /// <summary>
        /// Pushing/Pulling objects/actors.
        /// </summary>
        OBJECT_PUSH,

        /// <summary>
        /// Breaking objects.
        /// </summary>
        OBJECT_BREAK,

        /// <summary>
        /// Barricading.
        /// </summary>
        BARRICADE,

        /// <summary>
        /// Using an exit such as ladders, stairs.
        /// </summary>
        EXIT_STAIRS_LADDERS,

        /// <summary>
        /// Using an exit to leave the district.
        /// </summary>
        EXIT_LEAVING_DISTRICT,

        /// <summary>
        /// Sleeping.
        /// </summary>
        STATE_SLEEPY,

        /// <summary>
        /// Eating.
        /// </summary>
        STATE_HUNGRY,

        /// <summary>
        /// Trading with NPCs.
        /// </summary>
        NPC_TRADE,

        /// <summary>
        /// Giving items.
        /// </summary>
        NPC_GIVING_ITEM,

        /// <summary>
        /// Shouting.
        /// </summary>
        NPC_SHOUTING,

        /// <summary>
        /// Building fortifications.
        /// </summary>
        BUILD_FORTIFICATION,

        /// <summary>
        /// Leading : need Leadership skill.
        /// </summary>
        LEADING_NEED_SKILL,

        /// <summary>
        /// Leading : can recruit someone.
        /// </summary>
        LEADING_CAN_RECRUIT,

        /// <summary>
        /// Leading : give orders.
        /// </summary>
        LEADING_GIVE_ORDERS,

        /// <summary>
        /// Leading : switching place.
        /// </summary>
        LEADING_SWITCH_PLACE,

        /// <summary>
        /// Saving/Loading.
        /// </summary>
        GAME_SAVE_LOAD,

        /// <summary>
        /// City Information.
        /// </summary>
        CITY_INFORMATION,

        // alpha10 merge corpse hints
        /// <summary>
        /// Corpse actions.
        /// </summary>
        CORPSE,

        /// <summary>
        /// Eating corpses (undead).
        /// </summary>
        CORPSE_EAT,

        // alpha10 new hints

        /// <summary>
        /// Sanity.
        /// </summary>
        SANITY,

        /// <summary>
        /// Infection.
        /// </summary>
        INFECTION,

        /// <summary>
        /// Traps.
        /// </summary>
        TRAPS,

        _COUNT
    }

    [Serializable]
    class UniqueActor
    {
        public bool IsSpawned { get; set; }
        public Actor TheActor { get; set; }
        public bool IsWithRefugees { get; set; }
        public string EventThemeMusic { get; set; }
        public string EventMessage { get; set; }
    }

    [Serializable]
    class UniqueActors
    {
        public UniqueActor BigBear { get; set; }
        public UniqueActor Duckman { get; set; }
        public UniqueActor FamuFataru { get; set; }
        public UniqueActor HansVonHanz { get; set; }
        public UniqueActor JasonMyers { get; set; }
        public UniqueActor PoliceStationPrisoner { get; set; }
        public UniqueActor Roguedjack { get; set; }
        public UniqueActor Santaman { get; set; }
        public UniqueActor TheSewersThing { get; set; }

        /// <summary>
        /// Allocate a new array each call don't overuse it...
        /// TODO -- consider caching it.
        /// </summary>
        /// <returns></returns>
        public UniqueActor[] ToArray()
        {
            return new UniqueActor[] {
                BigBear, Duckman, FamuFataru, HansVonHanz, Roguedjack, Santaman,
                PoliceStationPrisoner,  TheSewersThing,
                JasonMyers  // alpha10
            };
        }
    }

    [Serializable]
    class UniqueItem
    {
        public bool IsSpawned { get; set; }
        public Item TheItem { get; set; }
    }

    [Serializable]
    class UniqueItems
    {
        public UniqueItem TheSubwayWorkerBadge { get; set; }
    }

    [Serializable]
    class UniqueMap
    {
        public Map TheMap { get; set; }
    }

    [Serializable]
    class UniqueMaps
    {
        public UniqueMap CHARUndergroundFacility { get; set; }
        public UniqueMap PoliceStation_OfficesLevel { get; set; }
        public UniqueMap PoliceStation_JailsLevel { get; set; }
        public UniqueMap Hospital_Admissions { get; set; }
        public UniqueMap Hospital_Offices { get; set; }
        public UniqueMap Hospital_Patients { get; set; }
        public UniqueMap Hospital_Storage { get; set; }
        public UniqueMap Hospital_Power { get; set; }
    }

    /// <summary>
    /// All the data that is needed to represent the game state, or in other words everything that need to be saved and loaded.
    /// </summary>
    [Serializable]
    class Session
    {
        public enum SaveFormat
        {
            FORMAT_BIN,
            FORMAT_SOAP,
            FORMAT_XML
        }

        #region Fields

        #region Game Mode
        GameMode m_GameMode;
        #endregion

        #region World map
        WorldTime m_WorldTime;
        World m_World;
        Map m_CurrentMap;
        #endregion

        #region Scoring
        Scoring m_Scoring;
        #endregion

        #region Events
        /// <summary>
        /// [RaidType, District.WorldPosition.X, District.WorldPosition.Y] -> turnCounter
        /// </summary>
        int[, ,] m_Event_Raids;
        #endregion

        #region Advisor

        #endregion

        // alpha10.1
        #region AutoSave
        int m_NextAutoSaveTime;
        #endregion

        [NonSerialized]
        static Session s_TheSession;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the curent Session (singleton).
        /// </summary>
        public static Session Get
        {
            get 
            {
                if (s_TheSession == null)
                    s_TheSession = new Session();
                return s_TheSession; 
            }
        }

        public GameMode GameMode
        {
            get { return m_GameMode; }
            set { m_GameMode = value; }
        }

        public int Seed { get; set; }
        public WorldTime WorldTime { get { return m_WorldTime; } }
        public int LastTurnPlayerActed { get; set; }

        public World World
        {
            get { return m_World; }
            set { m_World = value; }
        }

        public Map CurrentMap
        {
            get { return m_CurrentMap; }
            set { m_CurrentMap = value; }
        }

        public Scoring Scoring
        {
            get { return m_Scoring; }
        }

        // alpha10.01
        public int NextAutoSaveTime
        {
            get { return m_NextAutoSaveTime; }
            set { m_NextAutoSaveTime = value; }
        }

        #region Uniques

        public UniqueActors UniqueActors { get; set; }
        public UniqueItems UniqueItems { get; set; }
        public UniqueMaps UniqueMaps {get; set; }
  
        #endregion

        #region Special flags
        public bool PlayerKnows_CHARUndergroundFacilityLocation
        {
            get;
            set;
        }

        public bool PlayerKnows_TheSewersThingLocation
        {
            get;
            set;
        }

        public bool CHARUndergroundFacility_Activated
        {
            get;
            set;
        }

        public ScriptStage ScriptStage_PoliceStationPrisoner
        {
            get;
            set;
        }

        // alpha10
        public FireMode Player_CurrentFireMode { get; set; }

        public int Player_TurnCharismaRoll { get; set; }
        #endregion

        #endregion

        #region Init
        Session()
        {
            Reset();
        }

        public void Reset()
        {
            this.Seed = (int)DateTime.UtcNow.TimeOfDay.Ticks;
            m_CurrentMap = null;
            m_Scoring = new Scoring();
            m_World = null;
            m_WorldTime = new WorldTime();
            this.LastTurnPlayerActed = 0;

            m_Event_Raids = new int[(int)RaidType._COUNT, RogueGame.Options.CitySize, RogueGame.Options.CitySize];
            for (int i = (int)RaidType._FIRST; i < (int)RaidType._COUNT; i++)
            {
                for (int x = 0; x < RogueGame.Options.CitySize; x++)
                    for (int y = 0; y < RogueGame.Options.CitySize; y++)
                    {
                        m_Event_Raids[i, x, y] = -1;
                    }
            }

            ////////////////////////////
            // Reset special properties.
            ////////////////////////////
            this.CHARUndergroundFacility_Activated = false;
            this.PlayerKnows_CHARUndergroundFacilityLocation = false;
            this.PlayerKnows_TheSewersThingLocation = false;
            this.ScriptStage_PoliceStationPrisoner = ScriptStage.STAGE_0;
            this.UniqueActors = new UniqueActors();
            this.UniqueItems = new UniqueItems();
            this.UniqueMaps = new UniqueMaps();
            // alpha10
            this.Player_CurrentFireMode = FireMode.DEFAULT;
            this.Player_TurnCharismaRoll = 0;
            // alpha10.1
            m_NextAutoSaveTime = 0;
        }
        #endregion

        #region Events
        public bool HasRaidHappened(RaidType raid, District district)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            return m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y] > -1;
        }

        public int LastRaidTime(RaidType raid, District district)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            return m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y];
        }

        public void SetLastRaidTime(RaidType raid, District district, int turnCounter)
        {
            if (district == null)
                throw new ArgumentNullException("district");

            lock (m_Event_Raids) // thread safe.
            {
                m_Event_Raids[(int)raid, district.WorldPosition.X, district.WorldPosition.Y] = turnCounter;
            }
        }
        #endregion

        #region Saving & Loading
        public static void Save(Session session, string filepath, SaveFormat format)
        {
            // optimize.
            session.World.OptimizeBeforeSaving();

            // save.
            switch (format)
            {
                case SaveFormat.FORMAT_BIN: SaveBin(session, filepath); break;
                case SaveFormat.FORMAT_SOAP: SaveSoap(session, filepath); break;
                case SaveFormat.FORMAT_XML: SaveXml(session, filepath); break;
            }
        }

        public static bool Load(string filepath, SaveFormat format)
        {
            switch (format)
            {
                case SaveFormat.FORMAT_BIN: return LoadBin(filepath);
                case SaveFormat.FORMAT_SOAP: return LoadSoap(filepath); 
                case SaveFormat.FORMAT_XML: return LoadXml(filepath);
                default: return false;
            }
        }

        static void SaveBin(Session session, string filepath)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");

            IFormatter formatter = CreateFormatter();
            using (Stream stream = CreateStream(filepath, true))
            {
                formatter.Serialize(stream, session);
                stream.Flush();
                stream.Close();
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        static bool LoadBin(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");

            try
            {
                // deserialize.
                IFormatter formatter = CreateFormatter();
                using (Stream stream = CreateStream(filepath, false))
                {
                    s_TheSession = (Session)formatter.Deserialize(stream);
                    stream.Close();
                }

                // reconstruct auxiliary fields.
                s_TheSession.ReconstructAuxiliaryFields();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                s_TheSession = null;
                return false;
            }


            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
            return true;
        }

        static void SaveSoap(Session session, string filepath)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");

            IFormatter formatter = CreateSoapFormatter();
            using (Stream stream = CreateStream(filepath, true))
            {
                formatter.Serialize(stream, session);
                stream.Flush();
                stream.Close();
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        static bool LoadSoap(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");

            try
            {
                // deserialize.
                IFormatter formatter = CreateSoapFormatter();
                using (Stream stream = CreateStream(filepath, false))
                {
                    s_TheSession = (Session)formatter.Deserialize(stream);
                    stream.Close();
                }

                // reconstruct auxiliary fields.
                s_TheSession.ReconstructAuxiliaryFields();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                s_TheSession = null;
                return false;
            }


            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
            return true;
        }

        static void SaveXml(Session session, string filepath)
        {
            if (session == null)
                throw new ArgumentNullException("session");
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session...");

            XmlSerializer xs = new XmlSerializer(typeof(Session));
            using (Stream stream = CreateStream(filepath, true))
            {
                xs.Serialize(stream, session);
                stream.Flush();
                stream.Close();
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving session... done!");
        }

        /// <summary>
        /// Try to load, null if failed.
        /// </summary>
        /// <returns></returns>
        static bool LoadXml(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session...");

            try
            {
                // deserialize.
                XmlSerializer xs = new XmlSerializer(typeof(Session));
                using (Stream stream = CreateStream(filepath, false))
                {
                    s_TheSession = (Session)xs.Deserialize(stream);
                    stream.Flush();
                    stream.Close();
                }

                // reconstruct auxiliary fields.
                s_TheSession.ReconstructAuxiliaryFields();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load session (no save game?).");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                s_TheSession = null;
                return false;
            }


            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading session... done!");
            return true;
        }

        public static bool Delete(string filepath)
        {
            if (filepath == null)
                throw new ArgumentNullException("filepath");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game...");

            bool hasDeleted = false;
            try
            {
                File.Delete(filepath);
                hasDeleted = true;
            }
            catch (Exception e) 
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to delete saved game (no save?)");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "exception :");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, e.ToString());
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failing silently.");
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "deleting saved game... done!");

            return hasDeleted;
        }

        static IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        static IFormatter CreateSoapFormatter()
        {
            return new SoapFormatter();
        }

        static Stream CreateStream(string saveFileName, bool save)
        {
            return new FileStream(saveFileName,
                save ? FileMode.Create : FileMode.Open,
                save ? FileAccess.Write : FileAccess.Read,
                FileShare.None);
        }

        void ReconstructAuxiliaryFields()
        {
            // reconstruct all maps auxiliary fields.
            for(int x = 0; x < m_World.Size;x++)
                for (int y = 0; y < m_World.Size; y++)
                {
                    foreach (Map map in m_World[x, y].Maps)
                        map.ReconstructAuxiliaryFields();
                }
        }
        #endregion

        #region Helpers
        public static string DescGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.GM_STANDARD: return "STD - Standard Game";
                case GameMode.GM_CORPSES_INFECTION: return "C&I - Corpses & Infection";
                case GameMode.GM_VINTAGE: return "VTG - Vintage Zombies";
                default: throw new Exception("unhandled game mode");
            }
        }

        public static string DescShortGameMode(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.GM_STANDARD: return "STD";
                case GameMode.GM_CORPSES_INFECTION: return "C&I";
                case GameMode.GM_VINTAGE: return "VTG";
                default: throw new Exception("unhandled game mode");
            }
        }
        
        // alpha10
        public UniqueActor ActorToUniqueActor(Actor a)
        {
            if (!a.IsUnique)
                throw new ArgumentException("actor is not unique");
            foreach(UniqueActor unique in UniqueActors.ToArray())
            {
                if (unique.TheActor == a)
                    return unique;
            }
            throw new ArgumentException("actor is flaged as unique but did not find it!");
        }
        #endregion

#if DEBUG_STATS
        [Serializable]
        public class DistrictStat
        {
            [Serializable]
            public struct Record
            {
                public int livings;
                public int undeads;
            }

            public Dictionary<int, Record> TurnRecords = new Dictionary<int, Record>();
        }

        DistrictStat[,] m_Stats;

        #region Dev
        public void UpdateStats(District d)
        {
            if (m_Stats == null)
            {
                m_Stats = new DistrictStat[World.Size, World.Size];
                for (int x = 0; x < World.Size; x++)
                    for (int y = 0; y < World.Size; y++)
                        m_Stats[x, y] = new DistrictStat();
            }

            if (m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.ContainsKey(d.EntryMap.LocalTime.TurnCounter))
                return;

            int l = 0;
            int u = 0;
            foreach (Map m in d.Maps)
            {
                foreach (Actor a in m.Actors)
                {
                    if (a.IsDead) continue;
                    if (a.Model.Abilities.IsUndead)
                        ++u;
                    else
                        ++l;
                }
            }
            m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.Add(d.EntryMap.LocalTime.TurnCounter,
                new DistrictStat.Record()
                {
                    livings = l,
                    undeads = u
                });
        }

        public DistrictStat.Record? GetStatRecord(District d, int turn)
        {
            DistrictStat.Record record;
            if (!m_Stats[d.WorldPosition.X, d.WorldPosition.Y].TurnRecords.TryGetValue(turn, out record))
                return null;
            return record;
        }
        #endregion
#endif
    }
}
