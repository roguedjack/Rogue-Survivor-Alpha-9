using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemExplosive : Item
    {
        #region Fields
     
        #endregion

        #region Properties
        public int PrimedModelID { get; private set; }
        #endregion

        #region Init
        public ItemExplosive(ItemModel model, ItemModel primedModel)
            : base(model)
        {
            if (!(model is ItemExplosiveModel))
                throw new ArgumentException("model is not ItemExplosiveModel");

            this.PrimedModelID = primedModel.ID;
        }
        #endregion
    }
}
