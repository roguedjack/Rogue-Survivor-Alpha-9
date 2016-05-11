using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionMeleeAttack : ActorAction
    {
        #region Fields
        readonly Actor m_Target;
        #endregion

        #region Init
        public ActionMeleeAttack(Actor actor, RogueGame game, Actor target)
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
            return true;    // handled before in rules
        }

        public override void Perform()
        {
            m_Game.DoMeleeAttack(m_Actor, m_Target);
        }
        #endregion
    }
}
