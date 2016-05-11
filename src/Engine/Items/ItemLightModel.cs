using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemLightModel : ItemModel
    {
        #region Fields
        int m_MaxBatteries;
        int m_FovBonus;
        string m_OutOfBatteriesImageID;
        #endregion

        #region Properties
        public int MaxBatteries
        {
            get { return m_MaxBatteries; }
        }

        public int FovBonus
        {
            get { return m_FovBonus; }
        }

        public string OutOfBatteriesImageID
        {
            get { return m_OutOfBatteriesImageID; }
        }
        #endregion

        #region Init
        public ItemLightModel(string aName, string theNames, string imageID, int fovBonus, int maxBatteries, string outOfBatteriesImageID)
            : base(aName, theNames, imageID)
        {
            m_FovBonus = fovBonus;
            m_MaxBatteries = maxBatteries;
            m_OutOfBatteriesImageID = outOfBatteriesImageID;
            this.DontAutoEquip = true;
        }
        #endregion
    }
}
