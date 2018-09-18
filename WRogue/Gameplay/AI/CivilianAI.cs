using System;
using System.Collections.Generic;
using System.Drawing;   // Point

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.AI;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Gameplay.AI.Sensors;
using djack.RogueSurvivor.Gameplay.AI.Tools;

namespace djack.RogueSurvivor.Gameplay.AI
{
    [Serializable]
    /// <summary>
    /// Civilian AI : Civilians, Survivors, Cops.
    /// </summary>
    class CivilianAI : OrderableAI
    {
        #region Constants
        const int FOLLOW_NPCLEADER_MAXDIST = 1;
        const int FOLLOW_PLAYERLEADER_MAXDIST = 1;

        const int EXPLORATION_MAX_LOCATIONS = 30;
        const int EXPLORATION_MAX_ZONES = 6;  // alpha10.1 doubled from 3 to 6

        const int USE_EXIT_CHANCE = 20;

        const int BUILD_TRAP_CHANCE = 50;
        const int BUILD_SMALL_FORT_CHANCE = 20;
        const int BUILD_LARGE_FORT_CHANCE = 50;
        const int START_FORT_LINE_CHANCE = 1;

        const int TELL_FRIEND_ABOUT_RAID_CHANCE = 20;
        const int TELL_FRIEND_ABOUT_ENEMY_CHANCE = 10;
        const int TELL_FRIEND_ABOUT_ITEMS_CHANCE = 10;
        const int TELL_FRIEND_ABOUT_SOLDIER_CHANCE = 20;

        const int MIN_TURNS_SAFE_TO_SLEEP = 10;

        const int USE_STENCH_KILLER_CHANCE = 75;

        const int HUNGRY_CHARGE_EMOTE_CHANCE = 50;
        const int HUNGRY_PUSH_OBJECTS_CHANCE = 25;

        const int LAW_ENFORCE_CHANCE = 30;

        const int DONT_LEAVE_BEHIND_EMOTE_CHANCE = 50;

        static string[] FIGHT_EMOTES = 
        {
            "Go away",                  // flee
            "Damn it I'm trapped!",     // trapped
            "I'm not afraid"            // fight
        };

        // alpha10
        const string CANT_GET_ITEM_EMOTE = "Mmmh. Looks like I can't reach what I want.";

        // Unique emotes.
        static string[] BIG_BEAR_EMOTES =
        {
            "You fool",                 // flee
            "I'm fooled!",              // trapped
            "Be a man"                  // fight
        };
        static string[] FAMU_FATARU_EMOTES =
        {
            "Bakemono",       // flee
            "Nani!?",         // trapped
            "Kawaii"      // fight
        };
        static string[] SANTAMAN_EMOTES =
        {
            "DEM BLOODY KIDS!",                          // flee
            "LEAVE ME ALONE I AIN'T HAVE NO PRESENTS!",  // trapped
            "MERRY FUCKIN' CHRISTMAS"                   // fight
        };
        static string[] ROGUEDJACK_EMOTES =
        {
            "Sorry butt I am le busy,",                 // flee
            "I should have redone ze AI rootines!",     // trapped
            "Let me test le something on you"           // fight
        };
        static string[] DUCKMAN_EMOTES =
        {
            "I'LL QUACK YOU BACK",     // flee
            "THIS IS MY FINAL QUACK",  // trapped
            "I'M GONNA QUACK YOU"      // fight
        };
        static string[] HANS_VON_HANZ_EMOTES =
        {
            "RAUS",             // flee
            "MEIN FUHRER!",     // trapped
            "KOMM HIER BITE"    // fight
        };        

        #endregion

        #region Fields
        LOSSensor m_LOSSensor;

        int m_SafeTurns;
        ExplorationData m_Exploration;

        string[] m_Emotes;
        #endregion

        #region BaseAI
        public override void TakeControl(Actor actor)
        {
            base.TakeControl(actor);
        
            m_SafeTurns = 0;
            m_Exploration = new ExplorationData(EXPLORATION_MAX_LOCATIONS, EXPLORATION_MAX_ZONES);

            m_LastEnemySaw = null;
            m_LastItemsSaw = null;
            m_LastSoldierSaw = null;
            m_LastRaidHeard = null;
            m_Emotes = null;
        }

