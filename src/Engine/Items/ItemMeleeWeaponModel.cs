using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemMeleeWeaponModel : ItemWeaponModel
    {
        #region Properties
        public bool IsFragile { get; set; }
        #endregion

        #region Init
        public ItemMeleeWeaponModel(string aName, string theNames, string imageID, Attack attack)
            : base(aName, theNames, imageID, attack)
        {
        }
        #endregion
    }
}
