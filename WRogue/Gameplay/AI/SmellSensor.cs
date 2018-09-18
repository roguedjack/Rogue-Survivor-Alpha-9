using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.AI;

namespace djack.RogueSurvivor.Gameplay.AI.Sensors
{
    [Serializable]
    class SmellSensor : Sensor
    {
        #region Types
        [Serializable]
        public class AIScent
        {
            public Odor Odor { get; private set; }
            public int Strength { get; private set; }

            public AIScent(Odor odor, int strength)
            {
                this.Odor = odor;
                this.Strength = strength;
            }
        }
        #endregion

        #region Fields
        readonly Odor m_OdorToSmell;
        readonly int m_MinStrength;
        #endregion

        #region Init
        public SmellSensor(Odor odorToSmell, int minimumStrength)
        {
            m_OdorToSmell = odorToSmell;
            m_MinStrength = minimumStrength;
        }
        #endregion

        #region Sensor
        public override List<Percept> Sense(RogueGame game, Actor actor)
        {
            // Smell adjacent cells.
            List<Percept> list = new List<Percept>();

            foreach (Direction d in Direction.COMPASS)
            {
                Location next = actor.Location + d;
                if (!actor.Location.Map.IsInBounds(next.Position))
                    continue;

                int strength = next.Map.GetScentByOdorAt(m_OdorToSmell, next.Position);
                if (strength >= m_MinStrength)
                    list.Add(new Percept(new AIScent(m_OdorToSmell, strength), game.Session.WorldTime.TurnCounter, next.Position));
            }

            // done.
            return list;
        }
        #endregion
    }
}
