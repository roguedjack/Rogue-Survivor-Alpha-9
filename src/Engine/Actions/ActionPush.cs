using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionPush : ActorAction
    {
        #region Fields
        readonly MapObject m_Object;
        readonly Direction m_Direction;
        readonly Point m_To;
        #endregion

        #region Properties
        public Direction Direction { get { return m_Direction; } }
        public Point To { get { return m_To; } }
        #endregion

        #region Init
        public ActionPush(Actor actor, RogueGame game, MapObject pushObj, Direction pushDir)
            : base(actor, game)
        {
            if (pushObj == null)
                throw new ArgumentNullException("pushObj");

            m_Object = pushObj;
            m_Direction = pushDir;
            m_To = pushObj.Location.Position + pushDir;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorPush(m_Actor, m_Object) && m_Game.Rules.CanPushObjectTo(m_Object, m_To, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoPush(m_Actor, m_Object, m_To);
        }
        #endregion

    }
}
