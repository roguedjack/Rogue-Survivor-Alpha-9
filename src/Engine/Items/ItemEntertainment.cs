using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemEntertainment : Item
    {
        #region Properties
        public ItemEntertainmentModel EntertainmentModel { get { return this.Model as ItemEntertainmentModel; } }
        #endregion

        #region Init
        public ItemEntertainment(ItemModel model)
            : base(model)
        {
            if (!(model is ItemEntertainmentModel))
                throw new ArgumentException("model is not a EntertainmentModel");
        }
        #endregion
    }
}
