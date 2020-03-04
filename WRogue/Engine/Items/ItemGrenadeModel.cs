using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemGrenadeModel : ItemExplosiveModel
    {
        #region Fields
        int m_MaxThrowDistance;
        #endregion

        #region Properties
        public int MaxThrowDistance { get { return m_MaxThrowDistance; } }
        #endregion

        #region Init
        public ItemGrenadeModel(string aName, string theNames, string imageID, int fuseDelay, BlastAttack attack, string blastImageID, int maxThrowDistance) 
            : base(aName, theNames, imageID, fuseDelay, attack, blastImageID)
        {
            m_MaxThrowDistance = maxThrowDistance;
        }
        #endregion
    }
}
