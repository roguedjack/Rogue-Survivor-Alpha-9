using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionEatCorpse : ActorAction
    {
        #region Fields
        readonly Corpse m_Target;
        #endregion

        #region Init
        public ActionEatCorpse(Actor actor, RogueGame game, Corpse target)
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
            return m_Game.Rules.CanActorEatCorpse(m_Actor, m_Target, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoEatCorpse(m_Actor, m_Target);
        }
        #endregion
    }
}
