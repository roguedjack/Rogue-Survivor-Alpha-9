using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    enum AmmoType
    {
        _FIRST = 0,

        LIGHT_PISTOL = _FIRST,
        HEAVY_PISTOL,
        SHOTGUN,
        LIGHT_RIFLE,
        HEAVY_RIFLE,
        BOLT,

        _COUNT
    }

    class ItemAmmoModel : ItemModel
    {
        #region Fields
        AmmoType m_AmmoType;
        #endregion

        #region Properties
        public AmmoType AmmoType
        {
            get { return m_AmmoType; }
        }

        public int MaxQuantity
        {
            get { return this.StackingLimit; }
        }
        #endregion

        #region Init
        public ItemAmmoModel(string aName, string theNames, string imageID, AmmoType ammoType, int maxQuantity)
            : base(aName, theNames, imageID)
        {
            m_AmmoType = ammoType;
            this.IsStackable = true;
            this.StackingLimit = maxQuantity;
        }
        #endregion
    }
}
