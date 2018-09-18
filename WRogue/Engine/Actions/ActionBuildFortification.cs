using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBuildFortification : ActorAction
    {
        #region Fields
        Point m_BuildPos;
        bool m_IsLarge;
        #endregion

        #region Init
        public ActionBuildFortification(Actor actor, RogueGame game, Point buildPos, bool isLarge)
            : base(actor, game)
        {
            m_BuildPos = buildPos;
            m_IsLarge = isLarge;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorBuildFortification(m_Actor, m_BuildPos, m_IsLarge);
        }

        public override void Perform()
        {
            m_Game.DoBuildFortification(m_Actor, m_BuildPos, m_IsLarge);
        }
        #endregion
    }
}
