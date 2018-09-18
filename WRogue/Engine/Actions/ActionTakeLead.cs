using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTakeLead : ActorAction
    {
        #region Fields
        readonly Actor m_Target;
        #endregion

        #region Init
        public ActionTakeLead(Actor actor, RogueGame game, Actor target)
            : base(actor, game)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorTakeLead(m_Actor,m_Target);
        }

        public override void Perform()
        {
            // alpha10.1 steal lead vs take lead
            if (m_Target.HasLeader)
                m_Game.DoStealLead(m_Actor, m_Target);
            else
                m_Game.DoTakeLead(m_Actor, m_Target);
        }
        #endregion
    }
}
