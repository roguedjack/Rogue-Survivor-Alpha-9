using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemTrapModel : ItemModel
    {
        #region Flags
        [Flags]
        enum Flags
        {
            NONE                = 0,
            USE_TO_ACTIVATE     = (1<<0),
            IS_NOISY            = (1<<1),
            IS_ONE_TIME_USE     = (1<<2),         
            IS_FLAMMABLE        = (1<<3),
            DROP_ACTIVATE       = (1<<4)
        }
        #endregion

        #region Fields
        Flags m_Flags;
        int m_TriggerChance;
        int m_Damage;
        int m_BreakChance;
        int m_BreakChanceWhenEscape;
        int m_BlockChance;
        string m_NoiseName;
        #endregion

        #region Properties
        public int TriggerChance { get { return m_TriggerChance; } }
        public int Damage { get { return m_Damage; } }
        public bool UseToActivate { get { return (m_Flags & Flags.USE_TO_ACTIVATE) != 0; } }
        public bool IsNoisy { get { return (m_Flags & Flags.IS_NOISY) != 0; } }
        public bool IsOneTimeUse { get { return (m_Flags & Flags.IS_ONE_TIME_USE) != 0; } }
        public bool IsFlammable { get { return (m_Flags & Flags.IS_FLAMMABLE) != 0; } }
        public bool ActivatesWhenDropped { get { return (m_Flags & Flags.DROP_ACTIVATE) != 0; } }
        public int BreakChance { get { return m_BreakChance; } }
        public int BlockChance { get { return m_BlockChance; } }
        public int BreakChanceWhenEscape { get { return m_BreakChanceWhenEscape; } }
        public string NoiseName { get { return m_NoiseName; } }
        #endregion

        #region Init
        public ItemTrapModel(string aName, string theNames, string imageID, int stackLimit, int triggerChance, int damage, 
            bool dropActivate, bool useToActivate, bool IsOneTimeUse, 
            int breakChance, int blockChance, int breakChanceWhenEscape,
            bool IsNoisy, string noiseName, bool IsFlammable)
            : base(aName, theNames, imageID)
        {
            this.DontAutoEquip = true;

            if (stackLimit > 1)
            {
                this.IsStackable = true;
                this.StackingLimit = stackLimit;
            }
            m_TriggerChance = triggerChance;
            m_Damage = damage;
            m_BreakChance = breakChance;
            m_BlockChance = blockChance;
            m_BreakChanceWhenEscape = breakChanceWhenEscape;
            m_Flags = Flags.NONE;
            if (dropActivate) m_Flags |= Flags.DROP_ACTIVATE;
            if (useToActivate) m_Flags |= Flags.USE_TO_ACTIVATE;
            if (IsNoisy)
            {
                m_Flags |= Flags.IS_NOISY;
                m_NoiseName = noiseName;
            }
            if (IsOneTimeUse) m_Flags |= Flags.IS_ONE_TIME_USE;
            if (IsFlammable) m_Flags |= Flags.IS_FLAMMABLE;
        }
        #endregion
    }
}
