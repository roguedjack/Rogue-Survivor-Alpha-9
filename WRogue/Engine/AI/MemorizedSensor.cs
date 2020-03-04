using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.AI
{
    [Serializable]
    /// <summary>
    /// Sensor with a memory of past percepts.
    /// Implemented with the decorator pattern.
    /// </summary>
    class MemorizedSensor : Sensor
    {
        #region Fields
        Sensor m_Sensor;
        List<Percept> m_Percepts = new List<Percept>();
        int m_Persistance;
        #endregion

        #region Properties
        public Sensor Sensor
        {
            get { return m_Sensor; }
        }
        #endregion

        #region Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="noMemorySensor">sensor with no memory to get new percepts from</param>
        /// <param name="persistance">number of turns to keep old percepts in memory; 0 is equivalent to having no memory</param>
        public MemorizedSensor(Sensor noMemorySensor, int persistance)
        {
            if (noMemorySensor == null)
                throw new ArgumentNullException("decoratedSensor");

            m_Sensor = noMemorySensor;
            m_Persistance = persistance;
        }
        #endregion

        #region Clear memory
        public void Clear()
        {
            m_Percepts.Clear();
        }
        #endregion

        #region Sensor
        public override List<Percept> Sense(RogueGame game, Actor actor)
        {
            // Forget aged percepts
            for(int i = 0; i < m_Percepts.Count; )
            {
                if (m_Percepts[i].GetAge(actor.Location.Map.LocalTime.TurnCounter) > m_Persistance)
                    m_Percepts.RemoveAt(i);
                else
                    ++i;
            }

            // Forget dead actors or actors that are not anymore in the same map.
            for (int i = 0; i < m_Percepts.Count; )
            {
                Percept p = m_Percepts[i];
                Actor a = p.Percepted as Actor;
                if (a != null && (a.IsDead || a.Location.Map != actor.Location.Map))
                    m_Percepts.RemoveAt(i);
                else
                    ++i;
            }

            // Get new percepts.
            List<Percept> turnPercepts = m_Sensor.Sense(game, actor);

            // Update old percepts and add new ones.
            List<Percept> perceptsToAdd = null;
            foreach (Percept turnP in turnPercepts)
            {
                // Update?
                bool hasUpdated = false;
                foreach (Percept oldP in m_Percepts)
                {
                    if (oldP.Percepted == turnP.Percepted)
                    {
                        oldP.Location = turnP.Location;
                        oldP.Turn = turnP.Turn;
                        hasUpdated = true;
                        break;
                    }
                }

                // New one?
                if (!hasUpdated)
                {
                    if (perceptsToAdd == null)
                        perceptsToAdd = new List<Percept>(turnPercepts.Count);
                    perceptsToAdd.Add(turnP);
                }
            }
            if (perceptsToAdd != null)
            {
                foreach (Percept addP in perceptsToAdd)
                    m_Percepts.Add(addP);
            }

            // Return updated percepts.
            return m_Percepts;
        }
        #endregion
    }
}
