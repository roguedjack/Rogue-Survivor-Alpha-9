using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.AI
{
    [Serializable]
    class Percept
    {
        #region Fields
        int m_Turn;
        Location m_Location;
        Object m_Percepted;
        #endregion

        #region Properties
        public int Turn
        {
            get { return m_Turn; }
            set { m_Turn = value; }
        }

        public Object Percepted
        {
            get { return m_Percepted; }
        }

        public Location Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }
        #endregion

        #region Init
        public Percept(Object percepted, int turn, Location location)
        {
            if (percepted == null)
                throw new ArgumentNullException("percepted");

            m_Percepted = percepted;
            m_Turn = turn;
            m_Location = location;
        }
        #endregion

        #region Age
        public int GetAge(int currentGameTurn)
        {
            return Math.Max(0, currentGameTurn - m_Turn);
        }
        #endregion
    }
}
