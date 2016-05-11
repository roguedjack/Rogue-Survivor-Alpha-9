using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemMedicineModel : ItemModel
    {
        #region Fields
        int m_Healing;
        int m_StaminaBoost;
        int m_SleepBoost;
        int m_InfectionCure;
        int m_SanityCure;
        #endregion

        #region Properties
        public int Healing
        {
            get { return m_Healing; }
        }

        public int StaminaBoost
        {
            get { return m_StaminaBoost; }
        }

        public int SleepBoost
        {
            get { return m_SleepBoost; }
        }

        public int InfectionCure
        {
            get { return m_InfectionCure; }
        }

        public int SanityCure
        {
            get { return m_SanityCure; }
        }
        #endregion

        #region Init
        public ItemMedicineModel(string aName, string theNames, string imageID, int healing, int staminaBoost, int sleepBoost, int infectionCure, int sanityCure)
            : base(aName, theNames, imageID)
        {
            m_Healing = healing;
            m_StaminaBoost = staminaBoost;
            m_SleepBoost = sleepBoost;
            m_InfectionCure = infectionCure;
            m_SanityCure = sanityCure;
        }
        #endregion
    }
}
