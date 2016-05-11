using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSleep : ActorAction
    {
        #region Init
        public ActionSleep(Actor actor, RogueGame game)
            : base(actor, game)
        {
        }
        #endregion

        #region Implementation
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorSleep(m_Actor, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoStartSleeping(m_Actor);
        }
        #endregion
    }
}