        protected override void CreateSensors()
        {
            m_LOSSensor = new LOSSensor(LOSSensor.SensingFilter.ACTORS | LOSSensor.SensingFilter.ITEMS | LOSSensor.SensingFilter.CORPSES);
        }

        protected override List<Percept> UpdateSensors(RogueGame game)
        {
            // emotes!
            if (m_Emotes == null)
            {
                // FIXME: uggly code.
                if (m_Actor.IsUnique)
                {
                    if (m_Actor == game.Session.UniqueActors.BigBear.TheActor)
                        m_Emotes = BIG_BEAR_EMOTES;
                    else if (m_Actor == game.Session.UniqueActors.FamuFataru.TheActor)
                        m_Emotes = FAMU_FATARU_EMOTES;
                    else if (m_Actor == game.Session.UniqueActors.Santaman.TheActor)
                        m_Emotes = SANTAMAN_EMOTES;
                    else if (m_Actor == game.Session.UniqueActors.Roguedjack.TheActor)
                        m_Emotes = ROGUEDJACK_EMOTES;
                    else if (m_Actor == game.Session.UniqueActors.Duckman.TheActor)
                        m_Emotes = DUCKMAN_EMOTES;
                    else if (m_Actor == game.Session.UniqueActors.HansVonHanz.TheActor)
                        m_Emotes = HANS_VON_HANZ_EMOTES;
                    else
                        m_Emotes = FIGHT_EMOTES;
                }
                else
                    m_Emotes = FIGHT_EMOTES;
            }

            // sense.
            return m_LOSSensor.Sense(game, m_Actor);
        }

        protected override ActorAction SelectAction(RogueGame game, List<Percept> percepts)
        {
            List<Percept> mapPercepts = FilterSameMap(game, percepts);

            // DEBUG BOT
#if DEBUG
            bool botBreakpoint = false;
            bool verboseBotExploreWander = false;
            if (m_Actor.IsBotPlayer)
            {
                botBreakpoint = false; // true;
                verboseBotExploreWander = false; // true;
            }
#endif
            // END DEBUG BOT

            ///////////////////////
            // 0. Equip best item.  // alpha10
            // 1. Follow order
            // 2. Normal behavior.
            ///////////////////////

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
                ActorAction orderAction = ExecuteOrder(game, this.Order, mapPercepts, m_Exploration);
                if (orderAction == null)
                    SetOrder(null);
                else
                {
                    m_Actor.Activity = Activity.FOLLOWING_ORDER;
                    return orderAction;
                }
            }
            #endregion

            // 2. Normal behavior.
            #region
            //////////////////////////////////////////////////////////////////////
            // BEHAVIOR
            // - FLAGS
            // "courageous" : has leader, see leader, he is fighting and actor not tired.
            // - RULES
            // 0 run away from primed explosives.
            // 1 throw grenades at enemies.
            // alpha10 OBSOLETE 2 equip weapon/armor
            // 3 fire at nearest (always if has leader, half of the time if not)  - check directives
            // 4 fight or flee, shout
            // 5 use medicine
            // 6 rest if tired
            // alpha10 obsolete and redundant with rule 4! 7 charge enemy if courageous
            // 8 eat when hungry (also eat corpses)
            // 9 sleep when almost sleepy and safe.
            // 10 drop light/tracker with no batteries
            // alpha10 OBSOLETE 11 equip light/tracker/scent spray
            // 12 make room for food items if needed.
            // 13 get nearby item/trade (not if seeing enemy) - check directives.
            // 14 if hungry and no food, charge at people for food (option, not follower or law enforcer)
            // 15 use stench killer.
            // 16 close door behind me.
            // 17 use entertainment
            // 18 follow leader.
            // 19 take lead (if leadership)
            // 20 if hungry, tear down barricades & push objects.
            // 21 go revive corpse.
            // 22 use exit.
            // 23 build trap or fortification.
            // 24 tell friend about latest raid.
            // 25 tell friend about latest friendly soldier.
            // 26 tell friend about latest enemy.
            // 27 tell friend about latest items.
            // 28 (law enforcer) watch for murderers.
            // 29 (leader) don't leave followers behind.
            // 30 explore.
            // 31 wander.
            //////////////////////////////////////////////////////////////////////

