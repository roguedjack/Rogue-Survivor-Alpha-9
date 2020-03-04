using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;


namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// Rat AI : used by Zombie Rat branch.
    /// </summary>
    class RatAI : BaseAI
    {
        #region Fields
        LOSSensor m_LOSSensor;
        SmellSensor m_LivingSmellSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.CORPSES);
            m_LivingSmellSensor = new SmellSensor(Odor.LIVING);
        }

        protected override List<Percept> UpdateSensors(RogueGame game)
        {
            List<Percept> list = m_LOSSensor.Sense(game, m_Actor);
            list.AddRange(m_LivingSmellSensor.Sense(game, m_Actor));
            return list;
        }

        protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
        {
            HashSet<Point> fov = m_LOSSensor.FOV;
            List<Percept> mapPercepts = FilterSameMap(game, percepts);

            //////////////////////////////////////////////////////////////
            // 1 move closer to an enemy, nearest & visible enemies first
            // 2 eat corpses
            // 2 move to highest living scent
            // 3 wander
            //////////////////////////////////////////////////////////////

            // 1 move closer to an enemy, nearest & visible enemies first
            #region
            List<Percept> enemies = FilterEnemies(game, mapPercepts);
            if (enemies != null)
            {
                // try visible enemies first, the closer the best.
                List<Percept> visibleEnemies = Filter(game, enemies, (p) => p.Turn == m_Actor.Location.Map.LocalTime.TurnCounter);
                if (visibleEnemies != null)
                {
                    Percept bestEnemyPercept = null;
                    ActorAction bestBumpAction = null;
                    float closest = int.MaxValue;

                    foreach (Percept enemyP in visibleEnemies)
                    {
                        float distance = game.Rules.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                        if (distance < closest)
                        {
                            ActorAction bumpAction = BehaviorStupidBumpToward(game, enemyP.Location.Position, false, false);
                            if (bumpAction != null)
                            {
                                closest = distance;
                                bestEnemyPercept = enemyP;
                                bestBumpAction = bumpAction;
                            }
                        }
                    }

                    if (bestBumpAction != null)
                    {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                        return bestBumpAction;
                    }
                }

                // then try rest, the closer the best.
                List<Percept> oldEnemies = Filter(game, enemies, (p) => p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter);
                if (oldEnemies != null)
                {
                    Percept bestEnemyPercept = null;
                    ActorAction bestBumpAction = null;
                    float closest = int.MaxValue;

                    foreach (Percept enemyP in oldEnemies)
                    {
                        float distance = game.Rules.GridDistance(m_Actor.Location.Position, enemyP.Location.Position);
                        if (distance < closest)
                        {
                            ActorAction bumpAction = BehaviorStupidBumpToward(game, enemyP.Location.Position, false, false);
                            if (bumpAction != null)
                            {
                                closest = distance;
                                bestEnemyPercept = enemyP;
                                bestBumpAction = bumpAction;
                            }
                        }
                    }

                    if (bestBumpAction != null)
                    {
                        m_Actor.Activity = Activity.CHASING;
                        m_Actor.TargetActor = bestEnemyPercept.Percepted as Actor;
                        return bestBumpAction;
                    }
                }
            }
            #endregion

            // 2 eat corpses.
            List<Percept> corpses = FilterCorpses(game, mapPercepts);
            if (corpses != null)
            {
                ActorAction eatCorpses = BehaviorGoEatCorpse(game, corpses);
                if (eatCorpses != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return eatCorpses;
                }
            }

            // 3 move to highest living scent
            #region
            ActorAction trackLivingAction = BehaviorTrackScent(game, m_LivingSmellSensor.Scents);
            if (trackLivingAction != null)
            {
                m_Actor.Activity = Activity.TRACKING;
                return trackLivingAction;
            }

            #endregion

            // 4 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(game, null);
        }
        #endregion
    }
}
