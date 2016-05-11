using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTrade : ActorAction
    {
        #region Fields
        readonly Actor m_Target;
        #endregion

        #region Init
        public ActionTrade(Actor actor, RogueGame game, Actor target)
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
            return m_Game.Rules.CanActorInitiateTradeWith(m_Actor, m_Target);
        }

        public override void Perform()
        {
            m_Game.DoTrade(m_Actor, m_Target);
        }
        #endregion
    }
}
