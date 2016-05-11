using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemSprayPaintModel : ItemModel
    {
        #region Fields
        int m_MaxPaintQuantity;
        string m_TagImageID;
        #endregion

        #region Properties
        public int MaxPaintQuantity
        {
            get { return m_MaxPaintQuantity; }
        }

        public string TagImageID
        {
            get { return m_TagImageID; }
        }

        #endregion

        #region Init
        public ItemSprayPaintModel(string aName, string theNames, string imageID, int paintQuantity, string tagImageID)
            : base(aName, theNames, imageID)
        {
            if (tagImageID == null)
                throw new ArgumentNullException("tagImageID");

            m_MaxPaintQuantity = paintQuantity;
            m_TagImageID = tagImageID;
        }
        #endregion
    }
}
