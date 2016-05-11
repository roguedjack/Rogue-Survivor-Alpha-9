using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    abstract class ActorController
    {
        #region Fields
        protected Actor m_Actor;
        #endregion

        #region Taking control
        public virtual void TakeControl(Actor actor)
        {
            m_Actor = actor;
        }

        public virtual void LeaveControl()
        {
            m_Actor = null;
        }
        #endregion

        #region Updating
        public abstract ActorAction GetAction(RogueGame game);
        #endregion
    }
}
