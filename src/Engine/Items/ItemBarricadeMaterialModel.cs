using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemBarricadeMaterialModel : ItemModel
    {
        #region Fields
        int m_BarricadingValue;
        #endregion

        #region Properties
        public int BarricadingValue
        {
            get { return m_BarricadingValue; }
        }
        #endregion

        #region Init
        public ItemBarricadeMaterialModel(string aName, string theNames, string imageID, int barricadingValue)
            : base(aName, theNames, imageID)
        {
            m_BarricadingValue = barricadingValue;
        }
        #endregion
    }
}
