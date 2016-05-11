using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBreak : ActorAction
    {
        #region Fields
        MapObject m_Obj;
        #endregion

        #region Init
        public ActionBreak(Actor actor, RogueGame game, MapObject obj)
            : base(actor, game)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            m_Obj = obj;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.IsBreakableFor(m_Actor, m_Obj, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoBreak(m_Actor, m_Obj);
        }
        #endregion
    }
}

