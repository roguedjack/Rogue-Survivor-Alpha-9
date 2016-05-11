using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSay : ActorAction
    {
        #region Fields
        Actor m_Target;
        string m_Text;
        RogueGame.Sayflags m_Flags;
        #endregion

        #region Init
        public ActionSay(Actor actor, RogueGame game, Actor target, string text, RogueGame.Sayflags flags)
            : base(actor, game)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
            m_Text = text;
            m_Flags = flags;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return true;
        }

        public override void Perform()
        {
            m_Game.DoSay(m_Actor, m_Target, m_Text, m_Flags);
        }
        #endregion
    }
}
