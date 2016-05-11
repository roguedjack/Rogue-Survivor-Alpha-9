using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemAmmo : Item
    {
        #region Fields
        AmmoType m_AmmoType;
        #endregion

        #region Properties
        public AmmoType AmmoType
        {
            get { return m_AmmoType; }
        }
        #endregion

        #region Init
        public ItemAmmo(ItemModel model)
            : base(model)
        {
            if (!(model is ItemAmmoModel))
                throw new ArgumentException("model is not a AmmoModel");

            ItemAmmoModel m = model as ItemAmmoModel;
            m_AmmoType = m.AmmoType;
            this.Quantity = m.MaxQuantity;
        }
        #endregion
    }
}
