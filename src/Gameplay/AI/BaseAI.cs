using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay.AI.Sensors;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    abstract class BaseAI : AIController
    {
        #region Types
        protected class ChoiceEval<_T_>
        {
            public _T_ Choice { get; private set; }
            public float Value { get; private set; }

            public ChoiceEval(_T_ choice, float value)
            {
                this.Choice = choice;
                this.Value = value;
            }

            public override string ToString()
            {
                return String.Format("ChoiceEval({0}; {1:F})", (this.Choice == null ? "NULL" : this.Choice.ToString()), this.Value);
            }
        }
        #endregion

        #region Constants
        const int FLEE_THROUGH_EXIT_CHANCE = 50;

        const int EMOTE_GRAB_ITEM_CHANCE = 30;
        const int EMOTE_FLEE_CHANCE = 30;
        const int EMOTE_FLEE_TRAPPED_CHANCE = 50;
        const int EMOTE_CHARGE_CHANCE = 30;

        const float MOVE_DISTANCE_PENALTY = 0.42f;  // slightly > to diagonal distance (sqrt(2))

        const float LEADER_LOF_PENALTY = 1;
        #endregion

        #region Fields
        ActorOrder m_Order;
        ActorDirective m_Directive;
        Location m_prevLocation;
        List<Item> m_TabooItems;    // list is better than dictionary since we expect it to be very small.
        List<Point> m_TabooTiles;
        List<Actor> m_TabooTrades;
        #endregion

        #region Properties
        public override ActorOrder Order
        {
            get { return m_Order; }
        }

        public override ActorDirective Directives
        {
            get 
            {
                if (m_Directive == null)
                    m_Directive = new ActorDirective();
                return m_Directive; 
            }
            set { m_Directive = value; }
        }

        protected Location PrevLocation
        {
            get { return m_prevLocation; }
        }

        protected List<Item> TabooItems
        {
            get { return m_TabooItems; }
        }

        protected List<Point> TabooTiles
        {
            get { return m_TabooTiles; }
        }

        protected List<Actor> TabooTrades
        {
            get { return m_TabooTrades; }
        }
        #endregion

        #region AIController
        public override void TakeControl(Actor actor)
        {
            base.TakeControl(actor);

            CreateSensors();

            m_TabooItems = null;
            m_TabooTiles = null;
            m_TabooTrades = null;
        }

        public override void SetOrder(ActorOrder newOrder)
        {
            m_Order = newOrder;
        }

        public override ActorAction GetAction(RogueGame game)
        {
            /////////////////////////
            // 1. Update sensors.
            // 2. Issue action.
            /////////////////////////

            // 2. Update sensors.
            List<Percept> percepts = UpdateSensors(game);

            // 3. Issue action.
            if (m_prevLocation.Map == null)
                m_prevLocation = m_Actor.Location;
            m_Actor.TargetActor = null;
            ActorAction bestAction = SelectAction(game, percepts);
            m_prevLocation = m_Actor.Location;
            if (bestAction == null)
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor, game);
            }
            return bestAction;
        }
        #endregion

        #region Strategy followed in GetAction
        protected abstract void CreateSensors();
        protected abstract List<Percept> UpdateSensors(RogueGame game);
        protected abstract ActorAction SelectAction(RogueGame game, List<Percept> percepts);
        #endregion

        #region Common sensor filters
        protected List<Percept> FilterSameMap(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;
            Map map = m_Actor.Location.Map;
            foreach (Percept p in percepts)
            {
                if (p.Location.Map == map)
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="percepts"></param>
        /// <returns>null if no enemies</returns>
        protected List<Percept> FilterEnemies(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                Actor other = p.Percepted as Actor;
                if (other != null && other != m_Actor)
                {
                    if (game.Rules.IsEnemyOf(m_Actor, other))
                    {
                        if (list == null)
                            list = new List<Percept>(percepts.Count);
                        list.Add(p);
                    }
                }
            }

            return list;
        }

        protected List<Percept> FilterNonEnemies(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                Actor other = p.Percepted as Actor;
                if (other != null && other != m_Actor)
                {
                    if (!game.Rules.IsEnemyOf(m_Actor, other))
                    {
                        if (list == null)
                            list = new List<Percept>(percepts.Count);
                        list.Add(p);
                    }
                }
            }

            return list;
        }

        protected List<Percept> FilterCurrent(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            int turn = m_Actor.Location.Map.LocalTime.TurnCounter;
            foreach (Percept p in percepts)
            {
                if (p.Turn == turn)
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }

        protected Percept FilterNearest(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            Percept best = percepts[0];
            float nearest = game.Rules.StdDistance(m_Actor.Location.Position, percepts[0].Location.Position);

            for (int i = 1; i < percepts.Count; i++)
            {
                Percept p = percepts[i];
                float dist = game.Rules.StdDistance(m_Actor.Location.Position, p.Location.Position);
                if (dist < nearest)
                {
                    best = p;
                    nearest = dist;
                }
            }

            return best;
        }

        protected Percept FilterStrongestScent(RogueGame game, List<Percept> scents)
        {
            if (scents == null || scents.Count == 0)
                return null;

            Percept pBest = null;
            SmellSensor.AIScent best = null;
            foreach (Percept p in scents)
            {
                SmellSensor.AIScent aiScent = p.Percepted as SmellSensor.AIScent;
                if (aiScent == null)
                    throw new InvalidOperationException("percept not an aiScent");
                if (pBest == null || aiScent.Strength > best.Strength)
                {
                    best = aiScent;
                    pBest = p;
                }
            }

            return pBest;
        }

#if false
        obsolete
        protected List<Percept> FilterOdor(RogueGame game, List<Percept> percepts, Odor odor)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                SmellSensor.AIScent aiScent = p.Percepted as SmellSensor.AIScent;
                if (aiScent != null && aiScent.Odor == odor)
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }

        protected Percept FilterStrongestAdjacentScent(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            Percept best = null;
            int strongest = int.MaxValue;

            foreach (Percept p in percepts)
            {
                SmellSensor.AIScent aiScent = p.Percepted as SmellSensor.AIScent;
                if (aiScent != null)
                {
                    if (aiScent.Strength > strongest && game.Rules.IsAdjacent(p.Location.Position, m_Actor.Location.Position))
                    {
                        best = p;
                        strongest = aiScent.Strength;
                    }
                }
            }

            return best;
        }

        protected Percept FilterStrongestVisibleScent(RogueGame game, List<Percept> percepts, HashSet<Point> fov)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            Percept best = null;
            int strongest = int.MinValue;

            foreach (Percept p in percepts)
            {
                SmellSensor.AIScent aiScent = p.Percepted as SmellSensor.AIScent;
                if (aiScent != null)
                {
                    if (aiScent.Strength > strongest && fov.Contains(p.Location.Position))
                    {
                        best = p;
                        strongest = aiScent.Strength;
                    }
                }
            }

            return best;
        }
