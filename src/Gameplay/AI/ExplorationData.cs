using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    class ExplorationData
    {
        #region Fields
        int m_LocationsQueueSize;
        Queue<Location> m_LocationsQueue;
        int m_ZonesQueueSize;
        Queue<Zone> m_ZonesQueue;
        #endregion

        #region Properties
        #endregion

        #region Init
        public ExplorationData(int locationsToRemember, int zonesToRemember)
        {
            if (locationsToRemember < 1)
                throw new ArgumentOutOfRangeException("locationsQueueSize < 1");
            if (zonesToRemember < 1)
                throw new ArgumentOutOfRangeException("zonesQueueSize < 1");

            m_LocationsQueueSize = locationsToRemember;
            m_LocationsQueue = new Queue<Location>(locationsToRemember);
            m_ZonesQueueSize = zonesToRemember;
            m_ZonesQueue = new Queue<Zone>(zonesToRemember);
        }

        public void Clear()
        {
            m_LocationsQueue.Clear();
            m_ZonesQueue.Clear();
        }
        #endregion

        #region Exploring
        public bool HasExplored(Location loc)
        {
            return m_LocationsQueue.Contains(loc);
        }

        public void AddExplored(Location loc)
        {
            if (m_LocationsQueue.Count >= m_LocationsQueueSize)
                m_LocationsQueue.Dequeue();
            m_LocationsQueue.Enqueue(loc);
        }

        public bool HasExplored(Zone zone)
        {
            return m_ZonesQueue.Contains(zone);
        }

        /// <summary>
        /// Check if has explored all the zones. Null/empty zones are considered as explored.
        /// </summary>
        /// <param name="zones">can be null or empty</param>
        /// <returns></returns>
        public bool HasExplored(List<Zone> zones)
        {
            if (zones == null || zones.Count == 0)
                return true;
            foreach (Zone z in zones)
                if (!m_ZonesQueue.Contains(z))
                    return false;
            return true;
        }

        public void AddExplored(Zone zone)
        {
            if (m_ZonesQueue.Count >= m_ZonesQueueSize)
                m_ZonesQueue.Dequeue();
            m_ZonesQueue.Enqueue(zone);
        }
        #endregion

        #region Updating
        public void Update(Location location)
        {
            // location.
            //**disabled** if (!HasExplored(location))
                AddExplored(location);

            // zones.
            List<Zone> zones = location.Map.GetZonesAt(location.Position.X, location.Position.Y);
            if (zones != null && zones.Count > 0)
            {
                foreach (Zone z in zones)
                {
                    if (HasExplored(z))
                        continue;
                    AddExplored(z);
                }
            }
        }
        #endregion
    }
}
