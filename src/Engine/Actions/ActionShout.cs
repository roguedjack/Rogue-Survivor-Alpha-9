using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionShout : ActorAction
    {
        #region Fields
        string m_Text;
        #endregion

        #region Init
        public ActionShout(Actor actor, RogueGame game)
            : this(actor, game, null)
        {
        }

        public ActionShout(Actor actor, RogueGame game, string text)
            : base(actor, game)
        {
            m_Text = text;
        }
        #endregion

        #region Implementation
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorShout(m_Actor, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoShout(m_Actor, m_Text);
        }
        #endregion
    }
}
