using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemGrenadePrimed : ItemPrimedExplosive
    {
        #region Init
        public ItemGrenadePrimed(ItemModel model)
            : base(model)
        {
            if (!(model is ItemGrenadePrimedModel))
                throw new ArgumentException("model is not ItemGrenadePrimedModel");
        }
        #endregion
    }
}
