using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.MapObjects
{
    [Serializable]
    class DoorWindow : StateMapObject
    {
        #region Constants
        public const int BASE_HITPOINTS = 40;
        #endregion

        #region Fields
        string m_ClosedImageID;
        string m_OpenImageID;
        string m_BrokenImageID;
        bool m_IsWindow;
        int m_BarricadePoints;
        #endregion

        #region Door states
        public const int STATE_CLOSED = 1;
        public const int STATE_OPEN = 2;
        public const int STATE_BROKEN = 3;
        #endregion

        #region Properties
        public bool IsOpen
        {
            get { return this.State == STATE_OPEN; }
        }

        public bool IsClosed
        {
            get { return this.State == STATE_CLOSED; }
        }

        public bool IsBroken
        {
            get { return this.State == STATE_BROKEN; }
        }

        public override bool IsTransparent
        {
            get
            {
                if (m_BarricadePoints > 0)
                    return false;
                if (this.State == STATE_OPEN)
                {
                    if (this.FireState == Fire.ONFIRE)
                        return false;
                    return true;
                }

                return base.IsTransparent;
            }
        }

        public bool IsWindow
        {
            get { return m_IsWindow; }
            set { m_IsWindow = value; }
        }

        public int BarricadePoints
        {
            get { return m_BarricadePoints; }
            set 
            { 
                // handle barricading-debarricading
                if (value > 0 && m_BarricadePoints <= 0)
                {
                    // barricading
                    --this.JumpLevel;
                    this.IsWalkable = false;
                }
                else if(value <= 0 && m_BarricadePoints > 0)
                {
                    // de-barricading, restore state effects.
                    SetState(this.State);
                }

                m_BarricadePoints = value;
                if (m_BarricadePoints < 0) m_BarricadePoints = 0;
            }
        }

        public bool IsBarricaded
        {
            get { return m_BarricadePoints > 0; }
        }
        #endregion

        #region Init
        public DoorWindow(string name, string closedImageID, string openImageID, string brokenImageID, int hitPoints)
            : base(name, closedImageID, Break.BREAKABLE, Fire.BURNABLE, hitPoints)
        {
            m_ClosedImageID = closedImageID;
            m_OpenImageID = openImageID;
            m_BrokenImageID = brokenImageID;
            m_BarricadePoints = 0;

            SetState(STATE_CLOSED);
        }
        #endregion

        #region StateMapObject
        public override void SetState(int newState)
        {
            switch (newState)
            {
                case STATE_OPEN:
                    this.ImageID = m_OpenImageID;
                    this.IsWalkable = true;
                    break;

                case STATE_CLOSED:
                    this.ImageID = m_ClosedImageID;
                    this.IsWalkable = false;
                    break;

                case STATE_BROKEN:
                    this.ImageID = m_BrokenImageID;
                    this.BreakState = Break.BROKEN;
                    this.HitPoints = 0;
                    m_BarricadePoints = 0;
                    this.IsWalkable = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("newState unhandled");
            }

            base.SetState(newState);
        }
        #endregion
    }
}
