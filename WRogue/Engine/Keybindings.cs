using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace djack.RogueSurvivor.Engine
{
    [Serializable]
    class Keybindings
    {
        #region Fields
        Dictionary<PlayerCommand, Keys> m_CommandToKeyData;
        Dictionary<Keys, PlayerCommand> m_KeyToCommand;
        #endregion

        #region Init
        public Keybindings()
        {
            m_CommandToKeyData = new Dictionary<PlayerCommand, Keys>();
            m_KeyToCommand = new Dictionary<Keys, PlayerCommand>();
            ResetToDefaults();
        }

        public void ResetToDefaults()
        {
            m_CommandToKeyData.Clear();
            m_KeyToCommand.Clear();

            Set(PlayerCommand.BARRICADE_MODE, Keys.B);
            Set(PlayerCommand.BREAK_MODE, Keys.K);
            Set(PlayerCommand.CLOSE_DOOR, Keys.C);
            Set(PlayerCommand.FIRE_MODE, Keys.F);
            Set(PlayerCommand.HELP_MODE, Keys.H);
            Set(PlayerCommand.KEYBINDING_MODE, Keys.K | Keys.Shift);

            Set(PlayerCommand.ITEM_SLOT_0, Keys.D1);
            Set(PlayerCommand.ITEM_SLOT_1, Keys.D2);
            Set(PlayerCommand.ITEM_SLOT_2, Keys.D3);
            Set(PlayerCommand.ITEM_SLOT_3, Keys.D4);
            Set(PlayerCommand.ITEM_SLOT_4, Keys.D5);
            Set(PlayerCommand.ITEM_SLOT_5, Keys.D6);
            Set(PlayerCommand.ITEM_SLOT_6, Keys.D7);
            Set(PlayerCommand.ITEM_SLOT_7, Keys.D8);
            Set(PlayerCommand.ITEM_SLOT_8, Keys.D9);
            Set(PlayerCommand.ITEM_SLOT_9, Keys.D0);

            Set(PlayerCommand.ABANDON_GAME, Keys.A | Keys.Shift);
            Set(PlayerCommand.ADVISOR, Keys.H | Keys.Shift);
            Set(PlayerCommand.BUILD_LARGE_FORTIFICATION, Keys.N | Keys.Control);
            Set(PlayerCommand.BUILD_SMALL_FORTIFICATION, Keys.N);
            Set(PlayerCommand.CITY_INFO, Keys.I);
            Set(PlayerCommand.EAT_CORPSE, Keys.E | Keys.Shift);
            Set(PlayerCommand.GIVE_ITEM, Keys.G);
            Set(PlayerCommand.HINTS_SCREEN_MODE, Keys.H | Keys.Control);
            Set(PlayerCommand.NEGOCIATE_TRADE, Keys.E);
            Set(PlayerCommand.LOAD_GAME, Keys.L | Keys.Shift);
            Set(PlayerCommand.MARK_ENEMIES_MODE, Keys.E | Keys.Control);
            Set(PlayerCommand.MESSAGE_LOG, Keys.M | Keys.Shift);
            Set(PlayerCommand.MOVE_E, Keys.NumPad6);
            Set(PlayerCommand.MOVE_N, Keys.NumPad8);
            Set(PlayerCommand.MOVE_NE, Keys.NumPad9);
            Set(PlayerCommand.MOVE_NW, Keys.NumPad7);
            Set(PlayerCommand.MOVE_S, Keys.NumPad2);
            Set(PlayerCommand.MOVE_SE, Keys.NumPad3);
            Set(PlayerCommand.MOVE_SW, Keys.NumPad1);
            Set(PlayerCommand.MOVE_W, Keys.NumPad4);
            Set(PlayerCommand.OPTIONS_MODE, Keys.O | Keys.Shift);
            Set(PlayerCommand.ORDER_MODE, Keys.O);
            Set(PlayerCommand.PULL_MODE, Keys.P | Keys.Control); // alpha10
            Set(PlayerCommand.PUSH_MODE, Keys.P);
            Set(PlayerCommand.QUIT_GAME, Keys.Q | Keys.Shift);
            Set(PlayerCommand.REVIVE_CORPSE, Keys.R | Keys.Shift);
            Set(PlayerCommand.RUN_TOGGLE, Keys.R);
            Set(PlayerCommand.SAVE_GAME, Keys.S | Keys.Shift);
            Set(PlayerCommand.SCREENSHOT, Keys.N | Keys.Shift);
            Set(PlayerCommand.SHOUT, Keys.S);
            Set(PlayerCommand.SLEEP, Keys.Z);
            Set(PlayerCommand.SWITCH_PLACE, Keys.S | Keys.Control);
            Set(PlayerCommand.LEAD_MODE, Keys.T);
            Set(PlayerCommand.USE_SPRAY, Keys.A);
            Set(PlayerCommand.USE_EXIT, Keys.X);
            Set(PlayerCommand.WAIT_OR_SELF, Keys.NumPad5);
            Set(PlayerCommand.WAIT_LONG, Keys.W);
        }
        #endregion

        #region Getting & Setting
        /// <summary>
        /// Get KeyData (key code & modifiers).
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Keys Get(PlayerCommand command)
        {
            Keys key;
            if (m_CommandToKeyData.TryGetValue(command, out key))
                return key;
            else
                return Keys.None;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">KeyData</param>
        /// <returns></returns>
        public PlayerCommand Get(Keys key)
        {
            PlayerCommand cmd;
            if (m_KeyToCommand.TryGetValue(key, out cmd))
                return cmd;
            return PlayerCommand.NONE;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="key">KeyData</param>
        public void Set(PlayerCommand command, Keys key)
        {
            // remove previous bind.
            PlayerCommand prevCommand = Get(key);
            if (prevCommand != PlayerCommand.NONE)
            {
                m_CommandToKeyData.Remove(prevCommand);
            }
            Keys prevKey = Get(command);
            if (prevKey != Keys.None)
            {
                m_KeyToCommand.Remove(prevKey);
            }
                     
            // rebind.
            m_CommandToKeyData[command] = key;
            m_KeyToCommand[key] = command;
       }
        #endregion

        #region Checking for keys conflict
        public bool CheckForConflict()
        {
            foreach (Keys key1 in m_CommandToKeyData.Values)
            {
                int bound = m_KeyToCommand.Keys.Count((k) => k == key1);
                if (bound > 1)
                    return true;
            }

            return false;
        }
        #endregion

        #region Saving & Loading
        public static void Save(Keybindings kb, string filepath)
        {
            if (kb == null)
                throw new ArgumentNullException("kb");

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings...");

            IFormatter formatter = CreateFormatter();
            Stream stream = CreateStream(filepath, true);

            formatter.Serialize(stream, kb);
            stream.Flush();
            stream.Close();

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "saving keybindings... done!");
        }

        /// <summary>
        /// Attempt to load, if failed return bindings with defaults.
        /// </summary>
        /// <returns></returns>
        public static Keybindings Load(string filepath)
        {
            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings...");

            Keybindings kb;

            try
            {
                IFormatter formatter = CreateFormatter();
                Stream stream = CreateStream(filepath, false);

                kb = (Keybindings)formatter.Deserialize(stream);
                stream.Close();
            }
            catch (Exception e)
            {
                Logger.WriteLine(Logger.Stage.RUN_MAIN, "failed to load keybindings (first run?), using defaults.");
                Logger.WriteLine(Logger.Stage.RUN_MAIN, String.Format("load exception : {0}.", e.ToString()));
                kb = new Keybindings();
                kb.ResetToDefaults();
            }

            Logger.WriteLine(Logger.Stage.RUN_MAIN, "loading keybindings... done!");

            return kb;
        }

        static IFormatter CreateFormatter()
        {
            return new BinaryFormatter();
        }

        static Stream CreateStream(string saveName, bool save)
        {
            return new FileStream(saveName,
                save ? FileMode.Create : FileMode.Open,
                save ? FileAccess.Write : FileAccess.Read,
                FileShare.None);
        }
        #endregion
    }
}
