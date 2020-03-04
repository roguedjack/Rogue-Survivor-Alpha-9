using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Items
{
    class ItemTrackerModel : ItemModel
    {
        #region Types
        [Flags]
        public enum TrackingFlags
        {
            /// <summary>
            /// Followers and Leaders can track each other.
            /// </summary>
            FOLLOWER_AND_LEADER = (1 << 0),

            /// <summary>
            /// Can track undeads within close range.
            /// </summary>
            UNDEADS = (1 << 1),

            /// <summary>
            /// Can track all BlackOps faction members on the map.
            /// </summary>
            BLACKOPS_FACTION = (1 << 2),

            /// <summary>
            /// Can track all Police faction members on the map.
            /// </summary>
            POLICE_FACTION = (1 << 3)
        }
        #endregion

        #region Fields
        TrackingFlags m_Tracking;
        int m_MaxBatteries;
        bool m_HasClock;  // alpha10
        #endregion

        #region Properties
        public TrackingFlags Tracking
        {
            get { return m_Tracking; }
        }

        public int MaxBatteries
        {
            get { return m_MaxBatteries; }
        }

        // alpha10
        public bool HasClock
        {
            get { return m_HasClock; }
        }
        #endregion

        #region Init
        public ItemTrackerModel(string aName, string theNames, string imageID, TrackingFlags tracking, int maxBatteries, bool hasClock)
            : base(aName, theNames, imageID)
        {
            m_Tracking = tracking;
            m_MaxBatteries = maxBatteries;
            m_HasClock = hasClock;  // alpha10
            this.DontAutoEquip = true;
        }
        #endregion
    }
}