            // get data.
            List<Percept> enemies = FilterEnemies(game, mapPercepts);
            bool hasEnemies = enemies != null && enemies.Count > 0;
            bool checkOurLeader = m_Actor.HasLeader && !DontFollowLeader;
            bool seeLeader = checkOurLeader && m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
            bool isLeaderFighting = checkOurLeader && IsAdjacentToEnemy(game, m_Actor.Leader);
            bool isCourageous = checkOurLeader && seeLeader && isLeaderFighting && !game.Rules.IsActorTired(m_Actor);

            // safety counter.
            if (hasEnemies)
                m_SafeTurns = 0;
            else
                ++m_SafeTurns;

            // exploration.
            m_Exploration.Update(m_Actor.Location);

            // clear taboo tiles : periodically or when changing maps.
            if (m_Actor.Location.Map.LocalTime.TurnCounter % WorldTime.TURNS_PER_HOUR == 0 || 
                (PrevLocation != null && PrevLocation.Map != m_Actor.Location.Map))
            {
                ClearTabooTiles();
            }
            // clear trades.
            if (m_Actor.Location.Map.LocalTime.TurnCounter % WorldTime.TURNS_PER_DAY == 0)
            {
                ClearTabooTrades();
            }

            // last enemy saw.
            if (hasEnemies)
                m_LastEnemySaw = enemies[game.Rules.Roll(0, enemies.Count)];            

            // 0 run away from primed explosives.
            #region
            ActorAction runFromExplosives = BehaviorFleeFromExplosives(game, FilterStacks(game, mapPercepts));
            if (runFromExplosives != null)
            {
                m_Actor.Activity = Activity.FLEEING_FROM_EXPLOSIVE;
                return runFromExplosives;
            }
            #endregion

            // 1 throw grenades at enemies.
            #region
            // if directive off, unequip.
            if (!this.Directives.CanThrowGrenades)
            {
                // unequip grenade?
                ItemGrenade eqGrenade = m_Actor.GetEquippedWeapon() as ItemGrenade;
                if (eqGrenade != null)
                {
                    ActionUnequipItem unequipGre = new ActionUnequipItem(m_Actor, game, eqGrenade);
                    if (unequipGre.IsLegal())
                    {
                        m_Actor.Activity = Activity.IDLE;
                        return unequipGre;
                    }
                }
            }
            // throw?
            if (hasEnemies)
            {
                ActorAction throwAction = BehaviorThrowGrenade(game, m_LOSSensor.FOV, enemies);
                if (throwAction != null)
                {
                    return throwAction;
                }
            }
            #endregion

            // alpha10 obsolete
            //// 2 equip weapon/armor
            //#region
            //ActorAction equipWpnAction = BehaviorEquipWeapon(game);
            //if (equipWpnAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipWpnAction;
            //}
            //ActorAction equipArmAction = BehaviorEquipBodyArmor(game);
            //if (equipArmAction != null)
            //{
            //    m_Actor.Activity = Activity.IDLE;
            //    return equipArmAction;
            //}
            //#endregion

            // 3 fire at nearest enemy
            #region
            if (hasEnemies && this.Directives.CanFireWeapons && m_Actor.GetEquippedWeapon() is ItemRangedWeapon)
            {
                List<Percept> fireTargets = FilterFireTargets(game, enemies);
                if (fireTargets != null)
                {
                    Percept nearestTarget = FilterNearest(game, fireTargets);
                    Actor target = nearestTarget.Percepted as Actor;

                    // flee contact from someone SLOWER with no ranged weapon.
                    if (game.Rules.GridDistance(nearestTarget.Location.Position, m_Actor.Location.Position) == 1 &&
                        !HasEquipedRangedWeapon(target) &&
                        HasSpeedAdvantage(game, m_Actor,target))
                    {
                        // flee!
                        ActorAction fleeAction = BehaviorWalkAwayFrom(game, nearestTarget);
                        if (fleeAction != null)
                        {
                            RunIfPossible(game.Rules);
                            m_Actor.Activity = Activity.FLEEING;
                            return fleeAction;
                        }
                    }

                    // fire ze missiles!
                    ActorAction fireAction = BehaviorRangedAttack(game, nearestTarget);
                    if (fireAction != null)
                    {
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = target;
                        return fireAction;
                    }

                }
            }
            #endregion

