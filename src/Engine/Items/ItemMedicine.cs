using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemMedicine : Item
    {
        #region Properties
        public int Healing { get; private set; }
        public int StaminaBoost { get; private set; }
        public int SleepBoost { get; private set; }
        public int InfectionCure { get; private set; }
        public int SanityCure { get; private set; }
        #endregion

        #region Init
        public ItemMedicine(ItemModel model)
            : base(model)
        {
            if (!(model is ItemMedicineModel))
                throw new ArgumentException("model is not a MedecineModel");

            ItemMedicineModel m = model as ItemMedicineModel;
            this.Healing = m.Healing;
            this.StaminaBoost = m.StaminaBoost;
            this.SleepBoost = m.SleepBoost;
            this.InfectionCure = m.InfectionCure;
            this.SanityCure = m.SanityCure;
        }
        #endregion

    }
}
