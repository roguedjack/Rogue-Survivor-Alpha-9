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
    class LOSSensor : Sensor
    {
        #region Sensor
        public override List<Percept> Sense(RogueGame game, Actor actor)
        {
            // compute FOV
            HashSet<Point> visibleSet = LOS.ComputeFOVFor(game.Rules, actor);

            // compute percepts of other actors.
            List<Percept> list = new List<Percept>();

            // roughly estimate time for two sensing methods.
            int searchFovMethodTime = actor.ViewRange * actor.ViewRange;
            int searchActorsListMethodTime = actor.Location.Map.CountActors;

            // choose method which seems less costly in time.
            if (searchFovMethodTime < searchActorsListMethodTime)
            {
                // FOV check : bad when few actors, good when many actors.
                foreach (Point p in visibleSet)
                {
                    Actor other = actor.Location.Map.GetActorAt(p.X, p.Y);
                    if (other != null && other != actor)
                        list.Add(new Percept(other, game.Session.WorldTime.TurnCounter, other.Location.Position));
                }
            }
            else
            {
                // Actors list check : good when few actors, bad when many actors.
                int maxRange = actor.ViewRange;
                foreach (Actor other in actor.Location.Map.Actors)
                {
                    if (other == actor)
                        continue;

                    if (game.Rules.LOSDistance(actor.Location.Position, other.Location.Position) > maxRange)
                        continue;

                    if (visibleSet.Contains(other.Location.Position))
                        list.Add(new Percept(other, game.Session.WorldTime.TurnCounter, other.Location.Position));
                }
            }

            // done.
            return list;
        }
        #endregion
    }
}
