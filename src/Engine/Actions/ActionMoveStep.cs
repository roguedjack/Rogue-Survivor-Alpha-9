using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionMoveStep : ActorAction
    {
        #region Fields
        Location m_NewLocation;
        #endregion

        #region Properties
        #endregion

        #region Init
        public ActionMoveStep(Actor actor, RogueGame game, Direction direction)
            : base(actor, game)
        {
            m_NewLocation = actor.Location + direction;
        }

        public ActionMoveStep(Actor actor, RogueGame game, Point to)
            : base(actor, game)
        {
            m_NewLocation = new Location(actor.Location.Map, to);
        }
        #endregion

        #region ActorAction implementation
        public override bool IsLegal()
        {
            return m_Game.Rules.IsWalkableFor(m_Actor, m_NewLocation, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoMoveActor(m_Actor, m_NewLocation);
        }
        #endregion
    }
}
