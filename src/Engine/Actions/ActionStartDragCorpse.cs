using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionStartDragCorpse : ActorAction
    {
        #region Fields
        readonly Corpse m_Target;
        #endregion

        #region Init
        public ActionStartDragCorpse(Actor actor, RogueGame game, Corpse target)
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
            return m_Game.Rules.CanActorStartDragCorpse(m_Actor, m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoStartDragCorpse(m_Actor, m_Target);
        }
        #endregion
    }
}
