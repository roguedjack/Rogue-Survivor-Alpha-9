using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.AI
{
    [Serializable]
    abstract class Sensor
    {
        #region Sensing
        public abstract List<Percept> Sense(RogueGame game, Actor actor);
        #endregion
    }
}
