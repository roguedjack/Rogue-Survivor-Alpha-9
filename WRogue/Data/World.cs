using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class World
    {
        #region Fields
        District[,] m_DistrictsGrid;
        int m_Size;
        Weather m_Weather;
        int m_NextWeatherCheckTurn;  // alpha10
        #endregion

        #region Properties
        public int Size
        {
            get { return m_Size; }
        }

        public District this[int x, int y]
        {
            get 
            {
                if (x < 0 || x >= m_Size)
                    throw new ArgumentOutOfRangeException("x");
                if (y < 0 || y >= m_Size)
                    throw new ArgumentOutOfRangeException("y");
                return m_DistrictsGrid[x, y]; 
            }
            set
            {
                if (x < 0 || x >= m_Size)
                    throw new ArgumentOutOfRangeException("x");
                if (y < 0 || y >= m_Size)
                    throw new ArgumentOutOfRangeException("y");
                m_DistrictsGrid[x, y] = value;
            }
        }

        public Weather Weather
        {
            get { return m_Weather; }
            set { m_Weather = value; }
        }

        public int NextWeatherCheckTurn
        {
            get { return m_NextWeatherCheckTurn; }
            set { m_NextWeatherCheckTurn = value; }
        }
        #endregion

        #region Init
        public World(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException("size <=0");

            m_DistrictsGrid = new District[size, size];
            m_Size = size;
            m_Weather = Weather.CLEAR;
            m_NextWeatherCheckTurn = 0;
        }
        #endregion

        #region Coordinates
        /// <summary>
        /// Trim district coordinates to world bounds.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TrimToBounds(ref int x, ref int y)
        {
            if (x < 0) x = 0;
            if (x >= m_Size) x = m_Size - 1;
            if (y < 0) y = 0;
            if (y >= m_Size) y = m_Size - 1;
        }

        /// <summary>
        /// District coordinates to string.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>[A-Z][0-9]</returns>
        public static string CoordToString(int x, int y)
        {
            return String.Format("{0}{1}", (char)('A'+x), y);
        }
        #endregion

        #region Pre-saving
        public void OptimizeBeforeSaving()
        {
            for (int x = 0; x < m_Size; x++)
                for (int y = 0; y < m_Size; y++)
                    m_DistrictsGrid[x, y].OptimizeBeforeSaving();
        }
        #endregion
    }
}
