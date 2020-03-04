using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    /// <summary>
    /// "Marker" class to tell the game the actor is controller by the human player. The class does nothing as the game itself handles all the work.
    /// </summary>
    class PlayerController : ActorController
    {

        public override ActorAction GetAction(RogueGame game)
        {
            // shouldn't get here
            throw new InvalidOperationException("do not call PlayerController.GetAction()");
        }
    }
}
