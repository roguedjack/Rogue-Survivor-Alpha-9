using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemEntertainmentModel : ItemModel
    {
        #region Fields
        int m_Value;
        int m_BoreChance;
        #endregion

        #region Properties
        public int Value
        {
            get { return m_Value; }
            set { m_Value = value; }
        }

        public int BoreChance
        {
            get { return m_BoreChance; }
            set { m_BoreChance = value; }
        }
        #endregion

        #region Init
        public ItemEntertainmentModel(string aName, string theNames, string imageID, int value, int boreChance)
            : base(aName, theNames, imageID)
        {
            m_Value = value;
            m_BoreChance = boreChance;
        }
        #endregion
    }
}
