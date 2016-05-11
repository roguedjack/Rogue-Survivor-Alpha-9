using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    enum DistrictKind
    {
        _FIRST,
        GENERAL = _FIRST,
        RESIDENTIAL,
        SHOPPING,
        GREEN,
        BUSINESS,
        _COUNT
    }

    [Serializable]
    class District
    {
        #region Fields
        Point m_WorldPosition;
        DistrictKind m_Kind;
        string m_Name;
        List<Map> m_Maps = new List<Map>(3);
        Map m_EntryMap;
        Map m_SewersMap;
        Map m_SubwayMap;
        #endregion

        #region Properties
        public Point WorldPosition
        {
            get { return m_WorldPosition; }
        }

        public DistrictKind Kind
        {
            get { return m_Kind; }
        }

        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public IEnumerable<Map> Maps
        {
            get { return m_Maps; }
        }

        public int CountMaps
        {
            get { return m_Maps.Count; }
        }

        public Map EntryMap
        {
            get { return m_EntryMap; }
            set
            {
                if (m_EntryMap != null)
                    RemoveMap(m_EntryMap);
                m_EntryMap = value;
                if (value != null)
                    AddMap(value);
            }
        }

        public Map SewersMap
        {
            get { return m_SewersMap; }
            set
            {
                if (m_SewersMap != null)
                    RemoveMap(m_SewersMap);
                m_SewersMap = value;
                if (value != null)
                    AddMap(value);
            }
        }

        public Map SubwayMap
        {
            get { return m_SubwayMap; }
            set
            {
                if (m_SubwayMap != null)
                    RemoveMap(m_SubwayMap);
                m_SubwayMap = value;
                if (value != null)
                    AddMap(value);
            }
        }

        public bool HasSubway
        {
            get { return m_SubwayMap != null; }
        }

        #endregion

        #region Init
        public District(Point worldPos, DistrictKind kind)
        {
            m_WorldPosition = worldPos;
            m_Kind = kind;
        }
        #endregion

        #region Maps
        protected void AddMap(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            if (m_Maps.Contains(map))
                return;

            map.District = this;
            m_Maps.Add(map);
        }

        /// <summary>
        /// Add a unique map (not Entry, Sewers or Suubway map).
        /// </summary>
        /// <param name="map"></param>
        public void AddUniqueMap(Map map)
        {
            AddMap(map);
        }

        public Map GetMap(int index)
        {
            return m_Maps[index];
        }

        protected void RemoveMap(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            m_Maps.Remove(map);
            map.District = null;
        }
        #endregion

        #region Pre-saving
        public void OptimizeBeforeSaving()
        {
            m_Maps.TrimExcess();

            foreach (Map m in m_Maps)
                m.OptimizeBeforeSaving();
        }
        #endregion

        #region Hashcode
        public override int GetHashCode()
        {
            return m_WorldPosition.GetHashCode();
        }
        #endregion
    }
}
