using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemGrenade : ItemExplosive
    {
        #region Fields

        #endregion

        #region Properties

        #endregion

        #region Init
        public ItemGrenade(ItemModel model, ItemModel primedModel)
            : base(model, primedModel)
        {
            if (!(model is ItemGrenadeModel))
                throw new ArgumentException("model is not ItemGrenadeModel");
        }
        #endregion
    }
}
