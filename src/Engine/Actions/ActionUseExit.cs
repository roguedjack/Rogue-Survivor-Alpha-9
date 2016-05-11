using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionUseExit : ActorAction
    {
        #region Fields
        Point m_ExitPoint;
        #endregion

        #region Init
        public ActionUseExit(Actor actor, Point exitPoint, RogueGame game)
            : base(actor, game)
        {
            m_ExitPoint = exitPoint;
        }
        #endregion

        #region ActorAction implementation
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorUseExit(m_Actor, m_ExitPoint, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoUseExit(m_Actor, m_ExitPoint);
        }
        #endregion
    }
}
