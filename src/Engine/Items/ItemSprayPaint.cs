using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemSprayPaint : Item
    {
        #region Properties
        public int PaintQuantity { get; set; }
        #endregion

        #region Init
        public ItemSprayPaint(ItemModel model)
            : base(model)
        {
            if (!(model is ItemSprayPaintModel))
                throw new ArgumentException("model is not a SprayPaintModel");

            this.PaintQuantity = (model as ItemSprayPaintModel).MaxPaintQuantity;
        }
        #endregion
    }
}
