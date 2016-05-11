using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemSprayScentModel : ItemModel
    {
        #region Fields
        int m_MaxSprayQuantity;
        Odor m_Odor;
        int m_Strength;
        #endregion

        #region Properties
        public int MaxSprayQuantity
        {
            get { return m_MaxSprayQuantity;}
        }

        public int Strength
        {
            get { return m_Strength; }
        }

        public Odor Odor
        {
            get { return m_Odor;}
        }

        #endregion

        #region Init
        public ItemSprayScentModel(string aName, string theNames, string imageID, int sprayQuantity, Odor odor, int strength)
            : base(aName, theNames, imageID)
        {
            m_MaxSprayQuantity = sprayQuantity;
            m_Odor = odor;
            m_Strength = strength;
        }
        #endregion
    }
}