            // 4 fight or flee, shout
            #region
            if (hasEnemies)
            {
                // shout?
                if (game.Rules.RollChance(50))
                {
                    List<Percept> friends = FilterNonEnemies(game, mapPercepts);
                    if (friends != null)
                    {
                        ActorAction shoutAction = BehaviorWarnFriends(game, friends, FilterNearest(game, enemies).Percepted as Actor);
                        if (shoutAction != null)
                        {
                            m_Actor.Activity = Activity.IDLE;
                            return shoutAction;
                        }
                    }
                }
                // fight or flee.
                RouteFinder.SpecialActions allowedChargeActions = RouteFinder.SpecialActions.JUMP | RouteFinder.SpecialActions.DOORS; // alpha10
                ActorAction fightOrFlee = BehaviorFightOrFlee(game, enemies, seeLeader, isLeaderFighting, Directives.Courage, m_Emotes, allowedChargeActions);
                if (fightOrFlee != null)
                {
                    return fightOrFlee;
                }
            }
            #endregion

            // 5 use medicine
            #region
            ActorAction useMedAction = BehaviorUseMedecine(game, 2, 1, 2, 4, 2);
            if (useMedAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return useMedAction;
            }
            #endregion

            // 6 rest if tired
            #region
            ActorAction restAction = BehaviorRestIfTired(game);
            if (restAction != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return new ActionWait(m_Actor, game);
            }
            #endregion

            // alpha10 obsolete and redundant with rule 4!
            //// 7 charge enemy if courageous
            //#region
            //if (hasEnemies && isCourageous)
            //{
            //    Percept nearestEnemy = FilterNearest(game, enemies);
            //    ActorAction chargeAction = BehaviorChargeEnemy(game, nearestEnemy, false, false);
            //    if (chargeAction != null)
            //    {
            //        m_Actor.Activity = Activity.FIGHTING;
            //        m_Actor.TargetActor = nearestEnemy.Percepted as Actor;
            //        return chargeAction;
            //    }
            //}
            //#endregion

            // 8 eat when hungry (also eat corpses)
            #region
            if (game.Rules.IsActorHungry(m_Actor))
            {
                ActorAction eatAction = BehaviorEat(game);
                if (eatAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return eatAction;
                }
                if (game.Rules.IsActorStarving(m_Actor) || game.Rules.IsActorInsane(m_Actor))
                {
                    eatAction = BehaviorGoEatCorpse(game, FilterCorpses(game, mapPercepts));
                    if (eatAction != null)
                    {
                        m_Actor.Activity = Activity.IDLE;
                        return eatAction;
                    }
                }
            }
            #endregion

            // 9 sleep when almost sleepy and safe.
            #region
            if (m_SafeTurns >= MIN_TURNS_SAFE_TO_SLEEP && this.Directives.CanSleep && 
                WouldLikeToSleep(game, m_Actor) && IsInside(m_Actor) && game.Rules.CanActorSleep(m_Actor))
            {               
                // secure sleep.
                ActorAction secureSleepAction = BehaviorSecurePerimeter(game, m_LOSSensor.FOV);
                if (secureSleepAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return secureSleepAction;
                }

                // sleep.
                ActorAction sleepAction = BehaviorSleep(game, m_LOSSensor.FOV);
                if (sleepAction != null)
                {
                    if (sleepAction is ActionSleep)
                        m_Actor.Activity = Activity.SLEEPING;
                    return sleepAction;
                }
            }
            #endregion

            // 10 drop useless light/tracker/spray
            #region
            ActorAction dropUseless = BehaviorDropUselessItem(game);
            if (dropUseless != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return dropUseless;
            }
            #endregion

