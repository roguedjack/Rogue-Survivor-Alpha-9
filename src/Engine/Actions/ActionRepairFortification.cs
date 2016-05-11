using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionRepairFortification : ActorAction
    {
        #region Fields
        Fortification m_Fort;
        #endregion

        #region Init
        public ActionRepairFortification(Actor actor, RogueGame game, Fortification fort)
            : base(actor, game)
        {
            if (fort == null)
                throw new ArgumentNullException("fort");

            m_Fort = fort;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorRepairFortification(m_Actor, m_Fort, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoRepairFortification(m_Actor, m_Fort);
        }
        #endregion
    }
}
