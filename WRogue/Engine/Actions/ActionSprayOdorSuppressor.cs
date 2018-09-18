using System;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
    // alpha10
    class ActionSprayOdorSuppressor : ActorAction
    {
        #region Fields
        readonly ItemSprayScent m_Spray;
        readonly Actor m_SprayOn;
        #endregion

        #region Init
        public ActionSprayOdorSuppressor(Actor actor, RogueGame game, ItemSprayScent spray, Actor sprayOn)
            : base(actor, game)
        {
            if (sprayOn == null)
                throw new ArgumentNullException("target");

            m_Spray = spray;
            m_SprayOn = sprayOn;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorSprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoSprayOdorSuppressor(m_Actor, m_Spray, m_SprayOn);
        }
        #endregion
    }
}
