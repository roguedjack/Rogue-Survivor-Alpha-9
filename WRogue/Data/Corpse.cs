using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Corpse
    {
        #region Fields
        Actor m_DeadGuy;
        int m_Turn;
        Point m_Position;
        float m_HitPoints;
        int m_MaxHitPoints;
        float m_Rotation;
        float m_Scale;
        Actor m_DraggedBy;
        #endregion

        #region Properties
        public Actor DeadGuy { get { return m_DeadGuy; } }
        public int Turn { get { return m_Turn; } }
        public Point Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }
        public float HitPoints
        {
            get { return m_HitPoints; }
            set { m_HitPoints = value; }
        }
        public int MaxHitPoints
        {
            get { return m_MaxHitPoints; }
        }
        public float Rotation
        {
            get { return m_Rotation; }
            set { m_Rotation = value; }
        }
        public float Scale
        {
            get { return m_Scale; }
            set
            {
                m_Scale = Math.Max(0, Math.Min(1, value));
            }
        }
        public bool IsDragged
        {
            get { return m_DraggedBy != null && !m_DraggedBy.IsDead; }
        }

        public Actor DraggedBy
        {
            get { return m_DraggedBy; }
            set { m_DraggedBy = value; }
        }
        #endregion

        #region Init
        public Corpse(Actor deadGuy, int hitPoints, int maxHitPoints, int corpseTurn, float rotation, float scale)
        {
            m_DeadGuy = deadGuy;
            m_Turn = corpseTurn;
            m_HitPoints = hitPoints;
            m_MaxHitPoints = maxHitPoints;
            m_Rotation = rotation;
            m_Scale = scale;
            m_DraggedBy = null;
        }
        #endregion
    }
}
