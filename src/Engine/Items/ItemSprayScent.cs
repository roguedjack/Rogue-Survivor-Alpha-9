using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemSprayScent : Item
    {
        #region Properties
        public int SprayQuantity { get; set; }
        #endregion

        #region Init
        public ItemSprayScent(ItemModel model)
            : base(model)
        {
            if (!(model is ItemSprayScentModel))
                throw new ArgumentException("model is not a ItemScentSprayModel");

            this.SprayQuantity = (model as ItemSprayScentModel).MaxSprayQuantity;
        }
        #endregion
    }
}
