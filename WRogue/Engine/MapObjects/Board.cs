using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.MapObjects
{
    [Serializable]
    class Board : MapObject
    {
        #region Properties
        public string[] Text
        {
            get;
            set;
        }
        #endregion

        #region Init
        public Board(string name, string imageID, string[] text)
            : base(name, imageID)
        {
            this.Text = text;
        }
        #endregion

    }
}
