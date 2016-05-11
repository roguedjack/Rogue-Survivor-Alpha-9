using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemWeaponModel : ItemModel
    {
        #region Fields
        Attack m_Attack;
        #endregion

        #region Properties
        public Attack Attack
        {
            get { return m_Attack; }
        }
        #endregion

        #region Init
        public ItemWeaponModel(string aName, string theNames, string imageID, Attack attack)
            : base(aName, theNames, imageID)
        {
            m_Attack = attack;
        }
        #endregion
    }
}
