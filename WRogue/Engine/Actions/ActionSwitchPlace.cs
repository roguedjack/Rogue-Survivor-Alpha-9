using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSwitchPlace : ActorAction
    {
        #region Fields
        readonly Actor m_Target;
        #endregion

        #region Init
        public ActionSwitchPlace(Actor actor, RogueGame game, Actor target)
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
            return m_Game.Rules.CanActorSwitchPlaceWith(m_Actor, m_Target);
        }

        public override void Perform()
        {
            m_Game.DoSwitchPlace(m_Actor, m_Target);
        }
        #endregion
    }
}
