using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemTrap : Item
    {
        #region Fields
        bool m_IsActivated;
        bool m_IsTriggered;
        #endregion

        #region Properties
        public bool IsActivated
        {
            get { return m_IsActivated;}
            set { m_IsActivated=value;}
        }

        public bool IsTriggered
        {
            get { return m_IsTriggered; }
            set { m_IsTriggered = value; }
        }

        public ItemTrapModel TrapModel { get { return Model as ItemTrapModel; } }
        #endregion

        #region Init
        public ItemTrap(ItemModel model)
            : base(model)
        {
            if (!(model is ItemTrapModel))
                throw new ArgumentException("model is not a TrapModel");
        }
        #endregion

        #region Cloning
        public ItemTrap Clone()
        {
            ItemTrap c = new ItemTrap(TrapModel);
            return c;
        }
        #endregion
    }
}
