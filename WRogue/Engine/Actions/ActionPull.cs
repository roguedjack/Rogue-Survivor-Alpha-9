using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionPull : ActorAction
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Direction m_MoveActorDir;
        readonly Point m_MoveActorTo;
        #endregion

        #region Properties
        public Direction MoveActorDirection { get { return m_MoveActorDir; } }
        public Point MoveActorTo { get { return m_MoveActorTo; } }
        #endregion

        #region Init
        public ActionPull(Actor actor, RogueGame game, MapObject pullObj, Direction moveActorDir)
            : base(actor, game)
        {
            if (pullObj == null)
                throw new ArgumentNullException("pushObj");

            m_Object = pullObj;
            m_MoveActorDir = moveActorDir;
            m_MoveActorTo = m_Actor.Location.Position + moveActorDir;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanPullObject(m_Actor, m_Object, m_MoveActorTo, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoPull(m_Actor, m_Object, m_MoveActorTo);
        }
        #endregion

    }
}
