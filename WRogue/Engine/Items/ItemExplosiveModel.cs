using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemExplosiveModel : ItemModel
    {
        #region Fields
        int m_FuseDelay;
        BlastAttack m_Attack;
        string m_BlastImageID;
        #endregion

        #region Properties
        public int FuseDelay { get { return m_FuseDelay; } }
        public BlastAttack BlastAttack { get { return m_Attack; } }
        public string BlastImage { get { return m_BlastImageID; } }
        #endregion

        #region Init
        public ItemExplosiveModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID)
            : base(aName, theNames, imageID)
        {
            m_FuseDelay = fuseDelay;
            m_Attack = attack;
            m_BlastImageID = blastImageID;
        }
        #endregion
    }
}
