using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Data
{
    abstract class ActorAction
    {
        #region Fields
        protected readonly RogueGame m_Game;
        protected readonly Actor m_Actor;
        protected string m_FailReason;
        #endregion

        #region Properties
        public string FailReason
        {
            get { return m_FailReason; }
            set { m_FailReason = value; }
        }
        #endregion

        #region Init
        protected ActorAction(Actor actor, RogueGame game)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (game == null)
                throw new ArgumentNullException("game");

            m_Actor = actor;
            m_Game = game;
        }
        #endregion

        #region Legality & Performing
        public abstract bool IsLegal();
        public abstract void Perform();
        #endregion
    }
}