            // alpha10 obsolete
            // 11 equip light/tracker/spray.
            //#region
            //// tracker : if has leader or is a leader.
            //bool needCellPhone = m_Actor.HasLeader || m_Actor.CountFollowers > 0;
            //// then light.
            //bool needLight = NeedsLight(game);
            //// finally spray.
            //bool needSpray = IsGoodStenchKillerSpot(game, m_Actor.Location.Map, m_Actor.Location.Position);
            //// if tracker/light/spray useless, unequip it.
            //if (!needCellPhone && !needLight && !needSpray)
            //{
            //    ActorAction unequipUselessLeftItem = BehaviorUnequipLeftItem(game);
            //    if (unequipUselessLeftItem != null)
            //    {
            //        m_Actor.Activity = Activity.IDLE;
            //        return unequipUselessLeftItem;
            //    }
            //}
            //// tracker?
            //if(needCellPhone)
            //{
            //    ActorAction eqTrackerAction = BehaviorEquipCellPhone(game);
            //    if (eqTrackerAction != null)
            //    {
            //        m_Actor.Activity = Activity.IDLE;
            //        return eqTrackerAction;
            //    }
            //}
            //// ...or light?
            //else if (needLight)
            //{
            //    ActorAction eqLightAction = BehaviorEquipLight(game);
            //    if (eqLightAction != null)
            //    {
            //        m_Actor.Activity = Activity.IDLE;
            //        return eqLightAction;
            //    }

            //}
            //// ... scent spray?
            //else if (needSpray)
            //{
            //    ActorAction eqScentSpray = BehaviorEquipStenchKiller(game);
            //    if (eqScentSpray != null)
            //    {
            //        m_Actor.Activity = Activity.IDLE;
            //        return eqScentSpray;
            //    }
            //}
            //#endregion

            // 12 make room for food items if needed.
            // &&
            // 13 get nearby item/trade (not if seeing enemy)
            // ignore not currently visible items & blocked items.
            #region
            if (!hasEnemies && this.Directives.CanTakeItems)
            {
                Map map = m_Actor.Location.Map;

                #region Get items
                // alpha10 new common behaviour code, also used by GangAI
                ActorAction getItemAction = BehaviorGoGetInterestingItems(game, mapPercepts,
                    false, false, CANT_GET_ITEM_EMOTE, true, ref m_LastItemsSaw);

                if (getItemAction != null)
                    return getItemAction;
                #endregion

                #region Trade
                if (Directives.CanTrade)
                {
                    // get actors we want to trade with.
                    List<Percept> tradingActors = FilterOut(game, FilterNonEnemies(game, mapPercepts),
                        (p) =>
                        {
                            if (p.Turn != map.LocalTime.TurnCounter) return true;
                            Actor other = p.Percepted as Actor;
                            // dont bother player or someone we can't trade with or already did trade.
                            if (other.IsPlayer) return true;
                            if (!game.Rules.CanActorInitiateTradeWith(m_Actor, other)) return true;
                            if (IsActorTabooTrade(other)) return true;
                            // alpha10 dont bother someone who is fighting or fleeing
                            if (other.Activity == Activity.CHASING || other.Activity == Activity.FIGHTING || other.Activity == Activity.FLEEING || other.Activity == Activity.FLEEING_FROM_EXPLOSIVE)
                                return true;
                            // dont bother if no interesting items.
                            if (!HasAnyInterestingItem(game, other.Inventory, ItemSource.ANOTHER_ACTOR)) return true;
                            if (!((other.Controller as BaseAI).HasAnyInterestingItem(game, m_Actor.Inventory, ItemSource.ANOTHER_ACTOR))) return true;
                            // alpha10 reject if unreachable by baseai simple behaviours
                            if (!CanReachSimple(game, other.Location.Position, Tools.RouteFinder.SpecialActions.DOORS | Tools.RouteFinder.SpecialActions.JUMP))
                                return true;
                            // don't reject.
                            return false;
                        });
                    // trade with nearest.
                    if (tradingActors != null)
                    {
                        Actor tradeTarget = FilterNearest(game, tradingActors).Percepted as Actor;
                        if (game.Rules.IsAdjacent(m_Actor.Location, tradeTarget.Location))
                        {
                            ActorAction tradeAction = new ActionTrade(m_Actor, game, tradeTarget);
                            if (tradeAction.IsLegal())
                            {
                                // remember we tried to trade.
                                MarkActorAsRecentTrade(tradeTarget);
                                // say, so we make sure we spend a turn and won't loop.
                                game.DoSay(m_Actor, tradeTarget, String.Format("Hey {0}, let's make a deal!", tradeTarget.Name), RogueGame.Sayflags.NONE);
                                return tradeAction;
                            }
                        }
                        else
                        {
                            ActorAction bump = BehaviorIntelligentBumpToward(game, tradeTarget.Location.Position, false, false);
                            if (bump != null)
                            {
                                // alpha10 announce it to make it clear to the player whats happening but dont spend AP (free action)
                                // might spam for a few turns, but its better than not understanding whats going on.
                                game.DoSay(m_Actor, tradeTarget, String.Format("Hey {0}, let's make a deal!", tradeTarget.Name), RogueGame.Sayflags.IS_FREE_ACTION);

                                m_Actor.Activity = Activity.FOLLOWING;
                                m_Actor.TargetActor = tradeTarget;
                                return bump;
                            }
                        }
                    }
                }
                #endregion
            }
            #endregion

