using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    abstract class TimedTask
    {
        #region Properties
        public int TurnsLeft
        {
            get;
            set;
        }

        public bool IsCompleted
        {
            get { return this.TurnsLeft <= 0; }
        }
        #endregion

        #region Init
        protected TimedTask(int turnsLeft)
        {
            this.TurnsLeft = turnsLeft;
        }
        #endregion

        #region Process
        public void Tick(Map m)
        {
            if (--this.TurnsLeft <= 0)
                Trigger(m);
        }

        public abstract void Trigger(Map m);
        #endregion
    }
}
