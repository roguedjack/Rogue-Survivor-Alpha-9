using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;

namespace djack.RogueSurvivor.Gameplay.AI
{
    /// <summary>
    /// alpa10 this unused for now
    /// </summary>
    [Serializable]
    class FeralDogAI : BaseAI
    {
        #region Constants
        const int FOLLOW_NPCLEADER_MAXDIST = 1;
        const int FOLLOW_PLAYERLEADER_MAXDIST = 1;
        const int RUN_TO_TARGET_DISTANCE = 3;  // dogs run to their target when close enough

        static string[] FIGHT_EMOTES = 
        {
            "waf",            // flee
            "waf!?",         // trapped
            "GRRRRR WAF WAF"  // fight
        };   
        #endregion

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
            // 1 defend our leader.
            // 2 attack or flee enemies.
            // 3 go eat food on floor if almost hungry
            // 4 go eat corpses if hungry
            // 5 rest or sleep
            // 6 follow leader
            // 7 wander
            /////////////////////////////////////////////////////////////

            // 1 defend our leader
            if (m_Actor.HasLeader)
            {
                Actor target = m_Actor.Leader.TargetActor;
                if(target != null && target.Location.Map == m_Actor.Location.Map)
                {
                    // emote: bark
                    game.DoSay(m_Actor, target, "GRRRRRR WAF WAF", RogueGame.Sayflags.IS_FREE_ACTION | RogueGame.Sayflags.IS_DANGER);
                    // charge.
                    ActorAction chargeEnemy = BehaviorStupidBumpToward(game, target.Location.Position, true, false);
                    if (chargeEnemy != null)
                    {
                        RunToIfCloseTo(game, target.Location.Position, RUN_TO_TARGET_DISTANCE);
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = target;
                        return chargeEnemy;
                    }
                }
            }

            List<Percept> enemies = FilterEnemies(game, mapPercepts);
            bool isLeaderVisible = m_Actor.HasLeader && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
            bool isLeaderFighting = m_Actor.HasLeader && IsAdjacentToEnemy(game, m_Actor.Leader);

            // 2 attack or flee enemies.
            #region
            if (enemies != null)
            {
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP; // alpha10
                ActorAction ff = BehaviorFightOrFlee(game, enemies, isLeaderVisible, isLeaderFighting, Directives.Courage, FIGHT_EMOTES, allowedChargeActions);
                if (ff != null)
                {
                    // run to (or away if fleeing) if close.
                    if (m_Actor.TargetActor != null)
                        RunToIfCloseTo(game, m_Actor.TargetActor.Location.Position, RUN_TO_TARGET_DISTANCE);
                    return ff;
                }
            }
            #endregion

            // 3 go eat food on floor if almost hungry
            #region
            if (game.IsAlmostHungry(m_Actor))
            {
                List<Percept> itemsStack = FilterStacks(game, mapPercepts);
                if (itemsStack != null)
                {
                    ActorAction eatFood = BehaviorGoEatFoodOnGround(game, itemsStack);
                    if (eatFood != null)
                    {
                        RunIfPossible(game.Rules);
                        m_Actor.Activity = Activity.IDLE;
                        return eatFood;
                    }
                }
            }
            #endregion

            // 4 go eat corpses if hungry
            #region
            if (game.Rules.IsActorHungry(m_Actor))
            {
                List<Percept> corpses = FilterCorpses(game, mapPercepts);
                if (corpses != null)
                {
                    ActorAction eatCorpses = BehaviorGoEatCorpse(game, corpses);
                    if (eatCorpses != null)
                    {
                        RunIfPossible(game.Rules);
                        m_Actor.Activity = Activity.IDLE;
                        return eatCorpses;
                    }
                }
            }
            #endregion

            // 5 rest or sleep
            #region
            if (game.Rules.IsActorTired(m_Actor))
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor, game);
            }
            if (game.Rules.IsActorSleepy(m_Actor))
            {
                m_Actor.Activity = Activity.SLEEPING;
                return new ActionSleep(m_Actor, game);
            }
            #endregion

            // 6 follow leader
            #region
            if (m_Actor.HasLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
                ActorAction followAction = BehaviorFollowActor(game, m_Actor.Leader, lastKnownLeaderPosition, isLeaderVisible, maxDist);
                if (followAction != null)
                {
                    m_Actor.IsRunning = false;
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }
            #endregion

            // 7 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(game, null);
        }

        private ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, bool isLeaderVisible, bool isLeaderFighting, ActorCourage courage, string[] fIGHT_EMOTES)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Dogs specifics
        protected void RunToIfCloseTo(RogueGame game, Point pos, int closeDistance)
        {
            if (game.Rules.GridDistance(m_Actor.Location.Position, pos) <= closeDistance)
            {
                m_Actor.IsRunning = game.Rules.CanActorRun(m_Actor);
            }
            else
            {
                m_Actor.IsRunning = false;
            }
        }
        #endregion
    }
}
