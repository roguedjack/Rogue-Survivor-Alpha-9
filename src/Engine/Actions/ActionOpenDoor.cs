using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionOpenDoor : ActorAction
    {
        #region Fields
        DoorWindow m_Door;
        #endregion

        #region Init
        public ActionOpenDoor(Actor actor, RogueGame game, DoorWindow door)
            : base(actor, game)
        {
            if (door == null)
                throw new ArgumentNullException("door");

            m_Door = door;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.IsOpenableFor(m_Actor, m_Door, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoOpenDoor(m_Actor, m_Door);
        }
        #endregion
    }
}
