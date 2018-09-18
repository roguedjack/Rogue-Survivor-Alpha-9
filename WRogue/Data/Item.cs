using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Item
    {
        #region Fields
        int  m_ModelID;
        int  m_Quantity;
        DollPart m_EquipedPart;
        #endregion

        #region Properties
        public ItemModel Model
        {
            get { return Models.Items[m_ModelID]; }
        }

        public virtual string ImageID
        {
            get { return this.Model.ImageID; }
        }

        public string TheName
        {
            get
            {
                ItemModel model = this.Model;
                if (model.IsProper)
                    return model.SingleName;
                if (m_Quantity > 1 || model.IsPlural)
                    return "some " + model.PluralName;
                else
                    return "the " + model.SingleName;
            }
        }

        public string AName
        {
            get
            {
                ItemModel model = this.Model;
                if (model.IsProper)
                    return model.SingleName;
                if (m_Quantity > 1 || model.IsPlural)
                    return "some " + model.PluralName;
                else if (model.IsAn)
                    return "an " + model.SingleName;
                else
                    return "a " + model.SingleName;
            }
        }

        public int  Quantity
        {
            get { return m_Quantity; }
            set
            {
                m_Quantity = value;
                if (m_Quantity < 0) m_Quantity = 0;
            }
        }

        public bool CanStackMore
        {
            get
            {
                ItemModel myModel = this.Model;
                return myModel.IsStackable && m_Quantity < myModel.StackingLimit;
            }
        }

        public DollPart EquippedPart
        {
            get { return m_EquipedPart; }
            set { m_EquipedPart = value; }
        }

        public bool IsEquipped
        {
            get { return m_EquipedPart != DollPart.NONE; }
        }

        public bool IsUnique
        {
            get;
            set;
        }

        public bool IsForbiddenToAI
        {
            get;
            set;
        }
        #endregion

        #region Init
        public Item(ItemModel model)
        {
            m_ModelID = model.ID;
            m_Quantity = 1;
            m_EquipedPart = DollPart.NONE;
        }
        #endregion

        #region Pre-save
        public virtual void OptimizeBeforeSaving() { }
        #endregion
    }
}
