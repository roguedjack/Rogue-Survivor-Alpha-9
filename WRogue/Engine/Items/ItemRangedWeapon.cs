using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemRangedWeapon : ItemWeapon
    {
        #region Fields
        int m_Ammo;
        AmmoType m_AmmoType;
        #endregion

        #region Properties
        public int Ammo
        {
            get { return m_Ammo; }
            set { m_Ammo = value; }
        }

        public AmmoType AmmoType
        {
            get { return m_AmmoType; }
        }
        #endregion

        #region Init
        public ItemRangedWeapon(ItemModel model)
            : base(model)
        {
            if(!(model is ItemRangedWeaponModel))
                throw new ArgumentException("model is not RangedWeaponModel");

            ItemRangedWeaponModel m = model as ItemRangedWeaponModel;

            m_Ammo = m.MaxAmmo;
            m_AmmoType = m.AmmoType;
        }
        #endregion
    }
}
