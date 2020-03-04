using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemLight : Item
    {
        #region Fields
        int m_Batteries;
        #endregion

        #region Properties
        public int Batteries
        {
            get { return m_Batteries; }
            set
            {
                if (value < 0) value = 0;
                m_Batteries = Math.Min( value, (this.Model as ItemLightModel).MaxBatteries);
            }
        }

        public int FovBonus
        {
            get { return (this.Model as ItemLightModel).FovBonus; }
        }

        public bool IsFullyCharged
        {
            get { return m_Batteries >= (this.Model as ItemLightModel).MaxBatteries; }
        }

        public override string ImageID
        {
            get
            {
                if (this.IsEquipped && this.Batteries > 0)
                    return base.ImageID;
                else
                    return (this.Model as ItemLightModel).OutOfBatteriesImageID;
            }
        }
        #endregion

        #region Init
        public ItemLight(ItemModel model)
            : base(model)
        {
            if (!(model is ItemLightModel))
                throw new ArgumentException("model is not a LightModel");

            this.Batteries = (model as ItemLightModel).MaxBatteries;
        }
        #endregion
    }
}
