using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;


namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionRangedAttack : ActorAction
    {
        #region Fields
        Actor m_Target;
        List<Point> m_LoF = new List<Point>();
        FireMode m_Mode;
        #endregion

        #region Init
        public ActionRangedAttack(Actor actor, RogueGame game, Actor target, FireMode mode)
            : base(actor, game)
        {
            if (target == null)
                throw new ArgumentNullException("target");

            m_Target = target;
            m_Mode = mode;
        }

        public ActionRangedAttack(Actor actor, RogueGame game, Actor target)
            : this(actor, game, target, FireMode.DEFAULT)
        {
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            m_LoF.Clear();
            return m_Game.Rules.CanActorFireAt(m_Actor, m_Target, m_LoF, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoRangedAttack(m_Actor, m_Target, m_LoF, m_Mode);
        }
        #endregion
    }
}
