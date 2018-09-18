using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionThrowGrenade : ActorAction
    {
        #region Fields
        Point m_ThrowPos;
        #endregion

        #region Init
        public ActionThrowGrenade(Actor actor, RogueGame game, Point throwPos)
            : base(actor, game)
        {
            m_ThrowPos = throwPos;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorThrowTo(m_Actor, m_ThrowPos, null, out m_FailReason);
        }

        public override void Perform()
        {
            Item grenade = m_Actor.GetEquippedWeapon();

            if (grenade is ItemPrimedExplosive)
                m_Game.DoThrowGrenadePrimed(m_Actor, m_ThrowPos);
            else
                m_Game.DoThrowGrenadeUnprimed(m_Actor, m_ThrowPos);
        }
        #endregion
    }
}
