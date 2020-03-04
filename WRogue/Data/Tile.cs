using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Tile
    {
        #region Flags
        [Flags]
        enum Flags
        {
            NONE = 0,
            IS_INSIDE = (1 << 0),
            IS_IN_VIEW = (1 << 1),
            IS_VISITED = (1 << 2)
        }
        #endregion

        #region Fields
        int m_ModelID;
        Flags m_Flags;
        List<string> m_Decorations = null;
        #endregion

        #region Properties
        public TileModel Model
        {
            get { return Models.Tiles[m_ModelID]; }
            set { m_ModelID = value.ID; }
        }

        public bool IsInside
        {
            get { return (m_Flags & Flags.IS_INSIDE) != 0; }
            set { if (value) m_Flags |= Flags.IS_INSIDE; else m_Flags &= ~Flags.IS_INSIDE; }
        }

        public bool IsInView
        {
            get { return (m_Flags & Flags.IS_IN_VIEW) != 0; }
            set { if (value) m_Flags |= Flags.IS_IN_VIEW; else m_Flags &= ~Flags.IS_IN_VIEW; }
        }

        public bool IsVisited
        {
            get { return (m_Flags & Flags.IS_VISITED) != 0; }
            set { if (value) m_Flags |= Flags.IS_VISITED; else m_Flags &= ~Flags.IS_VISITED; }
        }

        public bool HasDecorations
        {
            get { return m_Decorations != null; }
        }

        /// <summary>
        /// Get tile decorations enumeration, null if no decorations.
        /// </summary>
        public IEnumerable<string> Decorations
        {
            get { return m_Decorations; }
        }
        #endregion

        #region Init
        public Tile(TileModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            m_ModelID = model.ID;
        }
        #endregion

        #region Decorations
        public void AddDecoration(string imageID)
        {
            if (m_Decorations == null)
                m_Decorations = new List<string>(1);
            if (m_Decorations.Contains(imageID))
                return;
            m_Decorations.Add(imageID);
        }

        public bool HasDecoration(string imageID)
        {
            if (m_Decorations == null)
                return false;
            return m_Decorations.Contains(imageID);
        }

        public void RemoveAllDecorations()
        {
            if (m_Decorations != null)
                m_Decorations.Clear();
            m_Decorations = null;
        }

        public void RemoveDecoration(string imageID)
        {
            if (m_Decorations == null) return;
            if (m_Decorations.Remove(imageID))
            {
                if (m_Decorations.Count == 0)
                    m_Decorations = null;
            }
        }
        #endregion

        #region Pre-saving
        public void OptimizeBeforeSaving()
        {
            if (m_Decorations != null)
                m_Decorations.TrimExcess();
        }
        #endregion

    }
}
