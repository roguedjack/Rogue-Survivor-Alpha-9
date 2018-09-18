using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Actions
{
    class ActionGetFromContainer : ActorAction
    {
        #region Fields
        Point m_Position;
        #endregion

        #region Properties
        /// <summary>
        /// Gets item that will be taken : top item from container position.
        /// </summary>
        public Item Item
        {
            get
            {
                Map map = m_Actor.Location.Map;
                return map.GetItemsAt(m_Position).TopItem;
            }
        }
        #endregion

        #region Init
        public ActionGetFromContainer(Actor actor, RogueGame game, Point position)
            : base(actor, game)
        {
            m_Position = position;
        }
        #endregion

        #region ActorAction
        public override bool IsLegal()
        {
            return m_Game.Rules.CanActorGetItemFromContainer(m_Actor, m_Position, out m_FailReason);
        }

        public override void Perform()
        {
            m_Game.DoTakeFromContainer(m_Actor, m_Position);
        }
        #endregion
    }
}
