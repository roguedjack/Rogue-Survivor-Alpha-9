using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionTakeItem : ActorAction
    {
        #region Fields
        Point m_Position;
        Item m_Item;
        #endregion

        #region Init
        public ActionTakeItem(Actor actor, RogueGame game, Point position, Item it)
            : base(actor, game)
        {
            if (it == null)
                throw new ArgumentNullException("item");

            m_Position = position;
            m_Item = it;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorGetItem(m_Actor, m_Item, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoTakeItem(m_Actor, m_Position, m_Item);
        }
        #endregion
    }
}