#endif

        protected List<Percept> FilterActorsModel(RogueGame game, List<Percept> percepts, ActorModel model)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                Actor a = p.Percepted as Actor;
                if (a != null && a.Model == model)
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }

        protected List<Percept> FilterActors(RogueGame game, List<Percept> percepts, Predicate<Actor> predicateFn)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                Actor a = p.Percepted as Actor;
                if (a != null && predicateFn(a))
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }

        protected List<Percept> FilterFireTargets(RogueGame game, List<Percept> percepts)
        {
            return Filter(game, percepts, 
                (p) =>
                {
                    Actor other = p.Percepted as Actor;
                    if (other == null)
                        return false;
                    return game.Rules.CanActorFireAt(m_Actor, other);
                });
        }

        protected List<Percept> FilterStacks(RogueGame game, List<Percept> percepts)
        {
            return Filter(game, percepts,
                (p) =>
                {
                    Inventory it = p.Percepted as Inventory;
                    if (it == null)
                        return false;
                    return true;
                });
        }

        protected List<Percept> FilterCorpses(RogueGame game, List<Percept> percepts)
        {
            return Filter(game, percepts,
                (p) =>
                {
                    List<Corpse> corpses = p.Percepted as List<Corpse>;
                    if (corpses == null)
                        return false;
                    return true;
                });
        }

        protected List<Percept> Filter(RogueGame game, List<Percept> percepts, Predicate<Percept> predicateFn)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> list = null;

            foreach (Percept p in percepts)
            {
                if (predicateFn(p))
                {
                    if (list == null)
                        list = new List<Percept>(percepts.Count);
                    list.Add(p);
                }
            }

            return list;
        }

        protected Percept FilterFirst(RogueGame game, List<Percept> percepts, Predicate<Percept> predicateFn)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            foreach (Percept p in percepts)
            {
                if (predicateFn(p))
                    return p;
            }

            return null;
        }

        protected List<Percept> FilterOut(RogueGame game, List<Percept> percepts, Predicate<Percept> rejectPredicateFn)
        {
            return Filter(game, percepts, (p) => !rejectPredicateFn(p));
        }

        /// <summary>
        /// Closest first.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="percepts"></param>
        /// <returns></returns>
        protected List<Percept> SortByDistance(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            Point from = m_Actor.Location.Position;

            List<Percept> sortedList = new List<Percept>(percepts);

            sortedList.Sort((pA, pB) =>
            {
                float dA = game.Rules.StdDistance(pA.Location.Position, from);
                float dB = game.Rules.StdDistance(pB.Location.Position, from);

                return dA > dB ? 1 :
                    dA < dB ? -1 :
                    0;
            });

            return sortedList;
        }

        /// <summary>
        /// Most recent first.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="percepts"></param>
        /// <returns></returns>
        protected List<Percept> SortByDate(RogueGame game, List<Percept> percepts)
        {
            if (percepts == null || percepts.Count == 0)
                return null;

            List<Percept> sortedList = new List<Percept>(percepts);

            sortedList.Sort((pA, pB) =>
            {
                return pA.Turn < pB.Turn ? 1 :
                    pA.Turn > pB.Turn ? -1 :
                    0;
            });

            return sortedList;
        }

        #endregion

        #region Common behaviors

        #region Movement
        protected ActorAction BehaviorWander(RogueGame game, Predicate<Location> goodWanderLocFn)
        {
            ChoiceEval<Direction> chooseRandomDir = Choose<Direction>(game,
                Direction.COMPASS_LIST,
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    if (goodWanderLocFn != null && !goodWanderLocFn(next))
                        return false;
                    ActorAction bumpAction = game.Rules.IsBumpableFor(m_Actor, game, next);
                    return isValidWanderAction(game, bumpAction);
                },
                (dir) =>
                {
                    int score = game.Rules.Roll(0, 666); 
                    if (m_Actor.Model.Abilities.IsIntelligent)
                    {
                        if (IsAnyActivatedTrapThere(m_Actor.Location.Map, (m_Actor.Location + dir).Position))
                            score -= 1000;
                    }
                    return score;
                },
                (a, b) => a > b);

            if (chooseRandomDir != null)
                return new ActionBump(m_Actor, game, chooseRandomDir.Choice);
            else
                return null;
        }

        protected ActorAction BehaviorWander(RogueGame game)
        {
            return BehaviorWander(game, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="goal"></param>
        /// <param name="distanceFn">float.Nan to forbid a move</param>
        /// <returns></returns>
        protected ActorAction BehaviorBumpToward(RogueGame game, Point goal, Func<Point, Point, float> distanceFn)
        {
            ChoiceEval<ActorAction> bestCloserDir = ChooseExtended<Direction, ActorAction>(game,
                Direction.COMPASS_LIST,
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    ActorAction bumpAction = game.Rules.IsBumpableFor(m_Actor, game, next);
                    if (bumpAction == null)
                    {
                        // for undeads, try to push the blocking object randomly.
                        if (m_Actor.Model.Abilities.IsUndead && game.Rules.HasActorPushAbility(m_Actor))
                        {
                            MapObject obj = m_Actor.Location.Map.GetMapObjectAt(next.Position);
                            if (obj != null)
                            {
                                if (game.Rules.CanActorPush(m_Actor, obj))
                                {
                                    Direction pushDir = game.Rules.RollDirection();
                                    if (game.Rules.CanPushObjectTo(obj, obj.Location.Position + pushDir))
                                        return new ActionPush(m_Actor, game, obj, pushDir);
                                }
                            }
                        }

                        // failed.
                        return null;
                    }
                    if (next.Position == goal)
                        return bumpAction;
                    if (IsValidMoveTowardGoalAction(bumpAction))
                        return bumpAction;
                    else
                        return null;
                },
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    if (distanceFn != null)
                        return distanceFn(next.Position, goal);
                    else
                        return game.Rules.StdDistance(next.Position, goal);
                },
                (a, b) => !float.IsNaN(a) && a < b);

            if (bestCloserDir != null)
                return bestCloserDir.Choice;
            else
                return null;
        }

        protected ActorAction BehaviorStupidBumpToward(RogueGame game, Point goal)
        {
            return BehaviorBumpToward(game, goal,
                (ptA, ptB) =>
                {
                    if (ptA == ptB) return 0;
                    float distance = game.Rules.StdDistance(ptA, ptB);
                    //if (distance < 2f) return distance;

                    // penalize having to push/bash/jump.
                    if (!game.Rules.IsWalkableFor(m_Actor, m_Actor.Location.Map, ptA.X, ptA.Y))
                        distance += MOVE_DISTANCE_PENALTY;

                    return distance;
                });
        }

        protected ActorAction BehaviorIntelligentBumpToward(RogueGame game, Point goal)
        {
            float currentDistance = game.Rules.StdDistance(m_Actor.Location.Position, goal);
            bool imStarvingOrCourageous = game.Rules.IsActorStarving(m_Actor) || Directives.Courage == ActorCourage.COURAGEOUS;

            ActorAction bump = BehaviorBumpToward(game, goal,
                (ptA, ptB) =>
                {
                    if (ptA == ptB) return 0;
                    float distance = game.Rules.StdDistance(ptA, ptB);
                    //if (distance < 2f) return distance;

                    // consider only moves that make takes us closer.
                    if (distance >= currentDistance)
                        return float.NaN;

                    // avoid stepping on damaging traps, unless starving or courageous.
                    if (!imStarvingOrCourageous)
                    {
                        int trapsDamage = ComputeTrapsMaxDamage(m_Actor.Location.Map, ptA);
                        if (trapsDamage > 0)
                        {
                            // if death, don't do it.
                            if (trapsDamage >= m_Actor.HitPoints)
                                return float.NaN;
                            // avoid.
                            distance += MOVE_DISTANCE_PENALTY;
                        }
                    }

                    return distance;
                });
            return bump;
        }

        protected ActorAction BehaviorWalkAwayFrom(RogueGame game, Percept goal)
        {
            return BehaviorWalkAwayFrom(game, new List<Percept>(1) { goal });
        }

        protected ActorAction BehaviorWalkAwayFrom(RogueGame game, List<Percept> goals)
        {
            // stuff to avoid stepping into leader LoF.
            Actor myLeader = m_Actor.Leader;
            bool leaderIsFiring = m_Actor.HasLeader && m_Actor.GetEquippedWeapon() is ItemRangedWeapon;
            Actor leaderNearestTarget = null;
            if (leaderIsFiring) leaderNearestTarget = GetNearestTargetFor(game, m_Actor.Leader);
            bool checkLeaderLoF = leaderNearestTarget != null && leaderNearestTarget.Location.Map == m_Actor.Location.Map;
            List<Point> leaderLoF = null;
            if (checkLeaderLoF)
            {
                leaderLoF = new List<Point>(1);
                ItemRangedWeapon wpn = m_Actor.GetEquippedWeapon() as ItemRangedWeapon;
                LOS.CanTraceFireLine(myLeader.Location, leaderNearestTarget.Location.Position, (wpn.Model as ItemRangedWeaponModel).Attack.Range, leaderLoF);
            }

            ChoiceEval<Direction> bestAwayDir = Choose<Direction>(game,
                Direction.COMPASS_LIST,
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    ActorAction bumpAction = game.Rules.IsBumpableFor(m_Actor, game, next);
                    return IsValidFleeingAction(bumpAction);
                },
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    // Heuristic value:
                    // - Safety from dangers.
                    // - If follower, stay close to leader but avoid stepping into leader LoF.
                    float safetyValue = SafetyFrom(game.Rules, next.Position, goals);
                    if (m_Actor.HasLeader)
                    {
                        // stay close to leader.
                        safetyValue -= game.Rules.StdDistance(next.Position, m_Actor.Leader.Location.Position);
                        // don't step into leader LoF.
                        if (checkLeaderLoF)
                        {
                            if (leaderLoF.Contains(next.Position))
                                safetyValue -= LEADER_LOF_PENALTY;
                        }
                    }
                    return safetyValue;
                },
                (a, b) => a > b);

            if (bestAwayDir != null)// && bestAwayDir.Value > notMovingValue) nope, moving is always better than not moving
                return new ActionBump(m_Actor, game, bestAwayDir.Choice);
            else
                return null;
        }
        #endregion

        #region Melee attack
        protected ActorAction BehaviorMeleeAttack(RogueGame game, Percept target)
        {
            Actor targetActor = target.Percepted as Actor;
            if (targetActor == null)
                throw new ArgumentException("percepted is not an actor");

            // if illegal cant.
            if (!game.Rules.CanActorMeleeAttack(m_Actor, targetActor))
                return null;

            // melee!
            return new ActionMeleeAttack(m_Actor, game, targetActor);
        }
        #endregion

        #region Ranged attack
        protected ActorAction BehaviorRangedAttack(RogueGame game, Percept target)
        {
            Actor targetActor = target.Percepted as Actor;
            if (targetActor == null)
                throw new ArgumentException("percepted is not an actor");

            // if illegal cant.
            if (!game.Rules.CanActorFireAt(m_Actor,targetActor))
                return null;

            // fire!
            return new ActionRangedAttack(m_Actor, game, targetActor);
        }
        #endregion

        #region Equiping items
        protected ActorAction BehaviorEquipWeapon(RogueGame game)
        {
            #region Ranged first
            // If already equiped a ranged weapon, we might want to reload it.
            Item eqWpn = GetEquippedWeapon();
            if (eqWpn != null && eqWpn is ItemRangedWeapon)
            {
                // ranged weapon equipped, if directive disabled unequip it!
                if (!this.Directives.CanFireWeapons)
                    return new ActionUnequipItem(m_Actor, game, eqWpn);

                // ranged weapon equipped, reload it?
                ItemRangedWeapon rw = eqWpn as ItemRangedWeapon;
                if (rw.Ammo <= 0)
                {
                    // reload it if we can.
                    ItemAmmo ammoIt = GetCompatibleAmmoItem(game, rw);
                    if (ammoIt != null)
                        return new ActionUseItem(m_Actor, game, ammoIt);
                }
                else
                    // nope, ranged equipped with ammo, nothing more to do with it.
                    return null;
            }

            // No ranged weapon equipped or equipped but out of ammo and no ammos to reload.
            // Equip other best available ranged weapon, if allowed to fire.
            if (this.Directives.CanFireWeapons)
            {
                Item newRanged = GetBestRangedWeaponWithAmmo((it) => !IsItemTaboo(it));
                if (newRanged != null)
                {
                    // equip new.
                    if (game.Rules.CanActorEquipItem(m_Actor, newRanged))
                        return new ActionEquipItem(m_Actor, game, newRanged);
                }
            }
            #endregion

            #region Melee second
            // Get best melee weapon in inventory.
            ItemMeleeWeapon bestMeleeWeapon = GetBestMeleeWeapon(game, (it) => !IsItemTaboo(it));

            // If none, nothing to do.
            if (bestMeleeWeapon == null)
                return null;
            
            // If it is already equipped, done.
            if (eqWpn == bestMeleeWeapon)
                return null;

            // If no weapon equipped, equip best now.
            if (eqWpn == null)
            {
                if (game.Rules.CanActorEquipItem(m_Actor, bestMeleeWeapon))
                    return new ActionEquipItem(m_Actor, game, bestMeleeWeapon);
                else
                    return null;
            }

            // Another weapon equipped, unequip it.
            if (eqWpn != null)
            {
                if (game.Rules.CanActorUnequipItem(m_Actor, eqWpn))
                    return new ActionUnequipItem(m_Actor, game, eqWpn);
                else
                    return null;
            }
            #endregion

            // Fail.
            return null;
        }

        protected ActorAction BehaviorEquipBodyArmor(RogueGame game)
        {
            // Get best armor available.
            ItemBodyArmor bestArmor = GetBestBodyArmor(game, (it) => !IsItemTaboo(it));

            // If none, don't bother.
            if (bestArmor == null)
                return null;

            // If already equipped, fine.
            Item eqArmor = GetEquippedBodyArmor();
            if (eqArmor == bestArmor)
                return null;

            // If another armor already equipped, unequip it first.
            if (eqArmor != null)
            {
                if (game.Rules.CanActorUnequipItem(m_Actor, eqArmor))
                    return new ActionUnequipItem(m_Actor, game, eqArmor);
                else
                    return null;
            }

            // Equip the new armor.
            if (eqArmor == null)
            {
                if (game.Rules.CanActorEquipItem(m_Actor, bestArmor))
                    return new ActionEquipItem(m_Actor, game, bestArmor);
                else
                    return null;
            }

            // Fail.
            return null;
        }

        protected ActorAction BehaviorEquipCellPhone(RogueGame game)
        {
            // Only equip cellphone if :
            // - is a leader.
            // - or if leader does.
            bool wantTracker = false;
            if (m_Actor.CountFollowers > 0)
                wantTracker = true;
            else if (m_Actor.HasLeader)
            {
                bool leaderHasTrackerEq = false;
                ItemTracker leaderTr = m_Actor.Leader.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
                if (leaderTr == null)
                    leaderHasTrackerEq = false;
                else if (leaderTr.CanTrackFollowersOrLeader)
                    leaderHasTrackerEq = true;

                wantTracker = leaderHasTrackerEq;
            }

            // If already equiped a cellphone, nothing to do or unequip it.
            Item eqTrack = GetEquippedCellPhone();
            if (eqTrack != null)
            {
                if (!wantTracker)
                    return new ActionUnequipItem(m_Actor, game, eqTrack);
                else
                    return null;
            }

            if (!wantTracker)
                return null;

            // Equip first available cellphone.
            Item newTracker = GetFirstTracker((it) => it.CanTrackFollowersOrLeader && !IsItemTaboo(it));
            if (newTracker != null)
            {
                // equip new.
                if (game.Rules.CanActorEquipItem(m_Actor, newTracker))
                    return new ActionEquipItem(m_Actor, game, newTracker);
            }

            // Fail.
            return null;
        }

        protected ActorAction BehaviorUnequipCellPhoneIfLeaderHasNot(RogueGame game)
        {
            // get left eq item.
            ItemTracker tr = m_Actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (tr == null)
                return null;
            if (!tr.CanTrackFollowersOrLeader)
                return null;

            // we have a cell phone equiped.
            // unequip if leader has not one equiped.
            ItemTracker leaderTr = m_Actor.Leader.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (leaderTr == null || !leaderTr.CanTrackFollowersOrLeader)
            {
                // unequip!
                if (game.Rules.CanActorUnequipItem(m_Actor, tr))
                    return new ActionUnequipItem(m_Actor, game, tr);
            }

            // fail.
            return null;
        }

        protected ActorAction BehaviorEquipLight(RogueGame game)
        {
            // If already equiped a light, nothing to do.
            Item eqLight = GetEquippedLight();
            if (eqLight != null)
                return null;

            // Equip first available light.
            Item newLight = GetFirstLight((it) => !IsItemTaboo(it));
            if (newLight != null)
            {
                // equip new.
                if (game.Rules.CanActorEquipItem(m_Actor, newLight))
                    return new ActionEquipItem(m_Actor, game, newLight);
            }

            // Fail.
            return null;
        }

        protected ActorAction BehaviorEquipStenchKiller(RogueGame game)
        {
            // If already equiped a suitable one, nothing to do.
            Item eqStench = GetEquippedStenchKiller();
            if (eqStench != null)
                return null;

            // Equip first available.
            ItemSprayScent newStench = GetFirstStenchKiller((it) => !IsItemTaboo(it));
            if (newStench != null)
            {
                // equip new.
                if (game.Rules.CanActorEquipItem(m_Actor, newStench))
                    return new ActionEquipItem(m_Actor, game, newStench);
            }

            // Fail.
            return null;
        }

        protected ActorAction BehaviorUnequipLeftItem(RogueGame game)
        {
            // get left eq item.
            Item eqLeft = m_Actor.GetEquippedItem(DollPart.LEFT_HAND);
            if (eqLeft == null)
                return null;

            // try to unequip it.
            if (game.Rules.CanActorUnequipItem(m_Actor, eqLeft))
                return new ActionUnequipItem(m_Actor, game, eqLeft);

            // fail.
            return null;
        }
        #endregion

        #region Getting items
        protected ActorAction BehaviorGrabFromStack(RogueGame game, Point position, Inventory stack)
        {
            // ignore empty stacks.
            if (stack == null || stack.IsEmpty)
                return null;

            // fix: don't try to get items under blocking map objects - bumping will say "yes can move" but we actually cannot take it.
            MapObject objThere = m_Actor.Location.Map.GetMapObjectAt(position);
            if (objThere != null)
            {
                // un-walkable fortification
                Fortification fort = objThere as Fortification;
                if (fort != null && !fort.IsWalkable)
                    return null;
                // barricaded door/window
                DoorWindow door = objThere as DoorWindow;
                if (door != null && door.IsBarricaded)
                    return null;
            }

            // for each item in the stack, consider only the takeable and interesting ones.
            Item goodItem = null;
            foreach (Item it in stack.Items)
            {
                // if can't take, ignore.
                if (!game.Rules.CanActorGetItem(m_Actor, it))
                    continue;
                // if not interesting, ignore.
                if (!IsInterestingItem(game, it))
                    continue;
                // gettable and interesting, get it.
                goodItem = it;
                break;
            }

            // if no good item, ignore.
            if (goodItem == null)
                return null;

            // take it!
            Item takeIt = goodItem;

            // emote?
            if (game.Rules.RollChance(EMOTE_GRAB_ITEM_CHANCE))
                game.DoEmote(m_Actor, String.Format("{0}! Great!", takeIt.AName));

            // try to move/get one.
            if (position == m_Actor.Location.Position)
                return new ActionTakeItem(m_Actor, game, position, takeIt);
            else
                return BehaviorIntelligentBumpToward(game, position);
        }
        #endregion

        #region Droping items
        protected ActorAction BehaviorDropItem(RogueGame game, Item it)
        {
            if (it == null)
                return null;

            // 1. unequip?
            if (game.Rules.CanActorUnequipItem(m_Actor, it))
            {
                // mark item as taboo.
                MarkItemAsTaboo(it);

                // unequip.
                return new ActionUnequipItem(m_Actor, game, it);
            }

            // 2. drop?
            if (game.Rules.CanActorDropItem(m_Actor, it))
            {
                // unmark item as taboo.
                UnmarkItemAsTaboo(it);

                // drop.
                return new ActionDropItem(m_Actor, game, it);
            }

            // failed!
            return null;
        }


        protected ActorAction BehaviorDropUselessItem(RogueGame game)
        {
            if (m_Actor.Inventory.IsEmpty)
                return null;

            // unequip/drop first light/tracker/spray out of batteries/quantity.
            foreach (Item it in m_Actor.Inventory.Items)
            {
                bool dropIt = false;

                if (it is ItemLight)
                    dropIt = (it as ItemLight).Batteries <= 0;
                else if (it is ItemTracker)
                    dropIt = (it as ItemTracker).Batteries <= 0;
                else if (it is ItemSprayPaint)
                    dropIt = (it as ItemSprayPaint).PaintQuantity <= 0;
                else if (it is ItemSprayScent)
                    dropIt = (it as ItemSprayScent).SprayQuantity <= 0;

                if (dropIt)
                    return BehaviorDropItem(game, it);
            }

            // nope.
            return null;
        }
        #endregion

        #region Resting, Eating & Sleeping
        protected ActorAction BehaviorRestIfTired(RogueGame game)
        {
            // if not tired, don't.
            if (m_Actor.StaminaPoints >= Rules.STAMINA_MIN_FOR_ACTIVITY)
                return null;

            // tired, rest.
            return new ActionWait(m_Actor, game);
        }

        protected ActorAction BehaviorEat(RogueGame game)
        {
            // find best edible eat.
            Item it = GetBestEdibleItem(game);
            if (it == null)
                return null;

            // i can haz it?
            if (!game.Rules.CanActorUseItem(m_Actor, it))
                return null;
            
            // eat it!
            return new ActionUseItem(m_Actor, game, it);
        }

        protected ActorAction BehaviorSleep(RogueGame game, HashSet<Point> FOV)
        {
            // can?
            if (!game.Rules.CanActorSleep(m_Actor))
                return null;

            // if next to a door/window, try moving away from it.
            Map map = m_Actor.Location.Map;
            if (map.HasAnyAdjacentInMap(m_Actor.Location.Position, (pt) => map.GetMapObjectAt(pt) is DoorWindow))
            {
                // wander where there is no door/window and not adjacent to a door window.
                ActorAction wanderAwayFromDoor = BehaviorWander(game, 
                    (loc) => map.GetMapObjectAt(loc.Position) as DoorWindow == null && !map.HasAnyAdjacentInMap(loc.Position, (pt) => loc.Map.GetMapObjectAt(pt) is DoorWindow));
                if (wanderAwayFromDoor != null)
                    return wanderAwayFromDoor;
                // no good spot, just try normal sleep behavior.
            }

            // sleep on a couch.
            if (game.Rules.IsOnCouch(m_Actor))
            {
                return new ActionSleep(m_Actor, game);
            }
            // find nearest couch.
            Point? couchPos = null;
            float nearestDist = float.MaxValue;
            foreach (Point p in FOV)
            {
                MapObject mapObj = map.GetMapObjectAt(p);
                if (mapObj != null && mapObj.IsCouch && map.GetActorAt(p) == null)
                {
                    float dist = game.Rules.StdDistance(m_Actor.Location.Position, p);
                    if (dist < nearestDist)
                    {
                        nearestDist = dist;
                        couchPos = p;
                    }
                }
            }
            // if we have a couch, try to get there.
            if (couchPos != null)
            {
                ActorAction moveThere = BehaviorIntelligentBumpToward(game, couchPos.Value);
                if (moveThere != null)
                {
                    return moveThere;
                }
            }

            // no couch or can't move there, sleep there.
            return new ActionSleep(m_Actor, game);
        }
        #endregion

        #region Barricading & Building & Traps

        protected int ComputeTrapsMaxDamage(Map map, Point pos)
        {
            Inventory inv = map.GetItemsAt(pos);
            if (inv == null) return 0;

            int sum = 0;
            ItemTrap trp=null;
            foreach (Item it in inv.Items)
            {
                trp = it as ItemTrap;
                if (trp == null) continue;
                sum += trp.TrapModel.Damage;
            }
            return sum;
        }

        protected ActorAction BehaviorBuildTrap(RogueGame game)
        {
            // don't bother if we don't have a trap.
            ItemTrap trap = m_Actor.Inventory.GetFirstByType(typeof(ItemTrap)) as ItemTrap;
            if (trap == null)
                return null;

            // is this a good spot for a trap?            
            string reason;
            if (!IsGoodTrapSpot(game, m_Actor.Location.Map, m_Actor.Location.Position, out reason))
                return null;

            // if trap needs to be activated, do it.
            if (!trap.IsActivated && !trap.TrapModel.ActivatesWhenDropped)
                return new ActionUseItem(m_Actor, game, trap);

            // trap ready to setup, do it!
            game.DoEmote(m_Actor, String.Format("{0} {1}!", reason, trap.AName));
            return new ActionDropItem(m_Actor, game, trap);
        }

        protected bool IsGoodTrapSpot(RogueGame game, Map map, Point pos, out string reason)
        {
            reason = "";
            bool potentialSpot = false;

            // 1. Potential spot?
            // 2. Don't overdo it.

            // 1. Potential spot?
            // outside and has a corpse.
            bool isInside = map.GetTileAt(pos).IsInside;
            if (!isInside && map.GetCorpsesAt(pos) != null)
            {
                reason = "that corpse will serve as a bait for";
                potentialSpot = true;
            }
            else
            {
                //  entering or leaving a building?
                bool wasInside = m_prevLocation.Map.GetTileAt(m_prevLocation.Position).IsInside;
                if (wasInside != isInside)
                {
                    reason = "protecting the building with";
                    potentialSpot = true;
                }
                else
                {
                    // ...or a door/window?
                    MapObject objThere = map.GetMapObjectAt(pos);
                    if (objThere != null && objThere is DoorWindow)
                    {
                        reason = "protecting the doorway with";
                        potentialSpot = true;
                    }
                    // ...or an exit?
                    else if (map.GetExitAt(pos) != null)
                    {
                        reason = "protecting the exit with";
                        potentialSpot = true;
                    }
                }
            }
            if (!potentialSpot)
                return false;

            // 2. Don't overdo it.
            // Never drop more than 3 traps.
            Inventory itemsThere = map.GetItemsAt(pos);
            if (itemsThere != null)
            {
                int countActivated = itemsThere.CountItemsMatching((it) =>
                {
                    ItemTrap trap = it as ItemTrap;
                    if (trap == null) return false;
                    return trap.IsActivated;
                });
                if (countActivated > 3) 
                    return false;
            }
            // TODO Need at least 2 neighbouring non adjacent tiles free of activated traps.

            // ok!
            return true;
        }
        
        protected ActorAction BehaviorBuildSmallFortification(RogueGame game)
        {
            // don't bother if no carpentry skill or not enough material.
            if (m_Actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) == 0)
                return null;
            if (game.Rules.CountBarricadingMaterial(m_Actor) < game.Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, false))
                return null;

            // pick a good adjacent tile.
            // good tiles are :
            // - in bounds, walkable, empty, not border.
            // - not exits.
            // - doorways.
            // eval is random.
            Map map = m_Actor.Location.Map;
            ChoiceEval<Direction> choice = Choose<Direction>(game, Direction.COMPASS_LIST,
                (dir) =>
                {
                    Point pt = m_Actor.Location.Position + dir;
                    if (!map.IsInBounds(pt))
                        return false;
                    if (!map.IsWalkable(pt))
                        return false;
                    if (map.IsOnMapBorder(pt.X, pt.Y))
                        return false;
                    if (map.GetActorAt(pt) != null)
                        return false;
                    if (map.GetExitAt(pt) != null)
                        return false;
                    return IsDoorwayOrCorridor(game, map, pt);
                },
                (dir) => game.Rules.Roll(0, 666),
                (a, b) => a > b);

            // if no choice, fail.
            if (choice == null)
                return null;

            // get pos.
            Point adj = m_Actor.Location.Position + choice.Choice;

            // if can't build there, fail.
            if (!game.Rules.CanActorBuildFortification(m_Actor, adj, false))
                return null;

            // ok!
            return new ActionBuildFortification(m_Actor, game, adj, false);
        }

        /// <summary>
        /// Try to make a line of large fortifications.
        /// </summary>
        /// <param name="game"></param>
        /// <returns></returns>
        protected ActorAction BehaviorBuildLargeFortification(RogueGame game, int startLineChance)
        {
            // don't bother if no carpentry skill or not enough material.
            if (m_Actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) == 0)
                return null;
            if (game.Rules.CountBarricadingMaterial(m_Actor) < game.Rules.ActorBarricadingMaterialNeedForFortification(m_Actor, true))
                return null;

            // pick a good adjacent tile.
            // good tiles are :
            // - not exit.
            // - not map border.
            // - outside and anchor/continue wall.
            // all things being equal, eval is random.
            Map map = m_Actor.Location.Map;
            ChoiceEval<Direction> choice = Choose<Direction>(game, Direction.COMPASS_LIST,
                (dir) =>
                {
                    Point pt = m_Actor.Location.Position + dir;
                    if (!map.IsInBounds(pt))
                        return false;
                    if (!map.IsWalkable(pt))
                        return false;
                    if (map.IsOnMapBorder(pt.X, pt.Y))
                        return false;
                    if (map.GetActorAt(pt) != null)
                        return false;
                    if (map.GetExitAt(pt) != null)
                        return false;

                    // outside.
                    if (map.GetTileAt(pt.X, pt.Y).IsInside)
                        return false;

                    // count stuff there.
                    int wallsAround = map.CountAdjacentInMap(pt, (ptAdj) => !map.GetTileAt(ptAdj).Model.IsWalkable);
                    int lfortsAround = map.CountAdjacentInMap(pt,
                        (ptAdj) =>
                        {
                            Fortification f = map.GetMapObjectAt(ptAdj) as Fortification;
                            return f != null && !f.IsTransparent;
                        });

                    // good spot?
                    if (wallsAround == 3 && lfortsAround == 0 && game.Rules.RollChance(startLineChance))
                        // fort line anchor.
                        return true;
                    if (wallsAround == 0 && lfortsAround == 1)
                        // fort line continuation.
                        return true;

                    // nope.
                    return false;
                },
                (dir) => game.Rules.Roll(0, 666),
                (a, b) => a > b);

            // if no choice, fail.
            if (choice == null)
                return null;

            // get pos.
            Point adj = m_Actor.Location.Position + choice.Choice;

            // if can't build there, fail.
            if (!game.Rules.CanActorBuildFortification(m_Actor, adj, true))
                return null;

            // ok!
            return new ActionBuildFortification(m_Actor, game, adj, true);
        }

        #endregion

        #region Breaking objects
        protected ActorAction BehaviorAttackBarricade(RogueGame game)
        {
            // find barricades.
            Map map = m_Actor.Location.Map;
            List<Point> adjBarricades = map.FilterAdjacentInMap(m_Actor.Location.Position,
                (pt) =>
                {
                    DoorWindow door = map.GetMapObjectAt(pt) as DoorWindow;
                    return (door != null && door.IsBarricaded);
                });

            // if none, fail.
            if (adjBarricades == null)
                return null;

            // try to attack one at random.
            DoorWindow randomBarricade = map.GetMapObjectAt(adjBarricades[game.Rules.Roll(0, adjBarricades.Count)]) as DoorWindow;
            ActionBreak attackBarricade = new ActionBreak(m_Actor, game, randomBarricade);
            if (attackBarricade.IsLegal())
                return attackBarricade;

            // nope :(
            return null;
        }

        protected ActorAction BehaviorAssaultBreakables(RogueGame game, HashSet<Point> fov)
        {
            // find all barricades & breakables in fov.
            Map map = m_Actor.Location.Map;
            List<Percept> breakables = null;
            foreach (Point pt in fov)
            {
                MapObject mapObj = map.GetMapObjectAt(pt);
                if (mapObj == null)
                    continue;
                if (!mapObj.IsBreakable)
                    continue;
                if (breakables == null)
                    breakables = new List<Percept>();
                breakables.Add(new Percept(mapObj, map.LocalTime.TurnCounter, new Location(map, pt)));
            }

            // if nothing to assault, fail.
            if (breakables == null)
                return null;

            // get nearest.
            Percept nearest = FilterNearest(game, breakables);

            // if adjacent, try to break it.
            if (game.Rules.IsAdjacent(m_Actor.Location.Position, nearest.Location.Position))
            {
                ActionBreak breakIt = new ActionBreak(m_Actor, game, nearest.Percepted as MapObject);
                if (breakIt.IsLegal())
                    return breakIt;
                else
                    // illegal, don't bother with it.
                    return null;
            }

            // not adjacent, try to get there.
            return BehaviorIntelligentBumpToward(game, nearest.Location.Position);
        }

        #endregion

        #region Pushing
        protected ActorAction BehaviorPushNonWalkableObject(RogueGame game)
        {
            // check ability.
            if (!game.Rules.HasActorPushAbility(m_Actor))
                return null;

            // find adjacent pushables that are blocking for us.
            Map map = m_Actor.Location.Map;
            List<Point> adjPushables = map.FilterAdjacentInMap(m_Actor.Location.Position,
                (pt) =>
                {
                    MapObject obj = map.GetMapObjectAt(pt);
                    if (obj == null)
                        return false;
                    // ignore if we can walk through it.
                    if (obj.IsWalkable)
                        return false;
                    // finally only if we can push it.
                    return game.Rules.CanActorPush(m_Actor, obj);
                });

            // if none, fail.
            if (adjPushables == null)
                return null;

            // try to push one at random in a random direction.
            MapObject randomPushable = map.GetMapObjectAt(adjPushables[game.Rules.Roll(0, adjPushables.Count)]);
            ActionPush pushIt = new ActionPush(m_Actor, game, randomPushable, game.Rules.RollDirection());
            if (pushIt.IsLegal())
                return pushIt;

            // nope :(
            return null;
        }
        #endregion

        #region Healing & Entertainment
        protected ActorAction BehaviorUseMedecine(RogueGame game, int factorHealing, int factorStamina, int factorSleep, int factorCure, int factorSan)
        {
            // if no items, don't bother.
            Inventory inv = m_Actor.Inventory;
            if (inv == null || inv.IsEmpty)
                return null;

            // check needs.
            bool needHP = m_Actor.HitPoints < game.Rules.ActorMaxHPs(m_Actor);
            bool needSTA = game.Rules.IsActorTired(m_Actor);
            bool needSLP = m_Actor.Model.Abilities.HasToSleep && WouldLikeToSleep(game, m_Actor);
            bool needCure = m_Actor.Infection > 0;
            bool needSan = m_Actor.Model.Abilities.HasSanity && m_Actor.Sanity < (int)(0.75f * game.Rules.ActorMaxSanity(m_Actor));
            
            // if no need, don't.
            if (!needHP && !needSTA && !needSLP && !needCure && !needSan)
                return null;

            // list meds items.
            List<ItemMedicine> medItems = inv.GetItemsByType<ItemMedicine>();
            if (medItems == null)
                return null;

            // use best item.
            ChoiceEval<ItemMedicine> bestMedChoice = Choose<ItemMedicine>(game, medItems,
                (it) =>
                {
                    return true;
                },
                (it) =>
                {
                    int score = 0;
                    if (needHP) score += factorHealing * it.Healing;
                    if (needSTA) score += factorStamina * it.StaminaBoost;
                    if (needSLP) score += factorSleep * it.SleepBoost;
                    if (needCure) score += factorCure * it.InfectionCure;
                    if (needSan) score += factorSan * it.SanityCure;
                    return score;
                },
                (a, b) => a > b);

            // if no suitable items or best item scores zero, do not want!
            if (bestMedChoice == null || bestMedChoice.Value <= 0)
                return null;
                
            // use med.
            return new ActionUseItem(m_Actor, game, bestMedChoice.Choice);
        }
        
        protected ActorAction BehaviorUseEntertainment(RogueGame game)
        {
            Inventory inv = m_Actor.Inventory;
            if (inv.IsEmpty) return null;

            // use first entertainment item available.
            ItemEntertainment ent = (ItemEntertainment)inv.GetFirstByType(typeof(ItemEntertainment));
            if (ent == null) return null;

            if (!game.Rules.CanActorUseItem(m_Actor, ent))
                return null;

            return new ActionUseItem(m_Actor, game, ent);
        }

        protected ActorAction BehaviorDropBoringEntertainment(RogueGame game)
        {
            Inventory inv = m_Actor.Inventory;
            if (inv.IsEmpty) return null;

            foreach (Item it in inv.Items)
            {
                if (it is ItemEntertainment && m_Actor.IsBoredOf(it))
                    return new ActionDropItem(m_Actor, game, it);
            }

            return null;
        }

        #endregion

        #region Following
        protected ActorAction BehaviorFollowActor(RogueGame game, Actor other, Point otherPosition, bool isVisible, int maxDist)
        {
            // if no other or dead, don't.
            if (other == null || other.IsDead)
                return null;

            // if close enough and visible, wait there.
            int dist = game.Rules.GridDistance(m_Actor.Location.Position, otherPosition);
            if (isVisible && dist <= maxDist)
                return new ActionWait(m_Actor, game);

            // if in different map and standing on an exit that leads there, try to use the exit.
            if (other.Location.Map != m_Actor.Location.Map)
            {
                Exit exitThere = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
                if (exitThere != null && exitThere.ToMap == other.Location.Map)
                {
                    if (game.Rules.CanActorUseExit(m_Actor, m_Actor.Location.Position))
                        return new ActionUseExit(m_Actor, m_Actor.Location.Position, game);
                }
            }

            // try to get close.
            ActorAction bumpAction = BehaviorIntelligentBumpToward(game, otherPosition);
            if (bumpAction != null && bumpAction.IsLegal())
            {
                // run if other is running.
                if (other.IsRunning)
                    RunIfPossible(game.Rules);

                // done.
                return bumpAction;
            }
            
            // fail.
            return null;
        }

        protected ActorAction BehaviorHangAroundActor(RogueGame game, Actor other, Point otherPosition, int minDist, int maxDist)
        {
            // if no other or dead, don't.
            if (other == null || other.IsDead)
                return null;

            // pick a random spot around other within distance.
            Point hangSpot;
            int tries = 0;
            const int maxTries = 100;
            do
            {
                hangSpot = otherPosition;
                hangSpot.X += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
                hangSpot.Y += game.Rules.Roll(minDist, maxDist + 1) - game.Rules.Roll(minDist, maxDist + 1);
                m_Actor.Location.Map.TrimToBounds(ref hangSpot);
            }
            while (game.Rules.GridDistance(hangSpot, otherPosition) < minDist && ++tries < maxTries);
            
            // try to get close.
            ActorAction bumpAction = BehaviorIntelligentBumpToward(game, hangSpot);
            if (bumpAction != null && IsValidMoveTowardGoalAction(bumpAction) && bumpAction.IsLegal())
            {
                // run if other is running.
                if (other.IsRunning)
                    RunIfPossible(game.Rules);

                // done.
                return bumpAction;
            }

            // fail.
            return null;
        }
        #endregion

        #region Tracking scents
        protected ActorAction BehaviorTrackScent(RogueGame game, List<Percept> scents)
        {
            // if no scents, nothing to do.
            if (scents == null || scents.Count == 0)
                return null;

            // get highest scent.
            Percept best = FilterStrongestScent(game, scents);

            // 2 cases:
            // 1. Standing on best scent.
            // or
            // 2. Best scent is adjacent.
            #region
            Map map = m_Actor.Location.Map;
            // 1. Standing on best scent.
            if (m_Actor.Location.Position == best.Location.Position)
            {
                // if exit there and can and want to use it, do it.
                Exit exitThere = map.GetExitAt(m_Actor.Location.Position);
                if (exitThere != null && m_Actor.Model.Abilities.AI_CanUseAIExits)
                    return BehaviorUseExit(game, UseExitFlags.ATTACK_BLOCKING_ENEMIES | UseExitFlags.BREAK_BLOCKING_OBJECTS);
                else
                    return null;
            }

            // 2. Best scent is adjacent.
            // try to bump there.
            ActorAction bump = BehaviorIntelligentBumpToward(game, best.Location.Position);
            if (bump != null)
                return bump;
            #endregion

            // nope.
            return null;
        }
        #endregion

        #region Charging enemy
        protected ActorAction BehaviorChargeEnemy(RogueGame game, Percept target)
        {
            // try melee attack first.
            ActorAction attack = BehaviorMeleeAttack(game, target);
            if (attack != null)
                return attack;

            Actor enemy = target.Percepted as Actor;

            // if we are tired and next to enemy, use med or rest to recover our STA for the next attack.
            if (game.Rules.IsActorTired(m_Actor) && game.Rules.IsAdjacent(m_Actor.Location, target.Location))
            {
                // meds?
                ActorAction useMed = BehaviorUseMedecine(game, 0, 1, 0, 0, 0);
                if (useMed != null)
                    return useMed;

                // rest!
                return new ActionWait(m_Actor, game);
            }

            // then try getting closer.
            ActorAction bumpAction = BehaviorIntelligentBumpToward(game, target.Location.Position);
            if (bumpAction != null)
            {
                // do we rush? 
                // we want to rush if enemy has a range advantage, we want to get closer asap.
                if (m_Actor.CurrentRangedAttack.Range < enemy.CurrentRangedAttack.Range)
                    RunIfPossible(game.Rules);

                // chaaarge!
                return bumpAction;
            }

            // failed.
            return null;
        }
        #endregion

        #region Leading
        protected ActorAction BehaviorLeadActor(RogueGame game, Percept target)
        {
            Actor other = target.Percepted as Actor;

            // if can't lead him, fail.
            if (!game.Rules.CanActorTakeLead(m_Actor, other))
                return null;

            // if next to him, lead him.
            if (game.Rules.IsAdjacent(m_Actor.Location.Position, other.Location.Position))
            {
                return new ActionTakeLead(m_Actor, game, other);
            }

            // then try getting closer.
            ActorAction bumpAction = BehaviorIntelligentBumpToward(game, other.Location.Position);
            if (bumpAction != null)
                return bumpAction;

            // failed.
            return null;

        }

        protected ActorAction BehaviorDontLeaveFollowersBehind(RogueGame game, int distance, out Actor target)
        {
            target = null;

            // Scan the group:
            // - Find farthest member of the group.
            // - If at least half the group is close enough we consider the group cohesion to be good enough and do nothing.
            int worstDist = Int32.MinValue;
            Map map = m_Actor.Location.Map;
            Point myPos = m_Actor.Location.Position;
            int closeCount = 0;
            int halfGroup = m_Actor.CountFollowers / 2;
            foreach (Actor a in m_Actor.Followers)
            {
                // ignore actors on different map.
                if (a.Location.Map != map)
                    continue;

                // this actor close enough?
                if (game.Rules.GridDistance(a.Location.Position, myPos) <= distance)
                {
                    // if half close enough, nothing to do.
                    if (++closeCount >= halfGroup)
                        return null;
                }

                // farthest?
                int dist = game.Rules.GridDistance(a.Location.Position, myPos);
                if (target == null || dist > worstDist)
                {
                    target = a;
                    worstDist = dist;
                }
            }

            // try to move toward farther dude.
            if (target == null)
                return null;
            return BehaviorIntelligentBumpToward(game, target.Location.Position);
        }
        #endregion

        #region Fight or Flee
        /// <summary>
        /// 
        /// </summary>
        /// <param name="game"></param>
        /// <param name="enemies"></param>
        /// <param name="hasVisibleLeader"></param>
        /// <param name="isLeaderFighting"></param>
        /// <param name="courage"></param>
        /// <param name="emotes">0 = flee; 1 = trapped; 2 = charge</param>
        /// <returns></returns>
        protected ActorAction BehaviorFightOrFlee(RogueGame game, List<Percept> enemies, bool hasVisibleLeader, bool isLeaderFighting, ActorCourage courage,
            string[] emotes)
        {
            Percept nearestEnemy = FilterNearest(game, enemies);

            bool decideToFlee;
            bool doRun = false;  // don't run by default.

            Actor enemy = nearestEnemy.Percepted as Actor;

            // Cases that are a no brainer, in this order:
            // 1. Always fight if he has a ranged weapon.
            // 2. Always fight if law enforcer vs murderer.
            // 3. Always flee melee if tired.

            // 1. Always fight if enemy has ranged weapon.
            // if we are here, it means we can't shoot him, cause firing behavior has priority.
            // so we want to get a chance at melee a shooting enemy.
            if (HasEquipedRangedWeapon(enemy))
                decideToFlee = false;
            // 2. Always fight if law enforcer vs murderer.
            // do our duty.
            else if (m_Actor.Model.Abilities.IsLawEnforcer && enemy.MurdersCounter > 0)
                decideToFlee = false;
            // 3. Always flee melee if tired.
            else if (game.Rules.IsActorTired(m_Actor) && game.Rules.IsAdjacent(m_Actor.Location, enemy.Location))
                decideToFlee = true;
            // Case need more analysis.
            else
            {
                if (m_Actor.Leader != null)
                {
                    //////////////////////////
                    // Fighting with a leader.
                    //////////////////////////
                    #region
                    switch (courage)
                    {
                        case ActorCourage.COWARD:
                            // always flee and run.
                            decideToFlee = true;
                            doRun = true;
                            break;

                        case ActorCourage.CAUTIOUS:
                            // kite.
                            decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
                            doRun = !HasSpeedAdvantage(game, m_Actor, enemy);
                            break;

                        case ActorCourage.COURAGEOUS:
                            // fight if leader is fighting.
                            // otherwise kite.
                            if (isLeaderFighting)
                                decideToFlee = false;
                            else
                            {
                                decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
                                doRun = !HasSpeedAdvantage(game, m_Actor, enemy);
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("unhandled courage");
                    }
                    #endregion
                }
                else
                {
                    ////////////////////////
                    // Leaderless fighting.
                    ////////////////////////
                    #region
                    switch (courage)
                    {
                        case ActorCourage.COWARD:
                            // always flee and run.
                            decideToFlee = true;
                            doRun = true;
                            break;

                        case ActorCourage.CAUTIOUS:
                        case ActorCourage.COURAGEOUS:
                            // kite.
                            decideToFlee = WantToEvadeMelee(game, m_Actor, courage, enemy);
                            doRun = !HasSpeedAdvantage(game, m_Actor, enemy);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("unhandled courage");
                    }
                    #endregion
                }
            }

            // flee or fight?
            if (decideToFlee)
            {
                #region Flee
                ////////////////////////////////////////////////////////////////////////////////////////
                // Try to:
                // 1. Close door between me and the enemy if he can't open it right after we closed it.
                // 2. Barricade door between me and the enemy.
                // 3. Use exit?
                // 4. Use medecine?
                // 5. Walk/run away.
                // 6. Blocked, turn to fight.
                ////////////////////////////////////////////////////////////////////////////////////////

                // emote?
                if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_CHANCE))
                    game.DoEmote(m_Actor, String.Format("{0} {1}!", emotes[0], enemy.Name));

                // 1. Close door between me and the enemy if he can't open it right after we closed it.
                #region
                if (m_Actor.Model.Abilities.CanUseMapObjects)
                {
                    ChoiceEval<Direction> closeDoorBetweenDirection = Choose<Direction>(game, Direction.COMPASS_LIST,
                        (dir) =>
                        {
                            Point pos = m_Actor.Location.Position + dir;
                            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(pos) as DoorWindow;
                            if (door == null)
                                return false;
                            if (!IsBetween(game, m_Actor.Location.Position, pos, enemy.Location.Position))
                                return false;
                            if (!game.Rules.IsClosableFor(m_Actor, door))
                                return false;
                            if (game.Rules.GridDistance(pos, enemy.Location.Position) == 1 && game.Rules.IsClosableFor(enemy, door))
                                return false;
                            return true;
                        },
                        (dir) =>
                        {
                            return game.Rules.Roll(0, 666);  // random eval, all things being equal.
                        },
                        (a, b) => a > b);
                    if (closeDoorBetweenDirection != null)
                    {
                        return new ActionCloseDoor(m_Actor, game, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + closeDoorBetweenDirection.Choice) as DoorWindow);
                    }
                }
                #endregion

                // 2. Barricade door between me and the enemy.
                #region
                if (m_Actor.Model.Abilities.CanBarricade)
                {
                    ChoiceEval<Direction> barricadeDoorBetweenDirection = Choose<Direction>(game, Direction.COMPASS_LIST,
                        (dir) =>
                        {
                            Point pos = m_Actor.Location.Position + dir;
                            DoorWindow door = m_Actor.Location.Map.GetMapObjectAt(pos) as DoorWindow;
                            if (door == null)
                                return false;
                            if (!IsBetween(game, m_Actor.Location.Position, pos, enemy.Location.Position))
                                return false;
                            if (!game.Rules.CanActorBarricadeDoor(m_Actor, door))
                                return false;
                            return true;
                        },
                        (dir) =>
                        {
                            return game.Rules.Roll(0, 666);  // random eval, all things being equal.
                        },
                        (a, b) => a > b);
                    if (barricadeDoorBetweenDirection != null)
                    {
                        return new ActionBarricadeDoor(m_Actor, game, m_Actor.Location.Map.GetMapObjectAt(m_Actor.Location.Position + barricadeDoorBetweenDirection.Choice) as DoorWindow);
                    }
                }
                #endregion

                // 3. Use exit?
                #region
                if (m_Actor.Model.Abilities.AI_CanUseAIExits &&
                    game.Rules.RollChance(FLEE_THROUGH_EXIT_CHANCE))
                {
                    ActorAction useExit = BehaviorUseExit(game, UseExitFlags.NONE);
                    if (useExit != null)
                    {
                        bool doUseExit = true;

                        // Exception : if follower use exit only if leading to our leader.
                        // we don't want to leave our leader behind (mostly annoying for the player).
                        if (m_Actor.HasLeader)
                        {
                            Exit exitThere = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
                            if (exitThere != null) // well. who knows?
                                doUseExit = (m_Actor.Leader.Location.Map == exitThere.ToMap);
                        }

                        // do it?
                        if (doUseExit)
                        {
                            m_Actor.Activity = Activity.FLEEING;
                            return useExit;
                        }
                    }
                }
                #endregion

                // 4. Use medecine?
                #region
                // when to use medecine? only when fighting vs an unranged enemy and not in contact.
                ItemRangedWeapon rngEnemy = enemy.GetEquippedWeapon() as ItemRangedWeapon;
                if (rngEnemy == null && !game.Rules.IsAdjacent(m_Actor.Location, enemy.Location))
                {
                    ActorAction medAction = BehaviorUseMedecine(game, 2, 2, 1, 0, 0);
                    if (medAction != null)
                    {
                        m_Actor.Activity = Activity.FLEEING;
                        return medAction;
                    }
                }
                #endregion

                // 5. Walk/run away.
                #region
                ActorAction bumpAction = BehaviorWalkAwayFrom(game, enemies);
                if (bumpAction != null)
                {
                    if (doRun)
                        RunIfPossible(game.Rules);
                    m_Actor.Activity = Activity.FLEEING;
                    return bumpAction;
                }
                #endregion

                // 6. Blocked, turn to fight.
                #region
                if (bumpAction == null)
                {
                    // fight!
                    if (IsAdjacentToEnemy(game, enemy))
                    {
                        // emote?
                        if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_FLEE_TRAPPED_CHANCE))
                            game.DoEmote(m_Actor, emotes[1]);

                        return BehaviorMeleeAttack(game, nearestEnemy);
                    }
                }
                #endregion

                #endregion
            }
            else
            {
                #region Fight
                ActorAction attackAction = BehaviorChargeEnemy(game, nearestEnemy);
                if (attackAction != null)
                {
                    // emote?
                    if (m_Actor.Model.Abilities.CanTalk && game.Rules.RollChance(EMOTE_CHARGE_CHANCE))
                        game.DoEmote(m_Actor, String.Format("{0} {1}!", emotes[2], enemy.Name));

                    // chaaarge!
                    m_Actor.Activity = Activity.FIGHTING;
                    m_Actor.TargetActor = nearestEnemy.Percepted as Actor;
                    return attackAction;
                }
                #endregion
            }

            // nope.
            return null;
        }
        #endregion

        #region Communication
        protected ActorAction BehaviorWarnFriends(RogueGame game, List<Percept> friends, Actor nearestEnemy)
        {
            // Never if actor is itself adjacent to the enemy.
            if (game.Rules.IsAdjacent(m_Actor.Location, nearestEnemy.Location))
                return null;

            // Shout if leader is sleeping.
            // (kinda hax, but make followers more useful for players over phone)
            if (m_Actor.HasLeader && m_Actor.Leader.IsSleeping)
                return new ActionShout(m_Actor, game);

            // Shout if we have a friend sleeping.
            foreach (Percept p in friends)
            {
                Actor other = p.Percepted as Actor;
                if (other == null)
                    throw new ArgumentException("percept not an actor");
                if (other == null || other == m_Actor)
                    continue;
                if (!other.IsSleeping)
                    continue;
                if (game.Rules.IsEnemyOf(m_Actor, other))
                    continue;
                if (!game.Rules.IsEnemyOf(other, nearestEnemy))
                    continue;

                // friend sleeping, wake up!
                string shoutText = nearestEnemy == null ? String.Format("Wake up {0}!", other.Name) : String.Format("Wake up {0}! {1} sighted!", other.Name, nearestEnemy.Name);
                return new ActionShout(m_Actor, game, shoutText);
            }

            // no one to alert.
            return null;
        }

        protected ActorAction BehaviorTellFriendAboutPercept(RogueGame game, Percept percept)
        {
            // get an adjacent awake friend, if none nothing to do.
            Map map = m_Actor.Location.Map;
            List<Point> friends = map.FilterAdjacentInMap(m_Actor.Location.Position,
                (pt) =>
                {
                    Actor otherActor = map.GetActorAt(pt);
                    if (otherActor == null)
                        return false;
                    if (otherActor.IsSleeping)
                        return false;
                    if (game.Rules.IsEnemyOf(m_Actor, otherActor))
                        return false;
                    return true;
                });
            if (friends == null || friends.Count == 0)
                return null;

            // pick a random friend.
            Actor friend = map.GetActorAt(friends[game.Rules.Roll(0, friends.Count)]);

            // make message.
            string tellMsg;
            string whereMsg = MakeCentricLocationDirection(game, m_Actor.Location, percept.Location);
            string timeMsg = String.Format("{0} ago", WorldTime.MakeTimeDurationMessage(m_Actor.Location.Map.LocalTime.TurnCounter - percept.Turn));
            if (percept.Percepted is Actor)
            {
                Actor who = percept.Percepted as Actor;
                tellMsg = String.Format("I saw {0} {1} {2}.", who.Name, whereMsg, timeMsg);
            }
            else if (percept.Percepted is Inventory)
            {
                // tell about a random item from the pile.
                // warning: the items might have changed since then, the AI cheats a bit by knowing which items are there now.
                Inventory inv = percept.Percepted as Inventory;
                if (inv.IsEmpty)
                    return null;    // all items were taken or destroyed.
                Item what = inv[game.Rules.Roll(0, inv.CountItems)];

                // ignore worthless items (eg: don't spam about stupid items like planks)
                if (!IsItemWorthTellingAbout(what))
                    return null;

                // ignore stacks that are probably in plain view of the friend.
                int friendFOVRange = game.Rules.ActorFOV(friend, map.LocalTime, game.Session.World.Weather);
                if (percept.Location.Map == friend.Location.Map &&
                    game.Rules.StdDistance(percept.Location.Position, friend.Location.Position) <= 2 + friendFOVRange)
                {
                    return null;
                }

                // do it.
                tellMsg = String.Format("I saw {0} {1} {2}.", what.AName, whereMsg, timeMsg);
            }
            else if (percept.Percepted is String)
            {
                String raidDesc = percept.Percepted as String;
                tellMsg = String.Format("I heard {0} {1} {2}!", raidDesc, whereMsg, timeMsg);
            }
            else
                throw new InvalidOperationException("unhandled percept.Percepted type");

            // tell friend - if legal.
            ActionSay say = new ActionSay(m_Actor, game, friend, tellMsg, RogueGame.Sayflags.NONE);
            if (say.IsLegal())
                return say;
            else
                return null;
        }
        
        #endregion

        #region Exploring
        protected ActorAction BehaviorExplore(RogueGame game, ExplorationData exploration)
        {
            // prepare data.
            Direction prevDirection = Direction.FromVector(m_Actor.Location.Position.X - m_prevLocation.Position.X, m_Actor.Location.Position.Y - m_prevLocation.Position.Y);
            bool imStarvingOrCourageous = game.Rules.IsActorStarving(m_Actor) || Directives.Courage == ActorCourage.COURAGEOUS;

            // eval all adjacent tiles for exploration utility and get the best one.
            ChoiceEval<Direction> chooseExploreDir = Choose<Direction>(game,
                Direction.COMPASS_LIST,
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    if (exploration.HasExplored(next))
                        return false;
                    return IsValidMoveTowardGoalAction(game.Rules.IsBumpableFor(m_Actor, game, next));
                },
                (dir) =>
                {
                    Location next = m_Actor.Location + dir;
                    Map map = next.Map;
                    Point pos = next.Position;

                    // intelligent NPC: forbid stepping on deadly traps, unless starving or courageous (desperate).
                    if (m_Actor.Model.Abilities.IsIntelligent && !imStarvingOrCourageous)
                    {
                        int trapsDamage = ComputeTrapsMaxDamage(map, pos);
                        if (trapsDamage >= m_Actor.HitPoints)
                            return float.NaN;
                    }

                    // Heuristic scoring:
                    // 1st Prefer unexplored zones.
                    // 2nd Prefer unexplored locs.
                    // 3rd Prefer doors and barricades (doors/windows, pushables)
                    // 4th Punish stepping on activated traps.
                    // 5th Prefer inside during the night vs outside during the day.
                    // 6th Prefer continue in same direction.
                    // 7th Small randomness.
                    const int EXPLORE_ZONES = 1000;
                    const int EXPLORE_LOCS = 500;
                    const int EXPLORE_BARRICADES = 100;
                    const int AVOID_TRAPS = -50;
                    const int EXPLORE_INOUT = 50;
                    const int EXPLORE_DIRECTION = 25;
                    const int EXPLORE_RANDOM = 10;

                    int score = 0;
                    // 1st Prefer unexplored zones.
                    if (!exploration.HasExplored(map.GetZonesAt(pos.X, pos.Y)))
                        score += EXPLORE_ZONES;
                    // 2nd Prefer unexplored locs.
                    if (!exploration.HasExplored(next))
                        score += EXPLORE_LOCS;
                    // 3rd Prefer doors and barricades (doors/windows, pushables)
                    MapObject mapObj = map.GetMapObjectAt(pos);
                    if (mapObj != null && (mapObj.IsMovable || mapObj is DoorWindow))
                        score += EXPLORE_BARRICADES;
                    // 4th Punish stepping on activated traps.
                    if (IsAnyActivatedTrapThere(map, pos))
                        score += AVOID_TRAPS;
                    // 5th Prefer inside during the night vs outside during the day.
                    bool isInside = map.GetTileAt(pos.X, pos.Y).IsInside;
                    if (isInside)
                    {
                        if (map.LocalTime.IsNight)
                            score += EXPLORE_INOUT;
                    }
                    else
                    {
                        if (!map.LocalTime.IsNight)
                            score += EXPLORE_INOUT;
                    }
                    // 6th Prefer continue in same direction.
                    if (dir == prevDirection)
                        score += EXPLORE_DIRECTION;
                    // 7th Small randomness.
                    score += game.Rules.Roll(0, EXPLORE_RANDOM);

                    // done.
                    return score;
                },
                (a, b) => !float.IsNaN(a) && a > b);

            if (chooseExploreDir != null)
                return new ActionBump(m_Actor, game, chooseExploreDir.Choice);
            else
                return null;
        }
        #endregion

        #region Advanced movement
        protected ActorAction BehaviorCloseDoorBehindMe(RogueGame game, Location previousLocation)
        {
            // if we've gone through a door, try to close it.
            DoorWindow prevDoor = previousLocation.Map.GetMapObjectAt(previousLocation.Position) as DoorWindow;
            if (prevDoor == null)
                return null;
            if (game.Rules.IsClosableFor(m_Actor, prevDoor))
                return new ActionCloseDoor(m_Actor, game, prevDoor);

            // nope.
            return null;
        }

        protected ActorAction BehaviorSecurePerimeter(RogueGame game, HashSet<Point> fov)
        {
            /////////////////////////////////////
            // Secure room procedure:
            // 1. Close doors/windows.
            // 2. Barricade unbarricaded windows.
            /////////////////////////////////////
            Map map = m_Actor.Location.Map;

            foreach (Point pt in fov)
            {
                MapObject mapObj = map.GetMapObjectAt(pt);
                if (mapObj == null) 
                    continue;
                DoorWindow door = mapObj as DoorWindow;
                if (door == null)
                    continue;

                // 1. Close doors/windows.
                if (door.IsOpen && game.Rules.IsClosableFor(m_Actor, door))
                {
                    if (game.Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                        return new ActionCloseDoor(m_Actor, game, door);
                    else
                        return BehaviorIntelligentBumpToward(game, door.Location.Position);
                }

                // 2. Barricade unbarricaded windows.
                if (door.IsWindow && !door.IsBarricaded && game.Rules.CanActorBarricadeDoor(m_Actor,door))
                {
                    if (game.Rules.IsAdjacent(door.Location.Position, m_Actor.Location.Position))
                        return new ActionBarricadeDoor(m_Actor, game, door);
                    else
                        return BehaviorIntelligentBumpToward(game, door.Location.Position);                    
                }
            }

            // nothing to secure.
            return null;
        }
        #endregion

        #region Exits
        [Flags]
        protected enum UseExitFlags
        {
            /// <summary>
            /// Use only free exits.
            /// </summary>
            NONE = 0,

            /// <summary>
            /// Can try to break a blocking object.
            /// </summary>
            BREAK_BLOCKING_OBJECTS = (1 << 0),

            /// <summary>
            /// Can try to attack a blocking actor.
            /// </summary>
            ATTACK_BLOCKING_ENEMIES = (1 << 1),

            /// <summary>
            /// Do not use exit if we go back to our last location.
            /// </summary>
            DONT_BACKTRACK = (1 << 2)
        }

        /// <summary>
        /// Intelligent use of exit through flags : can prevent from backtracking, can attack object, can attack actor.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="useFlags">combination of flags.</param>
        /// <returns></returns>
        protected ActorAction BehaviorUseExit(RogueGame game, UseExitFlags useFlags)
        {
            // get exit at location, if none or ai flag not set, fail.
            Exit exit = m_Actor.Location.Map.GetExitAt(m_Actor.Location.Position);
            if (exit == null)
                return null;
            if (!exit.IsAnAIExit)
                return null;

            // don't backtrack?
            if ((useFlags & UseExitFlags.DONT_BACKTRACK) != 0)
            {
                if (exit.ToMap == m_prevLocation.Map && exit.ToPosition == m_prevLocation.Position)
                    return null;
            }

            // if exit blocked by an enemy and want to attack it, do it.
            if ((useFlags & UseExitFlags.ATTACK_BLOCKING_ENEMIES) != 0)
            {
                Actor blockingActor = exit.ToMap.GetActorAt(exit.ToPosition);
                if (blockingActor != null && game.Rules.IsEnemyOf(m_Actor, blockingActor) && game.Rules.CanActorMeleeAttack(m_Actor, blockingActor))
                    return new ActionMeleeAttack(m_Actor, game, blockingActor);
            }

            // if exit blocked by a breakable object and want to bash, do it.
            if ((useFlags & UseExitFlags.BREAK_BLOCKING_OBJECTS) != 0)
            {
                MapObject blockingObj = exit.ToMap.GetMapObjectAt(exit.ToPosition);
                if (blockingObj != null && game.Rules.IsBreakableFor(m_Actor, blockingObj))
                    return new ActionBreak(m_Actor, game, blockingObj);
            }

            // if using exit is illegal, fail.
            if (!game.Rules.CanActorUseExit(m_Actor, m_Actor.Location.Position))
                return null;

            // use the exit.
            return new ActionUseExit(m_Actor, m_Actor.Location.Position, game);
        }
        #endregion

        #region Explosives
        protected ActorAction BehaviorFleeFromExplosives(RogueGame game, List<Percept> itemStacks)
        {
            // if no items in view, don't bother.
            if (itemStacks == null || itemStacks.Count == 0)
                return null;

            // filter stacks that have primed explosives.
            List<Percept> primedExplosives = Filter(game, itemStacks, 
                (p) =>
                {
                    Inventory stack = p.Percepted as Inventory;
                    if (stack == null || stack.IsEmpty)
                        return false;
                    foreach (Item it in stack.Items)
                    {
                        ItemPrimedExplosive explosive = it as ItemPrimedExplosive;
                        if (explosive == null)
                            continue;
                        // found a primed explosive.
                        return true;
                    }
                    // no primed explosive in this stack.
                    return false;
                });

            // if no primed explosive in sight, no worries.
            if (primedExplosives == null || primedExplosives.Count == 0)
                return null;

            // run away from primed explosives!
            ActorAction runAway = BehaviorWalkAwayFrom(game, primedExplosives);
            if (runAway == null)
                return null;
            RunIfPossible(game.Rules);
            return runAway;
        }

        protected ActorAction BehaviorThrowGrenade(RogueGame game, HashSet<Point> fov, List<Percept> enemies)
        {
            // don't bother if no enemies.
            if (enemies == null || enemies.Count == 0)
                return null;

            // only throw if enough enemies.
            if (enemies.Count < 3)
                return null;

            // don't bother if no grenade in inventory.
            Inventory inv = m_Actor.Inventory;
            if (inv == null || inv.IsEmpty)
                return null;
            ItemGrenade grenade = GetFirstGrenade((it) => !IsItemTaboo(it));
            if (grenade == null)
                return null;
            ItemGrenadeModel model = grenade.Model as ItemGrenadeModel;

            // find the best throw point : a spot with many enemies around and no friend to hurt.
            #region
            int maxThrowRange = game.Rules.ActorMaxThrowRange(m_Actor, model.MaxThrowDistance);
            Point? bestSpot = null;
            int bestSpotScore = 0;
            foreach (Point pt in fov)
            {
                // never throw within blast radius - don't suicide ^^
                if (game.Rules.GridDistance(m_Actor.Location.Position, pt) <= model.BlastAttack.Radius)
                    continue;

                // if we can't throw there, don't bother.
                if (game.Rules.GridDistance(m_Actor.Location.Position, pt) > maxThrowRange)
                    continue;
                if (!LOS.CanTraceThrowLine(m_Actor.Location, pt, maxThrowRange, null))
                    continue;

                // compute interest of throwing there.
                // - pro: number of enemies within blast radius.
                // - cons: friend in radius.
                // don't bother checking for blast wave actuallly reaching the targets.
                int score = 0;
                for (int x = pt.X - model.BlastAttack.Radius; x <= pt.X + model.BlastAttack.Radius; x++)
                    for (int y = pt.Y - model.BlastAttack.Radius; y <= pt.Y + model.BlastAttack.Radius; y++)
                    {
                        if (!m_Actor.Location.Map.IsInBounds(x, y))
                            continue;
                        Actor otherActor = m_Actor.Location.Map.GetActorAt(x, y);
                        if (otherActor == null)
                            continue;
                        if (otherActor == m_Actor)
                            continue;
                        int blastDistToTarget = game.Rules.GridDistance(pt, otherActor.Location.Position);
                        if (blastDistToTarget > model.BlastAttack.Radius)
                            continue;

                        // other actor within blast radius.
                        // - if friend, abort and never throw there.
                        // - if enemy, increase score.
                        if (game.Rules.IsEnemyOf(m_Actor, otherActor))
                        {
                            // score = damage inflicted vs target toughness(max hp).
                            // -> means it is better to hurt badly one big enemy than a few scratch on a group of weaklings.
                            int value = game.Rules.BlastDamage(blastDistToTarget, model.BlastAttack) * game.Rules.ActorMaxHPs(otherActor);
                            score += value;
                        }
                        else
                        {
                            score = -1;
                            break;
                        }
                    }

                // if negative score (eg: friends get hurt), don't throw.
                if (score <= 0)
                    continue;

                // possible spot. best one?
                if (bestSpot == null || score > bestSpotScore)
                {
                    bestSpot = pt;
                    bestSpotScore = score;
                }
            }
            #endregion

            // if no throw point, don't.
            if (bestSpot == null)
                return null;

            // equip then throw.
            if (!grenade.IsEquipped)
            {
                Item otherEquipped = m_Actor.GetEquippedWeapon();
                if (otherEquipped != null)
                    return new ActionUnequipItem(m_Actor, game, otherEquipped);
                else
                    return new ActionEquipItem(m_Actor, game, grenade);
            }
            else
            {
                ActorAction throwAction = new ActionThrowGrenade(m_Actor, game, bestSpot.Value);
                if (!throwAction.IsLegal())
                    return null;
                return throwAction;
            }
        }
        #endregion

        #region Inventory management
        protected ActorAction BehaviorMakeRoomForFood(RogueGame game, List<Percept> stacks)
        {
            // if no items in view, fail.
            if (stacks == null || stacks.Count == 0)
                return null;

            // if inventory not full, no need.
            int maxInv = game.Rules.ActorMaxInv(m_Actor);
            if (m_Actor.Inventory.CountItems < maxInv)
                return null;

            // if food item in inventory, no need.
            if (HasItemOfType(typeof(ItemFood)))
                return null;

            // if no food item in view, fail.
            bool hasFoodVisible = false;
            foreach (Percept p in stacks)
            {
                Inventory inv = p.Percepted as Inventory;
                if (inv == null)
                    continue;

                if (inv.HasItemOfType(typeof(ItemFood)))
                {
                    hasFoodVisible = true;
                    break;
                }
            }
            if (!hasFoodVisible)
                return null;

            // want to get rid of an item.
            // order of preference:
            // 1. get rid of not interesting item.
            // 2. get rid of barricading material.
            // 3. get rid of light & sprays.
            // 4. get rid of ammo.
            // 5. get rid of medecine.
            // 6. last resort, get rid of random item.
            Inventory myInv = m_Actor.Inventory;

            // 1. get rid of not interesting item.
            Item notInteresting = myInv.GetFirstMatching((it) => !IsInterestingItem(game, it));
            if (notInteresting != null)
                return BehaviorDropItem(game, notInteresting);

            // 2. get rid of barricading material.
            Item material = myInv.GetFirstMatching((it) => it is ItemBarricadeMaterial);
            if (material != null)
                return BehaviorDropItem(game, material);

            // 3. get rid of light & sprays.
            Item light = myInv.GetFirstMatching((it) => it is ItemLight);
            if (light != null)
                return BehaviorDropItem(game, light);
            Item spray = myInv.GetFirstMatching((it) => it is ItemSprayPaint);
            if (spray != null)
                return BehaviorDropItem(game, spray);
            spray = myInv.GetFirstMatching((it) => it is ItemSprayScent);
            if (spray != null)
                return BehaviorDropItem(game, spray);

            // 4. get rid of ammo.
            Item ammo = myInv.GetFirstMatching((it) => it is ItemAmmo);
            if (ammo != null)
                return BehaviorDropItem(game, ammo);

            // 5. get rid of medecine.
            Item med = myInv.GetFirstMatching((it) => it is ItemMedicine);
            if (med != null)
                return BehaviorDropItem(game, med);

            // 6. last resort, get rid of random item.
            Item anyItem = myInv[game.Rules.Roll(0, myInv.CountItems)];
            return BehaviorDropItem(game, anyItem);
        }
        #endregion

        #region Sprays
        protected ActorAction BehaviorUseStenchKiller(RogueGame game)
        {
            ItemSprayScent spray = m_Actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemSprayScent;

            // if no spray or empty, nope.
            if (spray == null)
                return null;
            if (spray.SprayQuantity <= 0)
                return null;
            // if not proper odor, nope.
            ItemSprayScentModel model = spray.Model as ItemSprayScentModel;
            if (model.Odor != Odor.PERFUME_LIVING_SUPRESSOR)
                return null;

            // spot must be interesting to spray.
            if (!IsGoodStenchKillerSpot(game, m_Actor.Location.Map, m_Actor.Location.Position))
                return null;

            // good spot, try to do it.
            ActionUseItem sprayIt = new ActionUseItem(m_Actor, game, spray);
            if (sprayIt.IsLegal())
                return sprayIt;

            // nope.
            return null;
        }

        protected bool IsGoodStenchKillerSpot(RogueGame game, Map map, Point pos)
        {
            // 1. Don't spray at an already sprayed spot.
            // 2. Spray in a good position:
            //    2.1 entering or leaving a building.
            //    2.2 a door/window.
            //    2.3 an exit.

            // 1. Don't spray at an already sprayed spot.
            if (map.GetScentByOdorAt(Odor.PERFUME_LIVING_SUPRESSOR, pos) > 0)
                return false;

            // 2. Spray in a good position:
            
            //    2.1 entering or leaving a building.
            bool wasInside = m_prevLocation.Map.GetTileAt(m_prevLocation.Position).IsInside;
            bool isInside  = map.GetTileAt(pos).IsInside;
            if (wasInside != isInside)
                return true;
            //    2.2 a door/window.
            MapObject objThere = map.GetMapObjectAt(pos);
            if (objThere != null && objThere is DoorWindow)
                return true;
            //    2.3 an exit.
            if (map.GetExitAt(pos) != null)
                return true;

            // nope.
            return false;
        }
        #endregion

        #region Law enforcement
        protected ActorAction BehaviorEnforceLaw(RogueGame game, List<Percept> percepts, out Actor target)
        {
            target = null;

            // sanity checks.
            if (!m_Actor.Model.Abilities.IsLawEnforcer)
                return null;
            if (percepts == null)
                return null;
            
            // filter murderers that are not our enemies yet.
            List<Percept> murderers = FilterActors(game, percepts,
                (a) => a.MurdersCounter > 0 && !game.Rules.IsEnemyOf(m_Actor, a));

            // if none, nothing to do.
            if (murderers == null || murderers.Count == 0)
                return null;

            // get nearest murderer.
            Percept nearestMurderer = FilterNearest(game, murderers);
            target = nearestMurderer.Percepted as Actor;

            // roll against target unsuspicious skill.
            if (game.Rules.RollChance(game.Rules.ActorUnsuspicousChance(m_Actor, target)))
            {
                // emote.
                game.DoEmote(target, String.Format("moves unnoticed by {0}.", m_Actor.Name));

                // done.
                return null;
            }

            // mmmmhhh. who's that?
            game.DoEmote(m_Actor, String.Format("takes a closer look at {0}.", target.Name));

            // then roll chance to spot and recognize him as murderer.
            int spotChance = game.Rules.ActorSpotMurdererChance(m_Actor, target);

            // if did not spot, nothing to do.
            if (!game.Rules.RollChance(spotChance))
                return null;

            // make him our enemy and tell him!
            game.DoMakeAggression(m_Actor, target);
            return new ActionSay(m_Actor, game, target, 
                String.Format("HEY! YOU ARE WANTED FOR {0} MURDER{1}!", target.MurdersCounter, target.MurdersCounter > 1 ? "s" : ""), RogueGame.Sayflags.IS_IMPORTANT);
        }
        #endregion

        #region Animals
        protected ActorAction BehaviorGoEatFoodOnGround(RogueGame game, List<Percept> stacksPercepts)
        {
            // nope if no percepts.
            if (stacksPercepts == null)
                return null;

            // filter stacks with food.
            List<Percept> foodStacks = Filter(game, stacksPercepts, (p) =>
            {
                Inventory inv = p.Percepted as Inventory;
                return inv.HasItemOfType(typeof(ItemFood));
            });

            // nope if no food stacks.
            if (foodStacks == null)
                return null;

            // either 1) eat there or 2) go get it

            // 1) check food here.
            Inventory invThere = m_Actor.Location.Map.GetItemsAt(m_Actor.Location.Position);
            if (invThere != null && invThere.HasItemOfType(typeof(ItemFood)))
            {
                // eat the first food we get.
                Item eatIt = invThere.GetFirstByType(typeof(ItemFood));
                return new ActionEatFoodOnGround(m_Actor, game, eatIt);
            }
            // 2) go to nearest food.
            Percept nearest = FilterNearest(game, foodStacks);
            return BehaviorStupidBumpToward(game, nearest.Location.Position);
        }
        #endregion

        #region Corpses & Revival
        protected ActorAction BehaviorGoEatCorpse(RogueGame game, List<Percept> corpsesPercepts)
        {
            // nope if no percepts.
            if (corpsesPercepts == null)
                return null;

            // if undead, must need health.
            if (m_Actor.Model.Abilities.IsUndead && m_Actor.HitPoints >= game.Rules.ActorMaxHPs(m_Actor))
                return null;

            // either 1) eat corpses or 2) go get them.

            // 1) check corpses here.
            List<Corpse> corpses = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
            if (corpses != null)
            {
                // eat the first corpse.
                Corpse eatIt = corpses[0];
                if (game.Rules.CanActorEatCorpse(m_Actor, eatIt))
                    return new ActionEatCorpse(m_Actor, game, eatIt);
            }
            // 2) go to nearest corpses.
            Percept nearest = FilterNearest(game, corpsesPercepts);
            return m_Actor.Model.Abilities.IsIntelligent ? 
                    BehaviorIntelligentBumpToward(game, nearest.Location.Position) : 
                    BehaviorStupidBumpToward(game, nearest.Location.Position);
        }

        /// <summary>
        /// TrRy to revive non-enemy corpses.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="corpsesPercepts"></param>
        /// <returns></returns>
        protected ActorAction BehaviorGoReviveCorpse(RogueGame game, List<Percept> corpsesPercepts)
        {
            // nope if no percepts.
            if (corpsesPercepts == null)
                return null;

            // make sure we have the basics : medic skill & medikit item.
            if (m_Actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) == 0)
                return null;
            if (!HasItemOfModel(game.GameItems.MEDIKIT))
                return null;

            // keep only corpses stacks where we can revive at least one corpse.
            List<Percept> revivables = Filter(game, corpsesPercepts, (p) =>
                {
                    List<Corpse> corpsesThere = p.Percepted as List<Corpse>;
                    foreach (Corpse c in corpsesThere)
                    {
                        // dont revive enemies!
                        if (game.Rules.CanActorReviveCorpse(m_Actor, c) && !game.Rules.IsEnemyOf(m_Actor,c.DeadGuy))
                            return true;
                    }
                    return false;
                });
            if (revivables == null)
                return null;

            // either 1) revive corpse or 2) go get them.

            // 1) check corpses here.
            List<Corpse> corpses = m_Actor.Location.Map.GetCorpsesAt(m_Actor.Location.Position);
            if (corpses != null)
            {
                // get the first corpse we can revive.
                foreach (Corpse c in corpses)
                {
                    if (game.Rules.CanActorReviveCorpse(m_Actor, c) && !game.Rules.IsEnemyOf(m_Actor,c.DeadGuy))
                        return new ActionReviveCorpse(m_Actor, game, c);
                }
            }
            // 2) go to nearest revivable.
            Percept nearest = FilterNearest(game, revivables);
            return m_Actor.Model.Abilities.IsIntelligent ?
                    BehaviorIntelligentBumpToward(game, nearest.Location.Position) :
                    BehaviorStupidBumpToward(game, nearest.Location.Position);
        }

        #endregion

        #endregion

        #region Behaviors helpers

        #region Messages
        string MakeCentricLocationDirection(RogueGame game, Location from, Location to)
        {
            // if not same location, just says the map.
            if (from.Map != to.Map)
            {
                return String.Format("in {0}", to.Map.Name);
            }

            // same location, says direction.
            Point fromPos = from.Position;
            Point toPos = to.Position;
            Point vDir = new Point(toPos.X - fromPos.X, toPos.Y - fromPos.Y);
            return String.Format("{0} tiles to the {1}", (int)game.Rules.StdDistance(vDir), Direction.ApproximateFromVector(vDir));
        }
        #endregion

        #region Items

        protected bool IsItemWorthTellingAbout(Item it)
        {
            if (it == null)
                return false;

            // items type to ignore:
            // - barricading material (planks drop a lot).
            if (it is ItemBarricadeMaterial)
                return false;

            // ignore items we are carrying (we have seen it then taken it)
            if (m_Actor.Inventory != null && !m_Actor.Inventory.IsEmpty && m_Actor.Inventory.Contains(it))
                return false;

            // ok.
            return true;
        }

        protected Item GetEquippedWeapon()
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.IsEquipped && it is ItemWeapon)
                    return it;

            return null;
        }

        protected Item GetBestRangedWeaponWithAmmo(Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            Item best = null;
            int bestSc = 0;
            foreach (Item it in m_Actor.Inventory.Items)
            {
                ItemRangedWeapon w = it as ItemRangedWeapon;
                if (w != null && (fn == null || fn(it)))
                {
                    bool checkIt = false;
                    if (w.Ammo > 0)
                    {
                        checkIt = true;
                    }
                    else
                    {
                        // out of ammo, but do we have a matching ammo item in inventory we could reload it with?
                        foreach (Item itReload in m_Actor.Inventory.Items)
                        {
                            if (itReload is ItemAmmo && (fn == null || fn(itReload)))
                            {
                                ItemAmmo itAmmo = itReload as ItemAmmo;
                                if (itAmmo.AmmoType == w.AmmoType)
                                {
                                    checkIt = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (checkIt)
                    {
                        int sc = ScoreRangedWeapon(w);
                        if (best == null || sc > bestSc)
                        {
                            best = w;
                            bestSc = sc;
                        }

                    }
                }
            }

            return best;
        }

        protected int ScoreRangedWeapon(ItemRangedWeapon w)
        {
            ItemRangedWeaponModel m = w.Model as ItemRangedWeaponModel;
            return 1000 * m.Attack.Range + m.Attack.DamageValue;
        }

        protected Item GetFirstMeleeWeapon(Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemMeleeWeapon && (fn == null || fn(it)))
                    return it;
            }

            return null;
        }

        protected Item GetFirstBodyArmor(Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemBodyArmor && (fn == null || fn(it)))
                    return it;
            }

            return null;
        }

        protected ItemGrenade GetFirstGrenade(Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemGrenade && (fn == null || fn(it)))
                    return it as ItemGrenade;
            }

            return null;
        }

        protected Item GetEquippedBodyArmor()
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.IsEquipped && it is ItemBodyArmor)
                    return it;

            return null;
        }

        protected Item GetEquippedCellPhone()
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.IsEquipped && it is ItemTracker)
                {
                    ItemTracker t = it as ItemTracker;
                    if (t.CanTrackFollowersOrLeader)
                        return it;
                }

            return null;
        }

        protected Item GetFirstTracker(Predicate<ItemTracker> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                ItemTracker t = it as ItemTracker;
                if (t != null && (fn == null || fn(t)))
                    return it;
            }

            return null;
        }

        protected Item GetEquippedLight()
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.IsEquipped && it is ItemLight)
                    return it;

            return null;
        }

        protected Item GetFirstLight(Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemLight && (fn == null || fn(it)))
                    return it;
            }

            return null;
        }

        protected ItemSprayScent GetEquippedStenchKiller()
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.IsEquipped && it is ItemSprayScent)
                {
                    ItemSprayScentModel m = (it as ItemSprayScent).Model as ItemSprayScentModel;
                    if (m.Odor == Odor.PERFUME_LIVING_SUPRESSOR)
                        return it as ItemSprayScent;
                }

            return null;
        }

        protected ItemSprayScent GetFirstStenchKiller(Predicate<ItemSprayScent> fn)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemSprayScent && (fn == null || fn(it as ItemSprayScent)))
                    return it as ItemSprayScent;
            }

            return null;
        }

        protected bool IsRangedWeaponOutOfAmmo(Item it)
        {
            ItemRangedWeapon w = it as ItemRangedWeapon;
            if (w == null)
                return false;
            return w.Ammo <= 0;
        }

        protected bool IsLightOutOfBatteries(Item it)
        {
            ItemLight l = it as ItemLight;
            if (l == null)
                return false;
            return l.Batteries <= 0;
        }

        protected Item GetBestEdibleItem(RogueGame game)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return null;

            int turn = m_Actor.Location.Map.LocalTime.TurnCounter;
            int need = game.Rules.ActorMaxFood(m_Actor) - m_Actor.FoodPoints;
            Item bestFood = null;
            int bestScore = int.MinValue;
            foreach (Item it in m_Actor.Inventory.Items)
            {
                ItemFood foodIt = it as ItemFood;
                if (foodIt == null)
                    continue;

                // compute heuristic score.
                // - economize food : punish food wasting, the more waste the worse.
                // - keep non-perishable food : punish eating non-perishable food, the more nutrition the worse.
                int score = 0;

                int nutrition = game.Rules.FoodItemNutrition(foodIt, turn);
                int waste = nutrition - need;

                // - punish food wasting, the more waste the worse.
                if (waste > 0)
                    score -= waste;

                // - punish eating non-perishable food, the more nutrition the worse.
                if (!foodIt.IsPerishable)
                    score -= nutrition;

                // best?
                if (bestFood == null || score > bestScore)
                {
                    bestFood = foodIt;
                    bestScore = score;
                }
            }

            // return best.
            return bestFood;
        }

        public bool IsInterestingItem(RogueGame game, Item it)
        {
            /////////////////////////////////////////////////////////////////////////////
            // Interesting items:
            // 0 Reject anything not food if only one slot left.
            // 1 Reject forbidden items.
            // 2 Reject spray paint.
            // 3 Reject activated traps.
            // 4 Food.
            // 5 Ranged weapons.
            // 6 Ammo.
            // 7 Other Weapons, Medicine.
            // 8 Lights.
            // 9 Reject primed explosives!
            // 10 Reject boring items.
            // 11 Rest.
            /////////////////////////////////////////////////////////////////////////////

            bool onlyOneSlotLeft = (m_Actor.Inventory.CountItems == game.Rules.ActorMaxInv(m_Actor) - 1);

            // 0 Reject anything not food if only one slot left.
            if (onlyOneSlotLeft)
            {
                if (it is ItemFood)
                    return true;
                else
                    return false;
            }

            // 1 Reject forbidden items.
            if (it.IsForbiddenToAI)
                return false;

            // 2 Reject spray paint.
            if (it is ItemSprayPaint)
                return false;

            // 3 Reject activated traps.
            if (it is ItemTrap)
            {
                if ((it as ItemTrap).IsActivated)
                    return false;
            }

            // 4 Food
            if (it is ItemFood)
            {
                // accept any food if hungry.
                if (game.Rules.IsActorHungry(m_Actor))
                    return true;

                bool hasEnoughFood = HasEnoughFoodFor(game, m_Actor.Sheet.BaseFoodPoints / 2);

                // food not urgent, only interested in not spoiled food and if need more.
                return !hasEnoughFood && !game.Rules.IsFoodSpoiled(it as ItemFood, m_Actor.Location.Map.LocalTime.TurnCounter);
            }

            // 5 Ranged weapons.
            // Reject is AI_NotInterestedInRangedWeapons flag set.
            // Reject empty if no matching ammo, not already 2 ranged weapons in inventory, and different than any weapon we already have.
            if (it is ItemRangedWeapon)
            {
                // ai flag.
                if (m_Actor.Model.Abilities.AI_NotInterestedInRangedWeapons)
                    return false;

                ItemRangedWeapon rw = it as ItemRangedWeapon;
                // empty and no matching ammo : no.
                if (rw.Ammo <= 0 && GetCompatibleAmmoItem(game, rw) == null)
                    return false;

                // already 1 ranged weapon = no
                if (CountItemsOfSameType(typeof(ItemRangedWeapon)) >= 1)
                    return false;

                // new item but same as a weapon we already have = no
                if (!m_Actor.Inventory.Contains(it) && HasItemOfModel(it.Model))
                    return false;

                // all clear, me want!
                return true;
            }

            // 6 Ammo : only if has matching weapon and if has less than two full stacks.
            if (it is ItemAmmo)
            {
                ItemAmmo am = it as ItemAmmo;
                if (GetCompatibleRangedWeapon(game, am) == null)
                    return false;
                return !HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }

            // 7 Melee weapons, Medecine
            // Reject melee weapons if we are skilled in martial arts or we alreay have 2.
            // Reject medecine if we alredy have full stacks.
            if (it is ItemMeleeWeapon)
            {
                // martial artists ignore melee weapons.
                if (m_Actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MARTIAL_ARTS) > 0)
                    return false;
                // only two melee weapons max.
                int nbMeleeWeaponsInInventory = CountItemQuantityOfType(typeof(ItemMeleeWeapon));
                return nbMeleeWeaponsInInventory < 2;
            }            
            if(it is ItemMedicine)
            {
                return !HasAtLeastFullStackOfItemTypeOrModel(it, 2);
            }

            // 8 Lights : ignore out of batteries.
            if (IsLightOutOfBatteries(it))
                return false;

            // 9 Reject primed explosives!
            if (it is ItemPrimedExplosive)
                return false;

            // 10 Reject boring items.
            if (m_Actor.IsBoredOf(it))
                return false;

            // 11 Rest : if has less than one full stack.
            return !HasAtLeastFullStackOfItemTypeOrModel(it, 1);
        }

        public bool HasAnyInterestingItem(RogueGame game, Inventory inv)
        {
            if (inv == null)
                return false;
            foreach (Item it in inv.Items)
                if (IsInterestingItem(game, it))
                    return true;
            return false;
        }

        protected Item FirstInterestingItem(RogueGame game, Inventory inv)
        {
            if (inv == null)
                return null;
            foreach (Item it in inv.Items)
                if (IsInterestingItem(game, it))
                    return it;
            return null;
        }

        protected bool HasEnoughFoodFor(RogueGame game, int nutritionNeed)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return false;

            int turnCounter = m_Actor.Location.Map.LocalTime.TurnCounter;
            int nutritionTotal = 0;
            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it is ItemFood)
                {
                    nutritionTotal += game.Rules.FoodItemNutrition(it as ItemFood, turnCounter);
                    if (nutritionTotal >= nutritionNeed) // exit asap
                        return true;
                }
            }

            return false;
        }

        protected bool HasAtLeastFullStackOfItemTypeOrModel(Item it, int n)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return false;

            if (it.Model.IsStackable)
            {
                // we want N stacks of it.
                return CountItemsQuantityOfModel(it.Model) >= n * it.Model.StackingLimit;
            }
            else
            {
                // not stackable, we are happy with N items of its type.
                return CountItemsOfSameType(it.GetType()) >= n;
            }
        }

        protected bool HasItemOfModel(ItemModel model)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return false;

            foreach (Item it in m_Actor.Inventory.Items)
                if (it.Model == model)
                    return true;

            return false;
        }

        protected int CountItemsQuantityOfModel(ItemModel model)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return 0;

            int count = 0;
            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (it.Model == model)
                    count += it.Quantity;
            }

            return count;
        }

        protected bool HasItemOfType(Type tt)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return false;

            return m_Actor.Inventory.HasItemOfType(tt);
        }

        protected int CountItemQuantityOfType(Type tt)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return 0;

            int quantity = 0;
            foreach (Item otherIt in m_Actor.Inventory.Items)
            {
                if (otherIt.GetType() == tt)
                    quantity += otherIt.Quantity;
            }

            return quantity;
        }

        protected int CountItemsOfSameType(Type tt)
        {
            if (m_Actor.Inventory == null || m_Actor.Inventory.IsEmpty)
                return 0;

            int count = 0;
            foreach (Item otherIt in m_Actor.Inventory.Items)
            {
                if (otherIt.GetType() == tt)
                    ++count;
            }

            return count;
        }

        #endregion

        #region Running
        protected void RunIfPossible(Rules rules)
        {
            if (rules.CanActorRun(m_Actor))
                m_Actor.IsRunning = true;
        }
        #endregion

        #region Distances & Safety
        protected int GridDistancesSum(Rules rules, Point from, List<Percept> goals)
        {
            int sum = 0;
            foreach (Percept to in goals)
                sum += rules.GridDistance(from, to.Location.Position);
            return sum;
        }

        /// <summary>
        /// Compute safety from a list of dangers at a given position.
        /// </summary>
        /// <param name="rules"></param>
        /// <param name="from">position to compute the safety</param>
        /// <param name="dangers">dangers to avoid</param>
        /// <returns>a heuristic value, the higher the better the safety from the dangers</returns>
        protected float SafetyFrom(Rules rules, Point from, List<Percept> dangers)
        {
            Map map = m_Actor.Location.Map;

            // Heuristics:
            // Primary: Get away from dangers.
            // Weighting factors:
            // 1 Avoid getting in corners.
            // 2 Prefer going outside/inside if majority of dangers are inside/outside.
            // 3 If can tire, prefer not jumping.

            // Primary: Get away from dangers.
            #region
            float avgDistance = GridDistancesSum(rules, from, dangers) / (1 + dangers.Count);
            #endregion

            // 1 Avoid getting in corners.
            #region
            int countFreeSquares = 0;
            foreach (Direction d in Direction.COMPASS)
            {
                Point to = from + d;
                if (to == m_Actor.Location.Position || rules.IsWalkableFor(m_Actor, map, to.X, to.Y))
                    ++countFreeSquares;
            }
            float avoidCornerBonus = countFreeSquares * 0.1f;   // [0,+0.8]
            #endregion

            // 2 Prefer going outside/inside if majority of dangers are inside/outside.
            #region
            bool isFromInside = map.GetTileAt(from).IsInside;
            int majorityDangersInside = 0;
            foreach (Percept p in dangers)
            {
                if (map.GetTileAt(p.Location.Position).IsInside)
                    ++majorityDangersInside;
                else
                    --majorityDangersInside;
            }
            const float inOutFactor = 1.25f;
            float inOutBonus = 0;
            if (isFromInside)
            {
                // from is inside, want that if majority dangers are outside.
                if (majorityDangersInside < 0) inOutBonus = inOutFactor;
            }
            else
            {
                // from is outside, want that if majority dangers are inside.                              
                if (majorityDangersInside > 0) inOutBonus = inOutFactor;
            }
            #endregion

            // 3 If can tire, prefer not jumping.
            #region
            float jumpPenalty = 0;
            if (m_Actor.Model.Abilities.CanTire && m_Actor.Model.Abilities.CanJump)
            {
                MapObject obj = map.GetMapObjectAt(from);
                if (obj != null && obj.IsJumpable)
                    jumpPenalty = 0.1f;
            }
            #endregion

            // Final Safety = getting away * heuristics weights.
            float heursticFactorBonus = 1f + avoidCornerBonus + inOutBonus - jumpPenalty;
            return avgDistance * heursticFactorBonus;
        }
        #endregion

        #region Choice making
        protected ChoiceEval<_T_> Choose<_T_>(RogueGame game, List<_T_> listOfChoices, 
            Func<_T_, bool> isChoiceValidFn,
            Func<_T_, float> evalChoiceFn, 
            Func<float, float, bool> isBetterEvalThanFn)
        {
            //Console.Out.WriteLine("Evaluating choices");

            // Degenerate cases.
            if (listOfChoices.Count == 0)
            {
                //Console.Out.WriteLine("no choice.");
                return null;
            }

            // Find valid choices and best value.
            bool hasValue = false;
            float bestValue = 0;    // irrevelant for 1st value, use flag hasValue instead.
            List<ChoiceEval<_T_>> validChoices = new List<ChoiceEval<_T_>>(listOfChoices.Count);
            for(int i = 0; i < listOfChoices.Count; i++)
            {
                if(!isChoiceValidFn(listOfChoices[i]))
                    continue;

                float value_i = evalChoiceFn(listOfChoices[i]);
                if (float.IsNaN(value_i))
                    continue;

                validChoices.Add(new ChoiceEval<_T_>(listOfChoices[i], value_i));

                if (!hasValue || isBetterEvalThanFn(value_i, bestValue))
                {
                    hasValue = true;
                    bestValue = value_i;
                }
            }

            /*Console.Out.WriteLine("Evals {");
            for (int j = 0; j < validChoices.Count; j++)
            {
                Console.Out.WriteLine("  {0}", validChoices[j].ToString());
            }
            Console.Out.WriteLine("}");*/

            // Degenerate cases.
            if (validChoices.Count == 0)
            {
                //Console.Out.WriteLine("no valid choice!");
                return null;
            }
            if (validChoices.Count == 1)
            {
                return validChoices[0];
            }

            // Keep all the candidates that have the best value.
            List<ChoiceEval<_T_>> candidates = new List<ChoiceEval<_T_>>(validChoices.Count);
            for (int i = 0; i < validChoices.Count; i++)
                if (validChoices[i].Value == bestValue)
                    candidates.Add(validChoices[i]);

            /*Console.Out.WriteLine("Candidates {");
            for (int j = 0; j < candidates.Count; j++)
            {
                Console.Out.WriteLine("  {0}", candidates[j].ToString());
            }
            Console.Out.WriteLine("}");*/

            // Of all the candidates randomly choose one.
            int iChoice = game.Rules.Roll(0, candidates.Count);
            return candidates[iChoice];
        }

        protected ChoiceEval<_DATA_> ChooseExtended<_T_, _DATA_>(RogueGame game, List<_T_> listOfChoices,
            Func<_T_, _DATA_> isChoiceValidFn,
            Func<_T_, float> evalChoiceFn,
            Func<float, float, bool> isBetterEvalThanFn)
        {
            //Console.Out.WriteLine("Evaluating choices");

            // Degenerate cases.
            if (listOfChoices.Count == 0)
            {
                //Console.Out.WriteLine("no choice.");
                return null;
            }

            // Find valid choices and best value.
            bool hasValue = false;
            float bestValue = 0;    // irrevelant for 1st value, use flag hasValue instead.
            List<ChoiceEval<_DATA_>> validChoices = new List<ChoiceEval<_DATA_>>(listOfChoices.Count);
            for (int i = 0; i < listOfChoices.Count; i++)
            {
                _DATA_ choiceData = isChoiceValidFn(listOfChoices[i]);
                if (choiceData == null)
                    continue;

                float value_i = evalChoiceFn(listOfChoices[i]);

                if (float.IsNaN(value_i))
                    continue;

                validChoices.Add(new ChoiceEval<_DATA_>(choiceData, value_i));

                if (!hasValue || isBetterEvalThanFn(value_i, bestValue))
                {
                    hasValue = true;
                    bestValue = value_i;
                }
            }

            /*Console.Out.WriteLine("Evals {");
            for (int j = 0; j < validChoices.Count; j++)
            {
                Console.Out.WriteLine("  {0}", validChoices[j].ToString());
            }
            Console.Out.WriteLine("}");*/

            // Degenerate cases.
            if (validChoices.Count == 0)
            {
                //Console.Out.WriteLine("no valid choice!");
                return null;
            }
            if (validChoices.Count == 1)
            {
                return validChoices[0];
            }

            // Keep all the candidates that have the best value.
            List<ChoiceEval<_DATA_>> candidates = new List<ChoiceEval<_DATA_>>(validChoices.Count);
            for (int i = 0; i < validChoices.Count; i++)
                if (validChoices[i].Value == bestValue)
                    candidates.Add(validChoices[i]);

            // TEST: if no best value, nope.
            if (candidates.Count == 0)
                return null;

            /*Console.Out.WriteLine("Candidates {");
            for (int j = 0; j < candidates.Count; j++)
            {
                Console.Out.WriteLine("  {0}", candidates[j].ToString());
            }
            Console.Out.WriteLine("}");*/

            // Of all the candidates randomly choose one.
            int iChoice = game.Rules.Roll(0, candidates.Count);
            return candidates[iChoice];
        }
        #endregion

        #region Action filtering
        /// <summary>
        /// Checks if an action can be considered a valid fleeing action : Move, OpenDoor, SwitchPlace.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected bool IsValidFleeingAction(ActorAction a)
        {
            return a != null && (a is ActionMoveStep || a is ActionOpenDoor || a is ActionSwitchPlace);
        }

        /// <summary>
        /// Checks if an action can be considered a valid wandering action : Move, SwitchPlace, Push, OpenDoor, Chat/Trade, Bash, GetFromContainer, Barricade.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected bool isValidWanderAction(RogueGame game, ActorAction a)
        {
            return a != null && 
                (a is ActionMoveStep || 
                a is ActionSwitchPlace ||
                a is ActionPush ||
                a is ActionOpenDoor || 
                (a is ActionChat && (this.Directives.CanTrade || (a as ActionChat).Target == m_Actor.Leader)) || 
                a is ActionBashDoor || 
                (a is ActionGetFromContainer && IsInterestingItem(game, (a as ActionGetFromContainer).Item)) ||
                a is ActionBarricadeDoor);
        }

        /// <summary>
        /// Checks if an action can be considered a valid action to move toward a goal.
        /// Not valid actions : Chat, GetFromContainer, SwitchPowerGenerator, RechargeItemBattery
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        protected bool IsValidMoveTowardGoalAction(ActorAction a)
        {
            return a != null &&
                !(a is ActionChat || a is ActionGetFromContainer || a is ActionSwitchPowerGenerator || a is ActionRechargeItemBattery);
        }
        #endregion

        #region Actors predicates
        protected bool HasNoFoodItems(Actor actor)
        {
            Inventory inv = actor.Inventory;
            if (inv == null || inv.IsEmpty)
                return true;
            return !inv.HasItemOfType(typeof(ItemFood));
        }

        protected bool IsSoldier(Actor actor)
        {
            return actor != null && actor.Controller is SoldierAI;
        }

        protected bool WouldLikeToSleep(RogueGame game, Actor actor)
        {
            return game.Rules.IsAlmostSleepy(actor) || game.Rules.IsActorSleepy(actor);
        }

        protected bool IsOccupiedByOther(Map map, Point position)
        {
            Actor other = map.GetActorAt(position);
            return other != null && other != m_Actor;
        }

        protected bool IsAdjacentToEnemy(RogueGame game, Actor actor)
        {
            if (actor == null)
                return false;

            Map map = actor.Location.Map;

            return map.HasAnyAdjacentInMap(actor.Location.Position,
                (pt) =>
                {
                    Actor other = map.GetActorAt(pt);
                    if (other == null)
                        return false;
                    return game.Rules.IsEnemyOf(actor, other);
                });
        }

        protected bool IsInside(Actor actor)
        {
            if (actor == null)
                return false;

            return actor.Location.Map.GetTileAt(actor.Location.Position.X, actor.Location.Position.Y).IsInside;
        }

        protected bool HasEquipedRangedWeapon(Actor actor)
        {
            return (actor.GetEquippedWeapon() as ItemRangedWeapon) != null;
        }

        protected ItemAmmo GetCompatibleAmmoItem(RogueGame game, ItemRangedWeapon rw)
        {
            if (m_Actor.Inventory == null)
                return null;

            // get first compatible ammo item.
            foreach (Item it in m_Actor.Inventory.Items)
            {
                ItemAmmo ammoIt = it as ItemAmmo;
                if (ammoIt == null)
                    continue;
                if (ammoIt.AmmoType == rw.AmmoType && game.Rules.CanActorUseItem(m_Actor, ammoIt))
                    return ammoIt;
            }

            // failed.
            return null;
        }

        protected ItemRangedWeapon GetCompatibleRangedWeapon(RogueGame game, ItemAmmo am)
        {
            if (m_Actor.Inventory == null)
                return null;

            // get first compatible ammo item.
            foreach (Item it in m_Actor.Inventory.Items)
            {
                ItemRangedWeapon rangedIt = it as ItemRangedWeapon;
                if (rangedIt == null)
                    continue;
                if (rangedIt.AmmoType == am.AmmoType)
                    return rangedIt;
            }

            // failed.
            return null;
        }

        protected ItemMeleeWeapon GetBestMeleeWeapon(RogueGame game, Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null)
                return null;

            // best = score = most damage 1st, most attack 2nd, less stamina penalty 3rd.
            int bestScore = 0;
            ItemMeleeWeapon bestWeapon = null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (fn != null && !fn(it))
                    continue;

                ItemMeleeWeapon weapon = it as ItemMeleeWeapon;
                if (weapon == null)
                    continue;
                ItemMeleeWeaponModel model = weapon.Model as ItemMeleeWeaponModel;

                int score = 10000 * model.Attack.DamageValue + 
                            100 * model.Attack.HitValue + 
                            -model.Attack.StaminaPenalty;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestWeapon = weapon;
                }
            }

            // done.
            return bestWeapon;
        }

        protected ItemBodyArmor GetBestBodyArmor(RogueGame game, Predicate<Item> fn)
        {
            if (m_Actor.Inventory == null)
                return null;

            // best = most PRO.
            int bestPRO = 0;
            ItemBodyArmor bestArmor = null;

            foreach (Item it in m_Actor.Inventory.Items)
            {
                if (fn != null && !fn(it))
                    continue;

                ItemBodyArmor armor = it as ItemBodyArmor;
                if (armor == null)
                    continue;

                int pro = armor.Protection_Hit + armor.Protection_Shot;
                if (pro > bestPRO)
                {
                    bestPRO = pro;
                    bestArmor = armor;
                }
            }

            // done.
            return bestArmor;
        }

        protected bool WantToEvadeMelee(RogueGame game, Actor actor, ActorCourage courage, Actor target)
        {
            ///////////////////////////////////////////////////////
            // Targets to evade or not:
            // 1. Yes : if fighting makes me tired.
            // 2. Yes : slower targets that will act next turn (kiting) and are targetting us.
            // 3. No  : target is weaker.
            // 4. Yes : actor is weaker.
            // 5. Unclear cases, utimately decide on courage.
            ///////////////////////////////////////////////////////

            // 1. Always if fighting makes me tired.
            if (WillTireAfterAttack(game, actor))
                return true;

            // 2. Yes : slower targets that will act next turn (kiting) and are targetting us.
            if (game.Rules.ActorSpeed(actor) > game.Rules.ActorSpeed(target))
            {
                // don't evade if we're gonna act again.
                if (game.Rules.WillActorActAgainBefore(actor, target))
                    return false;
                else
                {
                    // evade if he is targetting us.
                    if (target.TargetActor == actor)
                        return true;
                }
            }

            // get weaker actor in melee.
            Actor weakerOne = FindWeakerInMelee(game, m_Actor, target);

            // 3. No : target is weaker.
            if (weakerOne == target)
                return false;

            // 4. Yes : actor is weaker.
            if (weakerOne == m_Actor)
                return true;

            // 5. Unclear cases, utimately decide on courage.
            return courage == ActorCourage.COURAGEOUS ? false : true;
        }

        /// <summary>
        /// Get which of the two actor can be considered as a weaker one in a melee fight.
        /// </summary>
        /// <returns>weaker actor, null if they are equal.</returns>
        protected Actor FindWeakerInMelee(RogueGame game, Actor a, Actor b)
        {
            int value_A = a.HitPoints + a.CurrentMeleeAttack.DamageValue;
            int value_B = b.HitPoints + b.CurrentMeleeAttack.DamageValue;

            return value_A < value_B ? a : value_A > value_B ? b : null;
        }

        protected bool WillTireAfterAttack(RogueGame game, Actor actor)
        {
            if (!actor.Model.Abilities.CanTire)
                return false;
            int staAfter = actor.StaminaPoints - Rules.STAMINA_COST_MELEE_ATTACK;
            return staAfter < Rules.STAMINA_MIN_FOR_ACTIVITY;
        }

        protected bool WillTireAfterRunning(RogueGame game, Actor actor)
        {
            if (!actor.Model.Abilities.CanTire)
                return false;
            int staAfter = actor.StaminaPoints - Rules.STAMINA_COST_RUNNING;
            return staAfter < Rules.STAMINA_MIN_FOR_ACTIVITY;
        }

        protected bool HasSpeedAdvantage(RogueGame game, Actor actor, Actor target)
        {
            int actorSpeed = game.Rules.ActorSpeed(actor);
            int targetSpeed = game.Rules.ActorSpeed(target);

            // if better speed, yes.
            if (actorSpeed > targetSpeed) 
                return true;

            // if we can run and the target can't and that would make us faster without tiring us, then yes!
            if (game.Rules.CanActorRun(actor) && !game.Rules.CanActorRun(target) &&
                !WillTireAfterRunning(game, actor) && actorSpeed * 2 > targetSpeed)
                return true;

            // TODO: other tricky cases?

            return false;
        }

        protected bool NeedsLight(RogueGame game)
        {
            switch (m_Actor.Location.Map.Lighting)
            {
                case Lighting.DARKNESS:
                    return true;
                case Lighting.LIT:
                    return false;
                case Lighting.OUTSIDE:
                    // Needs only if At Night & (Outside or Heavy Rain).
                    return m_Actor.Location.Map.LocalTime.IsNight &&
                        (game.Session.World.Weather == Weather.HEAVY_RAIN || !m_Actor.Location.Map.GetTileAt(m_Actor.Location.Position.X, m_Actor.Location.Position.Y).IsInside);
                default:
                    throw new ArgumentOutOfRangeException("unhandled lighting");
            }
        }

        /// <summary>
        /// Check if a point can be considered between two others.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <param name="C"></param>
        /// <returns></returns>
        protected bool IsBetween(RogueGame game, Point A, Point between, Point B)
        {
            float A_between = game.Rules.StdDistance(A, between);
            float B_between = game.Rules.StdDistance(B, between);
            float A_B = game.Rules.StdDistance(A, B);

            return A_between + B_between <= A_B + 0.25f;
        }

        protected bool IsDoorwayOrCorridor(RogueGame game, Map map, Point pos)
        {
            ///////////////////////////////////////
            // Check for simple shapes:
            // FREE-WALL-FREE       FREE-FREE-FREE
            // FREE-FREE-FREE       WALL-FREE-WALL
            // FREE-WALL-FREE       FREE-FREE-FREE
            ///////////////////////////////////////

            bool wall = !map.GetTileAt(pos).Model.IsWalkable;
            if(wall)
                return false;

            Point N = pos + Direction.N;
            bool nWall = map.IsInBounds(N) && !map.GetTileAt(N).Model.IsWalkable;
            Point S = pos + Direction.S;
            bool sWall = map.IsInBounds(S) && !map.GetTileAt(S).Model.IsWalkable;
            Point E = pos + Direction.E;
            bool eWall = map.IsInBounds(E) && !map.GetTileAt(E).Model.IsWalkable;
            Point W = pos + Direction.W;
            bool wWall = map.IsInBounds(W) && !map.GetTileAt(W).Model.IsWalkable;

            Point NE = pos + Direction.NE;
            bool neWall = map.IsInBounds(NE) && !map.GetTileAt(NE).Model.IsWalkable;
            Point NW = pos + Direction.NW;
            bool nwWall = map.IsInBounds(NW) && !map.GetTileAt(NW).Model.IsWalkable;
            Point SE = pos + Direction.SE;
            bool seWall = map.IsInBounds(SE) && !map.GetTileAt(SE).Model.IsWalkable;
            Point SW = pos + Direction.SW;
            bool swWall = map.IsInBounds(SW) && !map.GetTileAt(SW).Model.IsWalkable;

            bool freeCorners = !neWall && !seWall && !nwWall && !swWall;

            if (freeCorners && nWall && sWall && !eWall && !wWall)
                return true;
            if (freeCorners && eWall && wWall && !nWall && !sWall)
                return true;

            return false;
        }

        /// <summary>
        /// Not an enemy AND same faction.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool IsFriendOf(RogueGame game, Actor other)
        {
            return !game.Rules.IsEnemyOf(m_Actor, other) && m_Actor.Faction == other.Faction;
        }

        protected Actor GetNearestTargetFor(RogueGame game, Actor actor)
        {
            Map map = actor.Location.Map;
            Actor nearest = null;
            int best = int.MaxValue;

            // quite uggly but better than computing the whole FoV...
            foreach (Actor a in map.Actors)
            {
                if (a.IsDead) continue;
                if (a == actor) continue;
                if (!game.Rules.IsEnemyOf(actor, a)) continue;

                int d = game.Rules.GridDistance(a.Location.Position, actor.Location.Position);
                if (d < best)
                {
                    if (d == 1 || LOS.CanTraceViewLine(actor.Location, a.Location.Position))
                    {
                        best = d;
                        nearest = a;
                    }
                }
            }

            return nearest;
        }
        #endregion

        #region Exits
        protected List<Exit> ListAdjacentExits(RogueGame game, Location fromLocation)
        {
            List<Exit> list = null;
            foreach (Direction d in Direction.COMPASS)
            {
                Point nextPos = fromLocation.Position + d;
                Exit exit = fromLocation.Map.GetExitAt(nextPos);
                if (exit == null)
                    continue;
                if (list == null)
                    list = new List<Exit>(8);
                list.Add(exit);
            }

            return list;
        }

        protected Exit PickAnyAdjacentExit(RogueGame game, Location fromLocation)
        {
            // get all adjacent exits.
            List<Exit> list = ListAdjacentExits(game, fromLocation);

            // if none, failed.
            if (list == null)
                return null;

            // pick one at random.
            return list[game.Rules.Roll(0, list.Count)];
        }
        #endregion

        #region Map
        public static bool IsAnyActivatedTrapThere(Map map, Point pos)
        {
            Inventory inv = map.GetItemsAt(pos);
            if (inv == null || inv.IsEmpty) return false;
            return inv.GetFirstMatching((it) => { ItemTrap trap = it as ItemTrap; return trap != null && trap.IsActivated; }) != null;
        }

        public static bool IsZoneChange(Map map, Point pos)
        {
            List<Zone> zonesHere = map.GetZonesAt(pos.X, pos.Y);
            if (zonesHere == null) return false;

            // adjacent to another zone.
            return map.HasAnyAdjacentInMap(pos, (adj) =>
            {
                List<Zone> zonesAdj = map.GetZonesAt(adj.X, adj.Y);
                if (zonesAdj == null) return false;
                if (zonesHere == null) return true;
                foreach (Zone z in zonesAdj)
                    if (!zonesHere.Contains(z))
                        return true;
                return false;
            });
        }
        #endregion

        protected Point RandomPositionNear(Rules rules, Map map, Point goal, int range)
        {
            int x = goal.X + rules.Roll(-range, +range);
            int y = goal.Y + rules.Roll(-range, +range);

            map.TrimToBounds(ref x, ref y);

            return new Point(x, y);
        }
        #endregion

        #region Taboo items
        protected void MarkItemAsTaboo(Item it)
        {
            if (m_TabooItems == null)
                m_TabooItems = new List<Item>(1);
            else if (m_TabooItems.Contains(it))
                return;
            m_TabooItems.Add(it);
        }

        protected void UnmarkItemAsTaboo(Item it)
        {
            if (m_TabooItems == null)
                return;
            m_TabooItems.Remove(it);
            if (m_TabooItems.Count == 0)
                m_TabooItems = null;
        }

        protected bool IsItemTaboo(Item it)
        {
            if (m_TabooItems == null)
                return false;
            return m_TabooItems.Contains(it);
        }
        #endregion

        #region Taboo tiles
        protected void MarkTileAsTaboo(Point p)
        {
            if (m_TabooTiles == null)
                m_TabooTiles = new List<Point>(1);
            else if (m_TabooTiles.Contains(p))
                return;
            m_TabooTiles.Add(p);
        }

        protected bool IsTileTaboo(Point p)
        {
            if (m_TabooTiles == null)
                return false;
            return m_TabooTiles.Contains(p);
        }

        protected void ClearTabooTiles()
        {
            m_TabooTiles = null;
        }
        #endregion

        #region Taboo trades
        protected void MarkActorAsRecentTrade(Actor other)
        {
            if (m_TabooTrades == null)
                m_TabooTrades = new List<Actor>(1);
            else if (m_TabooTrades.Contains(other))
                return;
            m_TabooTrades.Add(other);
        }

        protected bool IsActorTabooTrade(Actor other)
        {
            if (m_TabooTrades == null) return false;
            return m_TabooTrades.Contains(other);
        }

        protected void ClearTabooTrades()
        {
            m_TabooTrades = null;
        }
        #endregion
    }
}
