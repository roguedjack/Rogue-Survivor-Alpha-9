using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    enum Activity
    {
        /// <summary>
        /// Doing nothing in particular.
        /// </summary>
        IDLE,

        /// <summary>
        /// Chasing an enemy.
        /// </summary>
        CHASING,

        /// <summary>
        /// Fighting an enemy.
        /// </summary>
        FIGHTING,

        /// <summary>
        /// Following a track.
        /// </summary>
        TRACKING,

        /// <summary>
        /// Fleeing from a danger/enemy.
        /// </summary>
        FLEEING,

        /// <summary>
        /// Following another actor.
        /// </summary>
        FOLLOWING,

        /// <summary>
        /// zzzZZZzzz...
        /// </summary>
        SLEEPING,

        /// <summary>
        /// Following an order.
        /// </summary>
        FOLLOWING_ORDER,

        /// <summary>
        /// Fleeing from a primed explosive.
        /// </summary>
        FLEEING_FROM_EXPLOSIVE
    }
}
