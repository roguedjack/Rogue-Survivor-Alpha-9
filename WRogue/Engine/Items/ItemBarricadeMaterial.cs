using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemBarricadeMaterial : Item
    {
        #region Init
        public ItemBarricadeMaterial(ItemModel model) : base(model)
        {
            if (!(model is ItemBarricadeMaterialModel))
                throw new ArgumentException("model is not BarricadeMaterialModel");
        }
        #endregion
    }
}
