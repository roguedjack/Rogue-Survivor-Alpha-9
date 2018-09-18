using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;


namespace djack.RogueSurvivor.Data
{
    [Serializable]
    struct Location
    {
        #region Fields
        Map m_Map;
        Point m_Position;
        #endregion

        #region Properties
        public Map Map
        {
            get { return m_Map; }
        }

        public Point Position
        {
            get { return m_Position; }
        }
        #endregion

        #region Init
        public Location(Map map, Point position)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            m_Map = map;
            m_Position = position;
        }
        #endregion

        #region Operators
        public static bool operator ==(Location lhs, Location rhs)
        {
            return lhs.m_Map == rhs.m_Map && lhs.m_Position == rhs.m_Position;
        }

        public static bool operator !=(Location lhs, Location rhs)
        {
            return !(lhs == rhs);
        }

        public static Location operator +(Location lhs, Direction rhs)
        {
            return new Location(lhs.m_Map, new Point(lhs.m_Position.X + rhs.Vector.X, lhs.m_Position.Y + rhs.Vector.Y));
        }
        #endregion

        #region Hashcode, Equality, ToString
        public override int GetHashCode()
        {
            return m_Map.GetHashCode() ^ m_Position.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if(!(obj is Location))
                return false;

            Location other = (Location)obj;
            return this == other;
        }

        public override string ToString()
        {
            //TODO
            throw new NotImplementedException();
        }
        #endregion
    }
}
