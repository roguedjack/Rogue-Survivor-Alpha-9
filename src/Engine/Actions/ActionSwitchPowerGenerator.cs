using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionSwitchPowerGenerator : ActorAction
    {
        #region Fields
        PowerGenerator m_PowGen;
        #endregion

        #region Init
        public ActionSwitchPowerGenerator(Actor actor, RogueGame game, PowerGenerator powGen)
            : base(actor, game)
        {
            if (powGen == null)
                throw new ArgumentNullException("powGen");

            m_PowGen = powGen;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.IsSwitchableFor(m_Actor, m_PowGen, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoSwitchPowerGenerator(m_Actor, m_PowGen);
        }
        #endregion
    }
}
