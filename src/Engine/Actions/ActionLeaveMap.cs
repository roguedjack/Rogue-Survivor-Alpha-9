using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionLeaveMap : ActorAction
    {
        #region Fields
        Point m_ExitPoint;
        #endregion

        #region Properties
        public Point ExitPoint
        {
            get { return m_ExitPoint; }
        }
        #endregion

        #region Init
        public ActionLeaveMap(Actor actor, RogueGame game, Point exitPoint)
            : base(actor, game)
        {
            m_ExitPoint = exitPoint;
        }
        #endregion

        #region ActorAction implementation
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorLeaveMap(m_Actor, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoLeaveMap(m_Actor, m_ExitPoint, true);
        }
        #endregion
    }
}
