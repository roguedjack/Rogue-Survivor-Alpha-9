using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemWeapon : Item
    {
        #region Init
        public ItemWeapon(ItemModel model)
            : base(model)
        {
            if (!(model is ItemWeaponModel))
                throw new ArgumentException("model is not a WeaponModel");
        }
        #endregion
    }
}
