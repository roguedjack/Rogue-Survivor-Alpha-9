using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class StateMapObject : MapObject
    {
        #region Fields
        int m_State;
        #endregion

        #region Properties
        public int State
        {
            get { return m_State; }
        }
        #endregion

        #region Init
        public StateMapObject(string name, string hiddenImageID)
            : base(name, hiddenImageID)
        {
        }

        public StateMapObject(string name, string hiddenImageID, Break breakable, Fire burnable, int hitPoints)
            : base(name, hiddenImageID, breakable, burnable, hitPoints)
        {
        }
        #endregion

        #region Changing state
        public virtual void SetState(int newState)
        {
            m_State = newState;
        }
        #endregion
    }
}