            // 14 if hungry and no food, charge at people for food (option, not follower or law enforcer)
            #region
            if (RogueGame.Options.IsAggressiveHungryCiviliansOn && 
                mapPercepts != null && !m_Actor.HasLeader && !m_Actor.Model.Abilities.IsLawEnforcer &&
                game.Rules.IsActorHungry(m_Actor) && HasNoFoodItems(m_Actor))
            {
                Percept targetForFood = FilterNearest(game, FilterActors(game, mapPercepts,
                    (a) =>
                    {
                        // reject self, dead and leader/follower.
                        if (a == m_Actor) return false;
                        if (a.IsDead) return false;
                        if (a.Inventory == null || a.Inventory.IsEmpty) return false;
                        if (a.Leader == m_Actor || m_Actor.Leader == a) return false;

                        // actor has food or is standing on food.
                        if (a.Inventory.HasItemOfType(typeof(ItemFood))) return true;
                        Inventory groundInv = a.Location.Map.GetItemsAt(a.Location.Position);
                        if (groundInv == null || groundInv.IsEmpty) return false;
                        return groundInv.HasItemOfType(typeof(ItemFood));
                    }));

                if (targetForFood != null)
                {
                    // alpha10 hungry civs can break and push
                    ActorAction chargeAction = BehaviorChargeEnemy(game, targetForFood, true, true);
                    if (chargeAction != null)
                    {
                        // randomly emote.
                        if (game.Rules.RollChance(HUNGRY_CHARGE_EMOTE_CHANCE))
                            game.DoSay(m_Actor, targetForFood.Percepted as Actor, "HEY! YOU! SHARE SOME FOOD!", RogueGame.Sayflags.IS_FREE_ACTION | RogueGame.Sayflags.IS_DANGER);

                        // chaaarge!
                        m_Actor.Activity = Activity.FIGHTING;
                        m_Actor.TargetActor = targetForFood.Percepted as Actor;
                        return chargeAction;
                    }
                }
            }
            #endregion

            // 15 use stench killer.
            #region
            if (game.Rules.RollChance(USE_STENCH_KILLER_CHANCE))
            {
                ActorAction sprayAction = BehaviorUseStenchKiller(game);
                if (sprayAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return sprayAction;
                }
            }
            #endregion

            // 16 close door behind me.
            #region
            ActorAction closeBehindMe = BehaviorCloseDoorBehindMe(game, PrevLocation);
            if (closeBehindMe != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return closeBehindMe;
            }
            #endregion

            // 17 use entertainment
            #region
            if (m_Actor.Model.Abilities.HasSanity)
            {
                if (m_Actor.Sanity < 0.75f * game.Rules.ActorMaxSanity(m_Actor))
                {
                    ActorAction entAction = BehaviorUseEntertainment(game);
                    if (entAction != null)
                    {
                        m_Actor.Activity = Activity.IDLE;
                        return entAction;
                    }
                }
                // TODO -- consider moving this to DropUselessItems()
                ActorAction dropEnt = BehaviorDropBoringEntertainment(game);
                if (dropEnt != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return dropEnt;
                }                
            }
            #endregion

            // 18 build trap or fortification.
            // alpha10.1 moved trap/fortification rule before following leader rule so they will do it much more often
            #region
            if (game.Rules.RollChance(BUILD_TRAP_CHANCE))
            {
                ActorAction trapAction = BehaviorBuildTrap(game);
                if (trapAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return trapAction;
                }
            }
            // large fortification.
            if (game.Rules.RollChance(BUILD_LARGE_FORT_CHANCE))
            {
                ActorAction buildAction = BehaviorBuildLargeFortification(game, START_FORT_LINE_CHANCE);
                if (buildAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return buildAction;
                }
            }
            // small fortification.
            if (game.Rules.RollChance(BUILD_SMALL_FORT_CHANCE))
            {
                ActorAction buildAction = BehaviorBuildSmallFortification(game);
                if (buildAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return buildAction;
                }
            }
            #endregion

