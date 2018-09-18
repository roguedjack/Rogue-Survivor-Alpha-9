using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;   // Point

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// CHAR Guard AI.
    /// </summary>
    class CHARGuardAI : OrderableAI
    {
        #region Constants
        const int LOS_MEMORY = 10;

        static string[] FIGHT_EMOTES = 
        {
            "Go away",
            "Damn it I'm trapped!",
            "Hey"
        };                              
        #endregion

        #region Fields
        LOSSensor m_LOSSensor;
        MemorizedSensor m_MemorizedSensor;
        #endregion

        #region BaseAI
        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS);
            m_MemorizedSensor = new MemorizedSensor(m_LOSSensor, LOS_MEMORY);
        }

        public override void TakeControl(Actor actor)
        {
            base.TakeControl(actor);
        }

        protected override List<Percept> UpdateSensors(RogueGame game)
        {
            return m_MemorizedSensor.Sense(game, m_Actor);
        }

        protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
        {
            List<Percept> mapPercepts = FilterSameMap(game, percepts);

            // alpha10
            // don't run by default.
            m_Actor.IsRunning = false;

            // 0. Equip best item
            ActorAction bestEquip = BehaviorEquipBestItems(game, true, true);
            if (bestEquip != null)
            {
                return bestEquip;
            }
            // end alpha10

            // 1. Follow order
            #region
            if (this.Order != null)
            {
                ActorAction orderAction = ExecuteOrder(game, this.Order, mapPercepts, null);
                if (orderAction == null)
                    SetOrder(null);
                else
                {
                    m_Actor.Activity = Activity.FOLLOWING_ORDER;
                    return orderAction;
                }
            }
            #endregion

            ///////////////////////////////////////
            // alpha10 OBSOLETE 1 equip weapon
            // alpha10 OBSOLETE 2 equip armor
            // 3 fire at nearest enemy.
            // 4 hit adjacent enemy.
            // 5 warn trepassers.
            // 6 shout
            // 7 rest if tired
            // 8 charge enemy
            // 9 sleep when sleepy.
            // 10 follow leader.
            // 11 wander in CHAR office.
            // 12 wander.
            //////////////////////////////////////

            // don't run by default.
            m_Actor.IsRunning = false;

            // get data.
            List<Percept> allEnemies = FilterEnemies(game, mapPercepts);
            List<Percept> currentEnemies = FilterCurrent(game, allEnemies);
            bool checkOurLeader = m_Actor.HasLeader && !DontFollowLeader;
            bool hasAnyEnemies = allEnemies != null;

            //// 1 equip weapon
            //// alpha 10 OBSOLETE
            //ActorAction equipWpnAction = BehaviorEquipWeapon(game);
            //if (equipWpnAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipWpnAction;
            //}

            //// 2 equip armor
            //ActorAction equipArmAction = BehaviorEquipBestBodyArmor(game);
            //if (equipArmAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipArmAction;
            //}

            // 3 fire at nearest enemy.
            if (currentEnemies != null)
            {
                List<Percept> fireTargets = FilterFireTargets(game, currentEnemies);
                if (fireTargets != null)
                {
                    Percept nearestTarget = FilterNearest(game, fireTargets);
                    Actor targetActor = nearestTarget.Percepted as Actor;

                    ActorAction fireAction = BehaviorRangedAttack(game, nearestTarget);
                    if (fireAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = targetActor;
                        return fireAction;
                    }
                }
            }

            // 4 hit adjacent enemy
            if (currentEnemies != null)
            {
                Percept nearestEnemy = FilterNearest(game, currentEnemies);
                Actor targetActor = nearestEnemy.Percepted as Actor;

                // fight or flee?
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS; // alpha10
                ActorAction fightOrFlee = BehaviorFightOrFlee(game, currentEnemies, true, true, ActorCourage.COURAGEOUS, FIGHT_EMOTES, allowedChargeActions);
                if (fightOrFlee != null)
                {
                    return fightOrFlee;
                }
            }

            // 5 warn trepassers.
            List<Percept> nonEnemies = FilterNonEnemies(game, mapPercepts);
            if (nonEnemies != null)
            {
                List<Percept> trespassers = Filter(game, nonEnemies, (p) =>
                {
                    Actor other = (p.Percepted as Actor);
                    if (other.Faction == game.GameFactions.TheCHARCorporation)
                        return false;

                    // alpha10 bug fix only if visible right now!
                    if (p.Turn != m_Actor.Location.Map.LocalTime.TurnCounter)
                        return false;

                    return game.IsInCHARProperty(other.Location);
                });
                if (trespassers != null)
                {
                    // Hey YOU!
                    Actor trespasser = FilterNearest(game, trespassers).Percepted as Actor;

                    game.DoMakeAggression(m_Actor, trespasser);

                    m_Actor.Activity = Activity.FIGHTING;
                    m_Actor.TargetActor = trespasser;
                    return new ActionSay(m_Actor, game, trespasser, "Hey YOU!", RogueGame.Sayflags.IS_IMPORTANT | RogueGame.Sayflags.IS_DANGER);
                }
            }

            // 6 shout
            if (hasAnyEnemies)
            {
                if (nonEnemies != null)
                {
                    ActorAction shoutAction = BehaviorWarnFriends(game, nonEnemies, FilterNearest(game, allEnemies).Percepted as Actor);
                    if (shoutAction != null)
                    {
                        m_Actor.Activity = Activity.IDLE;
                        return shoutAction;
                    }
                }
            }

            // 7 rest if tired
            ActorAction restAction = BehaviorRestIfTired(game);
            if (restAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor, game);
            }

            // 8 charge/chase enemy
            if (allEnemies != null)
            {
                Percept chasePercept = FilterNearest(game, allEnemies);

                // cheat a bit for good chasing behavior.
                if (m_Actor.Location == chasePercept.Location)
                {
                    // memorized location reached, chase now the actor directly (cheat so they appear more intelligent)
                    Actor chasedActor = chasePercept.Percepted as Actor;
                    chasePercept = new Percept(chasedActor, m_Actor.Location.Map.LocalTime.TurnCounter, chasedActor.Location);
                }

                // alpha10 chase only if reachable
                if (CanReachSimple(game, chasePercept.Location.Position, RouteFinder.SpecialActions.DOORS | RouteFinder.SpecialActions.JUMP))
                {
                    // chase.
                    ActorAction chargeAction = BehaviorChargeEnemy(game, chasePercept, false, false);
                    if (chargeAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = chasePercept.Percepted as Actor;
                        return chargeAction;
                    }
                }
            }

            // 9 sleep when sleepy
            if (game.Rules.IsActorSleepy(m_Actor) && !hasAnyEnemies)
            {
                ActorAction sleepAction = BehaviorSleep(game, m_LOSSensor.FOV);
                if (sleepAction != null)
                {
                    if (sleepAction is ActionSleep)
                        m_Actor.Activity = Activity.SLEEPING;
                    return sleepAction;
                }
            }

            // 10 follow leader
            #region
            if (checkOurLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                bool isLeaderVisible = m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
                ActorAction followAction = BehaviorFollowActor(game, m_Actor.Leader, lastKnownLeaderPosition, isLeaderVisible, 1);
                if (followAction != null)
                {
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }
            #endregion

            // 11 wander in CHAR office.
            ActorAction wanderInOfficeAction = BehaviorWander(game, (loc) => RogueGame.IsInCHAROffice(loc), null);
            if (wanderInOfficeAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return wanderInOfficeAction;
            }

            // 12 wander
            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(game, null);
        }
        #endregion
    }
}
