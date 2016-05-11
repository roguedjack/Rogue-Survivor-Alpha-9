using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemMeleeWeapon : ItemWeapon
    {
        #region Properties
        public bool IsFragile
        {
            get { return (this.Model as ItemMeleeWeaponModel).IsFragile; }
        }
        #endregion

        #region Init
        public ItemMeleeWeapon(ItemModel model)
            : base(model)
        {
            if (!(model is ItemMeleeWeaponModel))
                throw new ArgumentException("model is not a MeleeWeaponModel");
        }
        #endregion
    }
}
