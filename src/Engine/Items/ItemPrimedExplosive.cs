using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemPrimedExplosive : ItemExplosive
    {
        #region Fields

        #endregion

        #region Properties
        public int FuseTimeLeft { get; set; }
        #endregion

        #region Init
        public ItemPrimedExplosive(ItemModel model)
            : base(model, model)
        {
            if (!(model is ItemExplosiveModel))
                throw new ArgumentException("model is not ItemExplosiveModel");

            this.FuseTimeLeft = (model as ItemExplosiveModel).FuseDelay;
        }
        #endregion
    }
}