            // 19 follow leader
            #region
            if (checkOurLeader)
            {
                Point lastKnownLeaderPosition = m_Actor.Leader.Location.Position;
                bool isLeaderVisible = m_LOSSensor.FOV.Contains(m_Actor.Leader.Location.Position);
                int maxDist = m_Actor.Leader.IsPlayer ? FOLLOW_PLAYERLEADER_MAXDIST : FOLLOW_NPCLEADER_MAXDIST;
                ActorAction followAction = BehaviorFollowActor(game, m_Actor.Leader, lastKnownLeaderPosition, isLeaderVisible, maxDist);
                if (followAction != null)
                {
                    m_Actor.Activity = Activity.FOLLOWING;
                    m_Actor.TargetActor = m_Actor.Leader;
                    return followAction;
                }
            }
            #endregion

            // 20 take lead (if leadership)
            #region
            bool hasLeadership = m_Actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LEADERSHIP) >= 1;
            if (hasLeadership)
            {
                bool canLead = !checkOurLeader && m_Actor.CountFollowers < game.Rules.ActorMaxFollowers(m_Actor);
                if (canLead)
                {
                    Percept nearestFriend = FilterNearest(game, FilterNonEnemies(game, mapPercepts));
                    if (nearestFriend != null)
                    {
                        // alpha10 only if unreachable by baseai simple behaviours
                        if (CanReachSimple(game, nearestFriend.Location.Position, Tools.RouteFinder.SpecialActions.DOORS | Tools.RouteFinder.SpecialActions.JUMP))
                        {
                            ActorAction leadAction = BehaviorLeadActor(game, nearestFriend);
                            if (leadAction != null)
                            {
                                m_Actor.Activity = Activity.IDLE;
                                m_Actor.TargetActor = nearestFriend.Percepted as Actor;
                                return leadAction;
                            }
                        }
                    }
                }
            }
            #endregion

            // 21 if hungry, tear down barricades & push objects.
            #region
            if (game.Rules.IsActorHungry(m_Actor))
            {
                ActorAction attackBarricadeAction = BehaviorAttackBarricade(game);
                if (attackBarricadeAction != null)
                {
                    // emote.
                    game.DoEmote(m_Actor, "Open damn it! I know there is food there!", true);

                    // go!
                    m_Actor.Activity = Activity.IDLE;
                    return attackBarricadeAction;
                }
                if (game.Rules.RollChance(HUNGRY_PUSH_OBJECTS_CHANCE))
                {
                    // alpha10.1 do that only inside where food is more likely to be hidden, pushing cars outside is stupid -_-
                    if (m_Actor.Location.Map.GetTileAt(m_Actor.Location.Position).IsInside)
                    {
                        ActorAction pushAction = BehaviorPushNonWalkableObject(game);
                        if (pushAction != null)
                        {
                            // emote.
                            game.DoEmote(m_Actor, "Where is all the damn food?!", true);

                            // go!
                            m_Actor.Activity = Activity.IDLE;
                            return pushAction;
                        }
                    }
                }
            }
            #endregion

            // 22 go revive corpse.
            ActorAction revive = BehaviorGoReviveCorpse(game, FilterCorpses(game, mapPercepts));
            if (revive != null)
            {
                m_Actor.Activity = Activity.IDLE;
                return revive;
            }

            // 23 use exit.
            #region
            if (game.Rules.RollChance(USE_EXIT_CHANCE))
            {
                ActorAction useExit = BehaviorUseExit(game, UseExitFlags.DONT_BACKTRACK);
                if (useExit != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return useExit;
                }
            }
            #endregion

            // 24 tell friend about latest raid.
            #region
            // tell?
            if (m_LastRaidHeard != null && game.Rules.RollChance(TELL_FRIEND_ABOUT_RAID_CHANCE))
            {
                ActorAction tellAction = BehaviorTellFriendAboutPercept(game, m_LastRaidHeard);
                if (tellAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return tellAction;
                }
            }
            #endregion

