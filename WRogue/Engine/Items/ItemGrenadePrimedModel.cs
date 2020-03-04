using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemGrenadePrimedModel : ItemExplosiveModel
    {
        #region Fields

        #endregion

        #region Properties
        public ItemGrenadeModel GrenadeModel { get; private set; }
        #endregion

        #region Init
        public ItemGrenadePrimedModel(string aName, string theNames, string imageID, ItemGrenadeModel grenadeModel)
            : base(aName, theNames, imageID, grenadeModel.FuseDelay, grenadeModel.BlastAttack, grenadeModel.BlastImage)
        {
            if (grenadeModel == null)
                throw new ArgumentNullException("grenadeModel");

            this.GrenadeModel = grenadeModel;
        }
        #endregion
    }
}
