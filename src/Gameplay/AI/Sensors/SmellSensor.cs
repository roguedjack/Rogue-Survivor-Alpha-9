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
        Odor m_OdorToSmell;
        List<Percept> m_List;
        #endregion

        #region Properties
        public List<Percept> Scents
        {
            get { return m_List; }
        }
        #endregion


        #region Init
        public SmellSensor(Odor odorToSmell)
        {
            m_OdorToSmell = odorToSmell;
            m_List = new List<Percept>(9);
        }
        #endregion

        #region Sensor
        public override List<Percept> Sense(RogueGame game, Actor actor)
        {
            m_List.Clear();
            int minStrength = game.Rules.ActorSmellThreshold(actor);

            // smell adjacent & self.
            int xmin = actor.Location.Position.X - 1;
            int xmax = actor.Location.Position.X + 1;
            int ymin = actor.Location.Position.Y - 1;
            int ymax = actor.Location.Position.Y + 1;
            actor.Location.Map.TrimToBounds(ref xmin, ref ymin);
            actor.Location.Map.TrimToBounds(ref xmax, ref ymax);

            int turnCounter = actor.Location.Map.LocalTime.TurnCounter;
            Point pt = new Point();
            for (int x = xmin; x <= xmax; x++)
            {
                pt.X = x;
                for (int y = ymin; y <= ymax; y++)
                {
                    pt.Y = y;
                    int strength = actor.Location.Map.GetScentByOdorAt(m_OdorToSmell, pt);
                    if (strength >= 0 && strength >= minStrength)
                        m_List.Add(new Percept(new AIScent(m_OdorToSmell, strength), turnCounter, new Location(actor.Location.Map, pt)));
                }
            }

            return m_List;

#if false
            old algo. smell in range.

            // Smell cells withing range.
            int xmin = actor.Location.Position.X - actor.Sheet.BaseSmellRange;
            int xmax = actor.Location.Position.X + actor.Sheet.BaseSmellRange;
            int ymin = actor.Location.Position.Y - actor.Sheet.BaseSmellRange;
            int ymax = actor.Location.Position.Y + actor.Sheet.BaseSmellRange;
            actor.Location.Map.TrimToBounds(ref xmin, ref ymin);
            actor.Location.Map.TrimToBounds(ref xmax, ref ymax);

            List<Percept> list = new List<Percept>(5);

            int turnCounter = actor.Location.Map.LocalTime.TurnCounter;
            Point pt = new Point();
            for (int x = xmin; x <= xmax; x++)
            {
                pt.X = x;
                for (int y = ymin; y <= ymax; y++)
                {
                    pt.Y = y;
                    int strength = actor.Location.Map.GetScentByOdorAt(m_OdorToSmell, pt);
                    if (strength >= m_MinStrength)
                        list.Add(new Percept(new AIScent(m_OdorToSmell, strength), turnCounter, new Location(actor.Location.Map, pt)));
                }
            }

            return list;
#endif
        }
        #endregion
    }
}
