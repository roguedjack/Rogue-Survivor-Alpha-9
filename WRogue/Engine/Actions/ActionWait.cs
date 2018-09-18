using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionWait : ActorAction
    {
        #region Init
        public ActionWait(Actor actor, RogueGame game)
            : base(actor, game)
        {
        }
        #endregion

        #region Implementation
        public override bool IsLegal()
        {
            return true;
        }

        public override void Perform()
        {
            m_Game.DoWait(m_Actor);
        }
        #endregion
    }
}
