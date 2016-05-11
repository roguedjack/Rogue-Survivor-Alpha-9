using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionChat : ActorAction
    {
        #region Fields
        readonly Actor m_Target;
        #endregion

        #region Properties
        public Actor Target
        {
            get { return m_Target; }
        }
        #endregion

        #region Init
        public ActionChat(Actor actor, RogueGame game, Actor target)
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
            return true;
        }

        public override void Perform()
        {
            m_Game.DoChat(m_Actor, m_Target);
        }
        #endregion
    }
}
