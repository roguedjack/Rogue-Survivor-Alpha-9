using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemFood : Item
    {
        #region Properties
        public int Nutrition { get; private set; }
        public bool IsPerishable { get; private set; }
        public WorldTime BestBefore { get; private set; }
        #endregion

        #region Init
        /// <summary>
        /// Not perishable.
        /// </summary>
        /// <param name="model"></param>
        public ItemFood(ItemModel model)
            : base(model)
        {
            if (!(model is ItemFoodModel))
                throw new ArgumentException("model is not a FoodModel");

            this.Nutrition = (model as ItemFoodModel).Nutrition;
            this.IsPerishable = false;

        }

        /// <summary>
        /// Perishable food.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="bestBefore"></param>
        public ItemFood(ItemModel model, int bestBefore)
            : base(model)
        {
            if (!(model is ItemFoodModel))
                throw new ArgumentException("model is not a FoodModel");

            this.Nutrition = (model as ItemFoodModel).Nutrition;
            this.BestBefore = new WorldTime(bestBefore);
            this.IsPerishable = true;
        }
        #endregion
    }
}
