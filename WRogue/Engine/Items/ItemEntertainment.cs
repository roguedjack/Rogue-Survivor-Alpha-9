using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    [Serializable]
    class ItemEntertainment : Item
    {
        #region Fields
        // alpha10 boring items moved out of Actor
        List<Actor> m_BoringFor = null;
        #endregion

        #region Properties
        public ItemEntertainmentModel EntertainmentModel { get { return this.Model as ItemEntertainmentModel; } }
        #endregion

        #region Init
        public ItemEntertainment(ItemModel model)
            : base(model)
        {
            if (!(model is ItemEntertainmentModel))
                throw new ArgumentException("model is not a EntertainmentModel");
        }
        #endregion

        // alpha10 boring items moved out of Actor
        #region Boring items
        public void AddBoringFor(Actor a)
        {
            if (m_BoringFor == null) m_BoringFor = new List<Actor>(1);
            if (m_BoringFor.Contains(a)) return;
            m_BoringFor.Add(a);
        }

        public bool IsBoringFor(Actor a)
        {
            if (m_BoringFor == null) return false;
            return m_BoringFor.Contains(a);
        }
        #endregion

        // alpha10
        #region Pre-saving
        public override void OptimizeBeforeSaving()
        {
            base.OptimizeBeforeSaving();

            // clean up dead actors refs
            // side effect: revived actors will forget about boring items
            if (m_BoringFor != null)
            {
                for (int i = 0; i < m_BoringFor.Count; )
                {
                    if (m_BoringFor[i].IsDead)
                        m_BoringFor.RemoveAt(i);
                    else
                        i++;
                }
                if (m_BoringFor.Count == 0)
                    m_BoringFor = null;
            }
        }
        #endregion
    }
}