            // 25 tell friend about latest soldier.
            #region
            // update percept.
            Percept seeingSoldier = FilterFirst(game, mapPercepts, 
                (p) =>
                {
                    Actor other = p.Percepted as Actor;
                    if (other == null || other == m_Actor)
                        return false;
                    return IsSoldier(other);
                });
            if (seeingSoldier != null)
                m_LastSoldierSaw = seeingSoldier;
            // tell?
            if (game.Rules.RollChance(TELL_FRIEND_ABOUT_SOLDIER_CHANCE) && m_LastSoldierSaw != null)
            {
                ActorAction tellAction = BehaviorTellFriendAboutPercept(game, m_LastSoldierSaw);
                if (tellAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return tellAction;
                }
            }
            #endregion

            // 26 tell friend about latest enemy.
            #region
            if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ENEMY_CHANCE) && m_LastEnemySaw != null)
            {
                ActorAction tellAction = BehaviorTellFriendAboutPercept(game, m_LastEnemySaw);
                if (tellAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return tellAction;
                }
            }
            #endregion

            // 27 tell friend about latest items.
            #region
            if (game.Rules.RollChance(TELL_FRIEND_ABOUT_ITEMS_CHANCE) && m_LastItemsSaw != null)
            {
                ActorAction tellAction = BehaviorTellFriendAboutPercept(game, m_LastItemsSaw);
                if (tellAction != null)
                {
                    m_Actor.Activity = Activity.IDLE;
                    return tellAction;
                }
            }
            #endregion

            // 28 (law enforcer) watch for murderers.
            #region
            if (m_Actor.Model.Abilities.IsLawEnforcer && mapPercepts != null && game.Rules.RollChance(LAW_ENFORCE_CHANCE))
            {
                Actor target;
                ActorAction lawAction = BehaviorEnforceLaw(game, mapPercepts, out target);
                if (lawAction != null)
                {
                    m_Actor.TargetActor = target;
                    return lawAction;
                }
            }
            #endregion

            // 29 (leader) don't leave followers behind.
            #region
            if (m_Actor.CountFollowers > 0)
            {
                Actor target;
                ActorAction stickTogether = BehaviorDontLeaveFollowersBehind(game, 2, out target);
                if (stickTogether != null)
                {
                    // emote?
                    if (game.Rules.RollChance(DONT_LEAVE_BEHIND_EMOTE_CHANCE))
                    {
                        if (target.IsSleeping)
                            game.DoEmote(m_Actor, String.Format("patiently waits for {0} to wake up.", target.Name));
                        else
                        {
                            if (m_LOSSensor.FOV.Contains(target.Location.Position))
                                game.DoEmote(m_Actor, String.Format("Come on {0}! Hurry up!", target.Name));
                            else
                                game.DoEmote(m_Actor, String.Format("Where the hell is {0}?", target.Name));
                        }
                    }

                    // go!
                    m_Actor.Activity = Activity.IDLE;
                    return stickTogether;
                }
            }
            #endregion

            // 30 explore
            #region

            // DEBUG BOT
#if DEBUG
            if (botBreakpoint)
                Console.Out.WriteLine("test bot exploration breakpoint");
#endif
            // END DEBUG BOT

            ActorAction exploreAction = BehaviorExplore(game, m_Exploration);
            if (exploreAction != null)
            {
                // VERBOSE BOT
#if DEBUG
                if (verboseBotExploreWander)
                    game.AddMessage(new Message(">> Bot is Exploring", m_Actor.Location.Map.LocalTime.TurnCounter));
#endif
                // END VERBOSE BOT

                m_Actor.Activity = Activity.IDLE;
                return exploreAction;
            }
            #endregion

            // 31 wander.
            #region
            // VERBOSE BOT
#if DEBUG
            if (verboseBotExploreWander)
                game.AddMessage(new Message(">> Bot is Wandering", m_Actor.Location.Map.LocalTime.TurnCounter));
#endif
            // END VERBOSE BOT

            m_Actor.Activity = Activity.IDLE;
            return BehaviorWander(game, m_Exploration);
            #endregion

            #endregion
        }
        #endregion
    }
}
