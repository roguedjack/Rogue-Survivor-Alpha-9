using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemBodyArmorModel : ItemModel
    {
        #region Fields
        int m_Protection_Hit;
        int m_Protection_Shot;
        int m_Encumbrance;
        int m_Weight;
        #endregion

        #region Properties
        public int Protection_Hit
        {
            get { return m_Protection_Hit; }
        }
        public int Protection_Shot
        {
            get { return m_Protection_Shot; }
        }
        public int Encumbrance
        {
            get { return m_Encumbrance; }
        }
        public int Weight
        {
            get { return m_Weight; }
        }
        #endregion

        #region Init
        public ItemBodyArmorModel(string aName, string theNames, string imageID, int protection_hit, int protection_shot, int encumbrance, int weight)
            : base(aName, theNames, imageID)
        {
            m_Protection_Hit = protection_hit;
            m_Protection_Shot = protection_shot;
            m_Encumbrance = encumbrance;
            m_Weight = weight;
        }
        #endregion

        #region Conversion
        public Defence ToDefence()
        {
            return new Defence(-m_Encumbrance, m_Protection_Hit, m_Protection_Shot);
        }
        #endregion
    }
}
