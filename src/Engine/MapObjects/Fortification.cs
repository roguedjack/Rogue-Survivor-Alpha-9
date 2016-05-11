using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.MapObjects
{
    [Serializable]
    class Fortification : MapObject
    {
        #region Constants
        public const int SMALL_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS / 2;
        public const int LARGE_BASE_HITPOINTS = DoorWindow.BASE_HITPOINTS;
        #endregion

        #region Init
        public Fortification(string name, string imageID, int hitPoints)
            : base(name, imageID, Break.BREAKABLE, Fire.BURNABLE, hitPoints)
        {
        }
        #endregion

    }
}
