using System;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionBreak : ActorAction
    {
        #region Fields
        MapObject m_Obj;
        #endregion

        #region Properties
        // alpha10.1 needed by RogueGame to ask player if he really wants to break
        public MapObject MapObject { get { return m_Obj; } }
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

