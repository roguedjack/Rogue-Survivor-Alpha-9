using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.Engine.Actions;
using djack.RogueSurvivor.Engine.Items;
using djack.RogueSurvivor.Engine.MapObjects;

namespace djack.RogueSurvivor.Engine
{
    class Rules
    {
        #region Constants
        public const int BASE_ACTION_COST = 100;
        public const int BASE_SPEED = BASE_ACTION_COST;

        #region Stamina
        /// <summary>
        /// Marker for infinite stamina, value doesnt matter since !Abilities.CanTire is checked instead.
        /// </summary>
        public const int STAMINA_INFINITE = 99;

        /// <summary>
        /// Minimum stamina for doing tiring actions (melee, running, jumping...)
        /// </summary>
        public const int STAMINA_MIN_FOR_ACTIVITY = 10;

        /// <summary>
        /// Stamina cost for running.
        /// </summary>
        public const int STAMINA_COST_RUNNING = 4;

        /// <summary>
        /// Bonus to stamina when waiting.
        /// </summary>
        public const int STAMINA_REGEN_WAIT = 2;

        /// <summary>
        /// Bonus to stamina each turn.
        /// </summary>
        public const int STAMINA_REGEN_PER_TURN = 2;

        /// <summary>
        /// Stamina cost for jumping.
        /// </summary>
        public const int STAMINA_COST_JUMP = 8;

        /// <summary>
        /// Stamina cost for melee fighting.
        /// </summary>
        public const int STAMINA_COST_MELEE_ATTACK = 8;

        /// <summary>
        /// Stamina cost for moving with a dragged corpse.
        /// </summary>
        public const int STAMINA_COST_MOVE_DRAGGED_CORPSE = 8;
        #endregion

        #region Stumbling 
        public const int JUMP_STUMBLE_CHANCE = 25;
        public const int JUMP_STUMBLE_ACTION_COST = BASE_ACTION_COST;
        #endregion

        #region Scents
        /// <summary>
        /// How much scent living actors drop on their tile.
        /// </summary>
        public const int LIVING_SCENT_DROP = OdorScent.MAX_STRENGTH;

        /// <summary>
        /// How much scent undead masters drop on their tile.
        /// </summary>
        public const int UNDEAD_MASTER_SCENT_DROP = OdorScent.MAX_STRENGTH;
        #endregion

        #region Barricading
        public const int BARRICADING_MAX = 2 * DoorWindow.BASE_HITPOINTS;
        #endregion

        #region FOV
        const int MINIMAL_FOV = 2;
        #endregion

        #region Day/Night and Weather effects
        public const int FOV_PENALTY_SUNSET = 1;
        public const int FOV_PENALTY_EVENING = 2;
        public const int FOV_PENALTY_MIDNIGHT = 3;
        public const int FOV_PENALTY_DEEP_NIGHT = 4;
        public const int FOV_PENALTY_SUNRISE = 2;
        public const int NIGHT_STA_PENALTY = 2;

        public const int FOV_PENALTY_RAIN = 1;
        public const int FOV_PENALTY_HEAVY_RAIN = 2;
        #endregion

        #region Weapons & Firing
        public const int MELEE_WEAPON_BREAK_CHANCE = 1;
        public const int MELEE_WEAPON_FRAGILE_BREAK_CHANCE = 3;
        public const int MELEE_DISARM_BASE_CHANCE = 5;  // alpha10
        public const int FIREARM_JAM_CHANCE_NO_RAIN = 1;
        public const int FIREARM_JAM_CHANCE_RAIN = 3;
        const float FIRING_WHEN_STA_TIRED = 0.75f;     // -25%
        const float FIRING_WHEN_STA_NOT_FULL = 0.90f;  // -10%
        // alpha10 made into constants
        const float FIRING_WHEN_SLP_EXHAUSTED = 0.50f; // -50%
        const float FIRING_WHEN_SLP_SLEEPY = 0.75f; // -25%
        #endregion

        #region Body armors
        public const int BODY_ARMOR_BREAK_CHANCE = 2;
        #endregion

        #region Hunger/Rot & Sleep & Sanity
        public const int FOOD_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 48;
        public const int FOOD_HUNGRY_LEVEL = FOOD_BASE_POINTS / 2;

        public const int ROT_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int ROT_HUNGRY_LEVEL = ROT_BASE_POINTS / 2;

        public const int SLEEP_BASE_POINTS = WorldTime.TURNS_PER_HOUR * 60;  // 60 = starting game at midnight => sleepy in late evening.
        public const int SLEEP_SLEEPY_LEVEL = SLEEP_BASE_POINTS / 2;

        public const int SANITY_BASE_POINTS = WorldTime.TURNS_PER_DAY * 4;
        public const int SANITY_UNSTABLE_LEVEL = SANITY_BASE_POINTS / 2;
        public const int SANITY_NIGHTMARE_CHANCE = 2;
        public const int SANITY_NIGHTMARE_SLP_LOSS = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_NIGHTMARE_SAN_LOSS = WorldTime.TURNS_PER_HOUR;
        public const int SANITY_NIGHTMARE_STA_LOSS = 10 * STAMINA_COST_RUNNING;  // alpha10 -- worth running for 10 turns
        public const int SANITY_INSANE_ACTION_CHANCE = 5;

        public const int SANITY_HIT_BUTCHERING_CORPSE = WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_UNDEAD_EATING_CORPSE = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_LIVING_EATING_CORPSE = 4 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_EATEN_ALIVE = 4 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_ZOMBIFY = 2 * WorldTime.TURNS_PER_HOUR;
        public const int SANITY_HIT_BOND_DEATH = 8 * WorldTime.TURNS_PER_HOUR;

        // alpha10 increased san recovery; chating & trading also recover san
        public const int SANITY_RECOVER_KILL_UNDEAD = 3 * WorldTime.TURNS_PER_HOUR;  // was 2h
        public const int SANITY_RECOVER_BOND_CHANCE = 5;
        public const int SANITY_RECOVER_BOND = 4 * WorldTime.TURNS_PER_HOUR;  // was 1h
        public const int SANITY_RECOVER_CHAT_OR_TRADE = 3 * WorldTime.TURNS_PER_HOUR;

        /// <summary>
        /// When starving, chance of dying from starvation each turn.
        /// </summary>
        public const int FOOD_STARVING_DEATH_CHANCE = 5;

        /// <summary>
        /// When eating expired food, chances of vomitting.
        /// </summary>
        public const int FOOD_EXPIRED_VOMIT_CHANCE = 25;

        /// <summary>
        /// Stamina lost for vomitting.
        /// </summary>
        public const int FOOD_VOMIT_STA_COST = 100;

        /// <summary>
        /// When rot-starving %% chance to loose an HP.
        /// </summary>
        public const int ROT_STARVING_HP_CHANCE = 5;

        /// <summary>
        /// When rot-hungry %% chance to loose a skill.
        /// </summary>
        public const int ROT_HUNGRY_SKILL_CHANCE = 5;

        /// <summary>
        /// When exhausted, chance of collapsing each turn.
        /// </summary>
        public const int SLEEP_EXHAUSTION_COLLAPSE_CHANCE = 5;

        /// <summary>
        /// When sleeping on a couch, how many sleep points per turn are regenerated.
        /// </summary>
        const int SLEEP_COUCH_SLEEPING_REGEN = 1 + SLEEP_BASE_POINTS / (12 * WorldTime.TURNS_PER_HOUR);

        /// <summary>
        /// When sleeping out of a couch, how many sleep points per turn are regenerated.
        /// </summary>
        public const int SLEEP_NOCOUCH_SLEEPING_REGEN = (2 * SLEEP_COUCH_SLEEPING_REGEN) / 3;

        /// <summary>
        /// When sleeping on a couch, chance to heal per turn.
        /// </summary>
        public const int SLEEP_ON_COUCH_HEAL_CHANCE = 5;

        /// <summary>
        /// When sleeping and healing, how many HPs regained.
        /// </summary>
        public const int SLEEP_HEAL_HITPOINTS = 2;
        #endregion

        #region Loud noises
        public const int LOUD_NOISE_RADIUS = 5;
        const int LOUD_NOISE_BASE_WAKEUP_CHANCE = 10;
        const int LOUD_NOISE_DISTANCE_BONUS = 10;
        #endregion

        #region Victims dropping items.
        public const int VICTIM_DROP_GENERIC_ITEM_CHANCE = 50;
        public const int VICTIM_DROP_AMMOFOOD_ITEM_CHANCE = 100;
        #endregion

        #region Improvised weapons
        public const int IMPROVED_WEAPONS_FROM_BROKEN_WOOD_CHANCE = 25;
        #endregion

        // alpha10 replaced with rapid fire attacks property of ranged weapons
        /*
        #region Rapid fire
        public const float RAPID_FIRE_FIRST_SHOT_ACCURACY = 0.70f;  // alpha9 was 0.50f;
        public const float RAPID_FIRE_SECOND_SHOT_ACCURACY = 0.50f;  // alpha9 was 0.30f;
        #endregion
        */

        #region Trackers
        public const int ZTRACKINGRADIUS = 6;
        #endregion

        #region Actor weight
        public const int DEFAULT_ACTOR_WEIGHT = 10;
        #endregion

        #region Things on fire
        /// <summary>
        /// When raining, chance per turn to test effects on fire on the map.
        /// 100 = will check every rain turn. Not recommended as it eats CPU.
        /// </summary>
        public const int FIRE_RAIN_TEST_CHANCE = 1;

        /// <summary>
        /// Chance for rain to put out a fire.
        /// </summary>
        public const int FIRE_RAIN_PUT_OUT_CHANCE = 10;
        #endregion

        #region Trust & Bond
        public const int TRUST_NEUTRAL = 0;
        public const int TRUST_TRUSTING_THRESHOLD = 12 * WorldTime.TURNS_PER_HOUR; // 12h of sticking together.
        public const int TRUST_MIN = -TRUST_TRUSTING_THRESHOLD;
        public const int TRUST_MAX = 4 * TRUST_TRUSTING_THRESHOLD;
        public const int TRUST_BOND_THRESHOLD = TRUST_MAX;
        public const int TRUST_BASE_INCREASE = 1;                                       // 1pt per turn.
        public const int TRUST_GOOD_GIFT_INCREASE = 3 * WorldTime.TURNS_PER_HOUR;       // 3 trust-hours gained.
        public const int TRUST_MISC_GIFT_INCREASE = TRUST_BASE_INCREASE + TRUST_GOOD_GIFT_INCREASE / 10;
        public const int TRUST_GIVE_ITEM_ORDER_PENALTY = -WorldTime.TURNS_PER_HOUR;     // 1 trust-hours lost.
        public const int TRUST_LEADER_KILL_ENEMY = 3 * WorldTime.TURNS_PER_HOUR;        // 3 trust-hours gain.
        #endregion

        #region Murder
        public const int MURDERER_SPOTTING_BASE_CHANCE = 5;
        public const int MURDERER_SPOTTING_DISTANCE_PENALTY = 1;
        public const int MURDER_SPOTTING_MURDERCOUNTER_BONUS = 5;
        #endregion

        #region Infection & Corpses
        const float INFECTION_BASE_FACTOR = 1.0f;

        public static int INFECTION_LEVEL_1_WEAK  = 10;
        public static int INFECTION_LEVEL_2_TIRED = 30;
        public static int INFECTION_LEVEL_3_VOMIT = 50;
        public static int INFECTION_LEVEL_4_BLEED = 75;
        public static int INFECTION_LEVEL_5_DEATH = 100;

        public static int INFECTION_LEVEL_1_WEAK_STA = 24;//16;
        public static int INFECTION_LEVEL_2_TIRED_STA = 24;//16;
        public static int INFECTION_LEVEL_2_TIRED_SLP = 3 * WorldTime.TURNS_PER_HOUR;//2 * WorldTime.TURNS_PER_HOUR;
        public static int INFECTION_LEVEL_4_BLEED_HP = 6;//4;

        public static int INFECTION_EFFECT_TRIGGER_CHANCE_1000 = (int)(1000 * 2.0f / WorldTime.TURNS_PER_DAY);

        const int CORPSE_ZOMBIFY_BASE_CHANCE = 0;
        const int CORPSE_ZOMBIFY_DELAY = 6 * WorldTime.TURNS_PER_HOUR;
        const float CORPSE_ZOMBIFY_INFECTIONP_FACTOR = 1f; // 1% infection = X% to raise when checked.
        const float CORPSE_ZOMBIFY_NIGHT_FACTOR = 2.0f;
        const float CORPSE_ZOMBIFY_DAY_FACTOR = 0.01f;
        const float CORPSE_ZOMBIFY_TIME_FACTOR = 1f / WorldTime.TURNS_PER_DAY; // -1% per day
        const float CORPSE_EATING_NUTRITION_FACTOR = 10.0f;
        const float CORPSE_EATING_INFECTION_FACTOR = 0.1f;

        const float CORPSE_DECAY_PER_TURN = 4f / WorldTime.TURNS_PER_DAY; // -1 HP per day ~ 1 week to rot at 30 HP.
        #endregion

        #region Refugees
        public const int GIVE_RARE_ITEM_DAY = 7;
        public const int GIVE_RARE_ITEM_CHANCE = 5;
        #endregion

        // alpha10
        #region Traps
        public const int TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY = 30;
        public const int TRAP_SMALL_ACTOR_AVOID_BONUS = 90;
        public const int CRUSHING_GATES_DAMAGE = 60;  // alpha10.1
        #endregion

        #region Skills limits & bonuses per level
        public const int UPGRADE_SKILLS_TO_CHOOSE_FROM = 5;
        public const int UNDEAD_UPGRADE_SKILLS_TO_CHOOSE_FROM = 2;

        /**
         * Actual skill values will be read from Skills.csv so they are not const.
         */
        // alpha10 updated default values to their currnt values in Skills.csv, was confusing.
        #region Livings

        public static int SKILL_AGILE_ATK_BONUS = 2;
        public static int SKILL_AGILE_DEF_BONUS = 4;

        public static float SKILL_AWAKE_SLEEP_BONUS = 0.10f;
        public static float SKILL_AWAKE_SLEEP_REGEN_BONUS = 0.15f;

        public static int SKILL_BOWS_ATK_BONUS = 10;
        public static int SKILL_BOWS_DMG_BONUS = 4;

        public static float SKILL_CARPENTRY_BARRICADING_BONUS = 0.15f;
        public static int SKILL_CARPENTRY_LEVEL3_BUILD_BONUS = 1;

        public static int SKILL_CHARISMATIC_TRUST_BONUS = 2;
        public static int SKILL_CHARISMATIC_TRADE_BONUS = 10;

        public static int SKILL_FIREARMS_ATK_BONUS = 10;
        public static int SKILL_FIREARMS_DMG_BONUS = 2;

        public static int SKILL_HARDY_HEAL_CHANCE_BONUS = 1;

        public static int SKILL_HAULER_INV_BONUS = 1;

        public static int SKILL_HIGH_STAMINA_STA_BONUS = 8;

        public static int SKILL_LEADERSHIP_FOLLOWER_BONUS = 1;

        public static float SKILL_LIGHT_EATER_MAXFOOD_BONUS = 0.10f;
        public static float SKILL_LIGHT_EATER_FOOD_BONUS = 0.15f;

        public static int SKILL_LIGHT_FEET_TRAP_BONUS = 15; // alpha10 prev value was 5

        public static int SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS = 20;  // alpha10 prev value was 10

        public static int SKILL_MARTIAL_ARTS_ATK_BONUS = 6;
        public static int SKILL_MARTIAL_ARTS_DMG_BONUS = 2;
        public static int SKILL_MARTIAL_ARTS_DISARM_BONUS = 10;  // alpha10

        public static float SKILL_MEDIC_BONUS = 0.15f;
        public static int SKILL_MEDIC_REVIVE_BONUS = 10;
        public static int SKILL_MEDIC_LEVEL_FOR_REVIVE_EST = 1;

        public static int SKILL_NECROLOGY_UNDEAD_BONUS = 2;
        public static int SKILL_NECROLOGY_CORPSE_BONUS = 4;
        public static int SKILL_NECROLOGY_LEVEL_FOR_INFECTION = 3;
        public static int SKILL_NECROLOGY_LEVEL_FOR_RISE = 5;

        public static float SKILL_STRONG_PSYCHE_LEVEL_BONUS = 0.15f;

        public static int SKILL_STRONG_DMG_BONUS = 2;
        public static int SKILL_STRONG_THROW_BONUS = 1;
        public static int SKILL_STRONG_RESIST_DISARM_BONUS = 5;  // alpha10

        public static int SKILL_TOUGH_HP_BONUS = 6;

        public static int SKILL_UNSUSPICIOUS_BONUS = 20;  // alpha10

        public static int UNSUSPICIOUS_BAD_OUTFIT_PENALTY = 75;  // alpha10 ; prev was 50
        public static int UNSUSPICIOUS_GOOD_OUTFIT_BONUS = 75; // alpha10 ; prev was 50

        #endregion

        #region Undeads
        public static int SKILL_ZAGILE_ATK_BONUS = 1;
        public static int SKILL_ZAGILE_DEF_BONUS = 2;

        public static int SKILL_ZSTRONG_DMG_BONUS = 2;

        public static int SKILL_ZTOUGH_HP_BONUS = 4;

        public static float SKILL_ZEATER_REGEN_BONUS = 0.20f;

        public static float SKILL_ZTRACKER_SMELL_BONUS = 0.10f;

        public static int SKILL_ZLIGHT_FEET_TRAP_BONUS = 3;

        public static int SKILL_ZGRAB_CHANCE = 4;  // alpha10 prev was 2

        public static float SKILL_ZINFECTOR_BONUS = 0.15f;

        public static float SKILL_ZLIGHT_EATER_MAXFOOD_BONUS = 0.15f;
        public static float SKILL_ZLIGHT_EATER_FOOD_BONUS = 0.10f;
        #endregion

        #endregion
        #endregion

        #region Fields
        readonly DiceRoller m_DiceRoller;
        #endregion

        #region Properties
        public DiceRoller DiceRoller { get { return m_DiceRoller; } }
        #endregion

        #region Init
        public Rules(DiceRoller diceRoller)
        {
            if (diceRoller == null)
                throw new ArgumentNullException("diceRoller");

            m_DiceRoller = diceRoller;
        }
        #endregion

        #region Rolling & Random choices
        /// <summary>
        /// Roll in range [min, max[.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int Roll(int min, int max)
        {
            return m_DiceRoller.Roll(min, max);
        }

        public bool RollChance(int chance)
        {
            return m_DiceRoller.RollChance(chance);
        }

        public float RollFloat()
        {
            return m_DiceRoller.RollFloat();
        }

        /// <summary>
        /// Apply a random deviation to a value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="deviation">amount (not percentage!) of deviation from value</param>
        /// <returns></returns>
        public float Randomize(float value, float deviation)
        {
            float halfDeviation = deviation/2f;
            return value - halfDeviation * m_DiceRoller.RollFloat() + halfDeviation * m_DiceRoller.RollFloat();
        }

        public int RollX(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            return m_DiceRoller.Roll(0, map.Width);
        }

        public int RollY(Map map)
        {
            if (map == null)
                throw new ArgumentNullException("map");

            return m_DiceRoller.Roll(0, map.Height);
        }

        public Direction RollDirection()
        {
            return Direction.COMPASS[m_DiceRoller.Roll(0, 8)];
        }

        /// <summary>
        /// [0,skillValue]
        /// </summary>
        /// <param name="skillValue"></param>
        /// <returns></returns>
        public int RollSkill(int skillValue)
        {
            if (skillValue <= 0)
                return 0;
            // bell curve results = less extremes => favor higher skill vs lower skill
            return (m_DiceRoller.Roll(0, skillValue + 1) + m_DiceRoller.Roll(0, skillValue + 1)) / 2;
        }

        /// <summary>
        /// [damageValue/2, damageValue] 
        /// </summary>
        /// <param name="damageValue"></param>
        /// <returns></returns>
        public int RollDamage(int damageValue)
        {
            if (damageValue <= 0)
                return 0;
            return m_DiceRoller.Roll(damageValue / 2, damageValue + 1);
        }

        public Location RollNeighbourInMap(Location from)
        {
            while (true)
            {
                Location next = from + RollDirection();

                if (next.Map != from.Map)
                    continue;

                if (from.Map.IsInBounds(next.Position.X, next.Position.Y))
                    return next;
            }
        }

        public bool RollValidMoveInMap(Actor actor, int maxTries, out Location nextLocation)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                nextLocation = RollNeighbourInMap(actor.Location);

                if (IsWalkableFor(actor, nextLocation.Map, nextLocation.Position.X, nextLocation.Position.Y))
                    return true;
            }

            nextLocation = actor.Location;
            return false;
        }

        public bool RollValidMoveDirection(Actor actor, int maxTries, out Direction direction)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                direction = RollDirection();
                Location nextLocation = actor.Location + direction;
                if (nextLocation.Map != actor.Location.Map)
                    continue;
                if (IsWalkableFor(actor, nextLocation))
                    return true;
            }

            direction = null;
            return false;
        }

        public bool RollValidBumpDirection(Actor actor, int maxTries, out Direction direction)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            for (int i = 0; i < maxTries; i++)
            {
                direction = RollDirection();
                Location nextLocation = actor.Location + direction;
                if (nextLocation.Map != actor.Location.Map)
                    continue;
                return true;
            }

            direction = null;
            return false;
        }
        #endregion

        #region Rules checking

        #region Items
        public bool CanActorGetItemFromContainer(Actor actor, Point position)
        {
            string reason;
            return CanActorGetItemFromContainer(actor, position, out reason);
        }

        public bool CanActorGetItemFromContainer(Actor actor, Point position, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            ////////////////////////////////////
            // Cant if any is true:
            // 1. Map object is not a container.
            // 2. There is no items there.
            // 3. Actor cannot take the item.
            ////////////////////////////////////

            // 1. Map object is not a container.
            MapObject mapObj = actor.Location.Map.GetMapObjectAt(position);
            if (mapObj == null || !mapObj.IsContainer)
            {
                reason = "object is not a container";
                return false;
            }

            // 2. There is no items there.
            Inventory invThere = actor.Location.Map.GetItemsAt(position);
            if (invThere == null || invThere.IsEmpty)
            {
                reason = "nothing to take there";
                return false;
            }

            // 3. Actor cannot take the item.
            if (!actor.Model.Abilities.HasInventory ||
                !actor.Model.Abilities.CanUseMapObjects ||
                actor.Inventory == null ||
                !CanActorGetItem(actor, invThere.TopItem))
            {
                reason = "cannot take an item";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorGetItem(Actor actor, Item it)
        {
            string reason;
            return CanActorGetItem(actor, it, out reason);
        }

        public bool CanActorGetItem(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            //////////////////////////////////////////////
            // Cant if any is true:
            // 1. Actor cannot take items.
            // 2. Inventory is full and cannot stack item
            // 3. Triggered trap.
            //////////////////////////////////////////////

            // 1. Actor cannot take items
            if (!actor.Model.Abilities.HasInventory ||
                !actor.Model.Abilities.CanUseMapObjects ||
                actor.Inventory == null)
            {
                reason = "no inventory";
                return false;
            }
            if (actor.Inventory.IsFull && !actor.Inventory.CanAddAtLeastOne(it))
            {
                reason = "inventory is full";
                return false;
            }
            if (it is ItemTrap && (it as ItemTrap).IsTriggered)
            {
                reason = "triggered trap";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorEquipItem(Actor actor, Item it)
        {
            string reason;
            return CanActorEquipItem(actor, it, out reason);
        }

        public bool CanActorEquipItem(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            ////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Item not equipable.
            // (DISABLED 3. No free slot on doll.)
            ////////////////////////////

            // 1. Actor cant use items.
            if (!actor.Model.Abilities.CanUseItems)
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Item not equipable.
            if (!it.Model.IsEquipable)
            {
                reason = "this item cannot be equipped";
                return false;
            }

            // (DISABLED 3. No free slot on doll.)
            // reason: equipping automatically unequip other first.
#if false
            if (actor.GetEquippedItem(it.Model.EquipmentPart) != null)
            {
                reason = "equipment slot not free";
                return false;
            }
#endif

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorUnequipItem(Actor actor, Item it)
        {
            string reason;
            return CanActorUnequipItem(actor, it, out reason);
        }

        public bool CanActorUnequipItem(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            /////////////////////////
            // Can't if any is true:
            // 1. Not equipped.
            // 2. Not in inventory.
            ////////////////////////

            // 1. Not equipped.
            if (!it.IsEquipped)
            {
                reason = "not equipped";
                return false;
            }

            // 2. Not in inventory.
            Inventory inv = actor.Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorDropItem(Actor actor, Item it)
        {
            string reason;
            return CanActorDropItem(actor, it, out reason);
        }

        public bool CanActorDropItem(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            /////////////////////////
            // Can't if any is true:
            // 1. Equipped.
            // 2. Not in inventory.
            ////////////////////////

            // 1. Equipped.
            if(it.IsEquipped)
            {
                reason = "unequip first";
                return false;
            }

            // 2. Not in inventory.
            Inventory inv = actor.Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorUseItem(Actor actor, Item it)
        {
            string reason;
            return CanActorUseItem(actor, it, out reason);
        }

        public bool CanActorUseItem(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            //////////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Actor cant use this specific kind of items.
            // (unused 3. Actor cant use item in his present state.)
            // 4. Not in inventory.
            //////////////////////////////////////////////////

            // 1. Actor cant use items.
            if (!(actor.Model.Abilities.CanUseItems))
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Actor cant use this specific kind of items.
            if (it is ItemWeapon)
            {
                reason = "to use a weapon, equip it";
                return false;
            }
            else if (it is ItemFood && !actor.Model.Abilities.HasToEat)
            {
                reason = "no ability to eat";
                return false;
            }
            else if (it is ItemMedicine && actor.Model.Abilities.IsUndead)
            {
                reason = "undeads cannot use medecine";
                return false;
            }
            else if (it is ItemBarricadeMaterial)
            {
                reason = "to use material, build a barricade";
                return false;
            }
            else if (it is ItemAmmo)
            {
                ItemAmmo ammoIt = it as ItemAmmo;
                /////////////////////////////////////
                // Can't use ammo if
                // 1. No compatible weapon equipped.
                // 2. Weapon fully loaded.
                /////////////////////////////////////
                // 1. No compatible weapon equipped.
                ItemRangedWeapon eqRanged = actor.GetEquippedWeapon() as ItemRangedWeapon;
                if (eqRanged == null || eqRanged.AmmoType != ammoIt.AmmoType)
                {
                    reason = "no compatible ranged weapon equipped";
                    return false;
                }
                // 2. Weapon fully loaded.
                if(eqRanged.Ammo >= (eqRanged.Model as ItemRangedWeaponModel).MaxAmmo)
                {
                    reason = "weapon already fully loaded";
                    return false;
                }
                // fine!
            }
            // alpha10 new way to use spray scent
            //else if (it is ItemSprayScent)
            //{
            //    ItemSprayScent spray = it as ItemSprayScent;
            //    // can't if empty.
            //    if (spray.SprayQuantity <= 0)
            //    {
            //        reason = "no spray left.";
            //        return false;
            //    }
            //    // fine!
            //}
            else if (it is ItemTrap)
            {
                ItemTrap trap = it as ItemTrap;
                if (!trap.TrapModel.UseToActivate)
                {
                    reason = "does not activate manually";
                    return false;
                }
            }
            else if (it is ItemEntertainment)
            {
                if (!actor.Model.Abilities.IsIntelligent)
                {
                    reason = "not intelligent";
                    return false;
                }
                if ((it as ItemEntertainment).IsBoringFor(actor)) // alpha10 boring items item centric
                {
                    reason = "bored by this";
                    return false;
                }
            }

            // (3. Actor cant use item in his present state.)
            // todo if needed.

            // 4. Not in inventory.
            Inventory inv = actor.Inventory;
            if (inv == null || !inv.Contains(it))
            {
                reason = "not in inventory";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorEatFoodOnGround(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");


            ///////////////////////////////
            // Can't if any is true:
            // 1. Item not food.
            // 2. Item not in actor tile.
            //////////////////////////////

            // 1. Item not food.
            if (!(it is ItemFood))
            {
                reason = "not food";
                return false;
            }

            // 2. Item not in actor tile.
            Inventory stackHere = actor.Location.Map.GetItemsAt(actor.Location.Position);
            if (stackHere == null || !stackHere.Contains(it))
            {
                reason = "item not here";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public bool CanActorRechargeItemBattery(Actor actor, Item it, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (it == null)
                throw new ArgumentNullException("item");

            //////////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor cant use items.
            // 2. Item not equipped by actor.
            // 3. Not a battery powered item.
            //////////////////////////////////////////////////

            // 1. Actor cant use items.
            if (!(actor.Model.Abilities.CanUseItems))
            {
                reason = "no ability to use items";
                return false;
            }

            // 2. Item not equipped.            
            if (!it.IsEquipped || !actor.Inventory.Contains(it))
            {
                reason = "item not equipped";
                return false;
            }

            // 3. Not a battery powered item.
            if (!IsItemBatteryPowered(it))
            {
                reason = "not a battery powered item";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool IsItemBatteryPowered(Item it)
        {
            if (it == null) return false;
            return (it is ItemLight || it is ItemTracker);
        }

        public bool IsItemBatteryFull(Item it)
        {
            if (it == null) return true;
            ItemLight light = it as ItemLight;
            if (light != null && light.IsFullyCharged) return true;
            ItemTracker tracker = it as ItemTracker;
            if (tracker != null && tracker.IsFullyCharged) return true;
            return false;
        }

        public bool CanActorGiveItemTo(Actor actor, Actor target, Item gift, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");
            if (gift == null)
                throw new ArgumentNullException("gift");

            /////////////////////////////////
            // Can't if any is true:
            // 1. Target is enemy.
            // 2. Item is equipped.
            // 3. Target is sleeping.
            // 4. Target can't receive item.
            /////////////////////////////////

            // 1. Target is enemy.
            if (AreEnemies(actor, target))
            {
                reason = "enemy";
                return false;
            }

            // 2. Item is equipped.
            if (gift.IsEquipped)
            {
                reason = "equipped";
                return false;
            }

            // 3. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 4. Target can't receive item.
            if (!CanActorGetItem(target, gift, out reason))
            {
                return false;
            }

            // all clear.
            return true;
        }

        // alpha10
        public bool CanActorSprayOdorSuppressor(Actor actor, ItemSprayScent suppressor, Actor sprayOn)
        {
            string reason;
            return CanActorSprayOdorSuppressor(actor, suppressor, sprayOn, out reason);
        }

        // alpha10
        public bool CanActorSprayOdorSuppressor(Actor actor, ItemSprayScent suppressor, Actor sprayOn, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (suppressor == null)
                throw new ArgumentNullException("suppressor");
            if (sprayOn == null)
                throw new ArgumentNullException("target");

            ////////////////////////////////////////////////////////
            // Cant if any is true:
            // 1. Actor cannot use items
            // 2. Not an odor suppressor
            // 3. Spray is not equiped by actor or has no spray left.
            // 4. SprayOn is not self or adjacent.
            ////////////////////////////////////////////////////////

            // 1. Actor cannot use items
            if (!actor.Model.Abilities.CanUseItems)
            {
                reason = "cannot use items";
                return false;
            }

            // 2. Not an odor suppressor
            if (suppressor.Odor != Odor.SUPPRESSOR)
            {
                reason = "not an odor suppressor";
                return false;
            }

            // 2. Spray is not equiped by actor or has no spray left.
            if (suppressor.SprayQuantity <= 0)
            {
                reason = "no spray left";
                return false;
            }
            if (!(suppressor.IsEquipped && (actor.Inventory != null && actor.Inventory.Contains(suppressor))))
            {
                reason = "spray not equipped";
                return false;
            }

            // 3. SprayOn is not self or adjacent.
            if (sprayOn != actor)
            {
                if (!(actor.Location.Map == sprayOn.Location.Map && IsAdjacent(actor.Location.Position, sprayOn.Location.Position)))
                {
                    reason = "not adjacent";
                    return false;
                }
            }
            
            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Movement/Melee
        public bool CanActorRun(Actor actor)
        {
            string reason;
            return CanActorRun(actor, out reason);
        }

        public bool CanActorRun(Actor actor, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            //////////////////////////////
            // Cant run if any is true:
            // 1. Has not CanRun ability.
            // 2. Stamina below min level.
            //////////////////////////////

            // 1. Has not CanRun ability.
            if (!actor.Model.Abilities.CanRun)
            {
                reason = "no ability to run";
                return false;
            }

            // 2. Stamina below min level.
            if (actor.StaminaPoints < STAMINA_MIN_FOR_ACTIVITY)
            {
                reason = "not enough stamina to run";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool IsActorTired(Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            return actor.Model.Abilities.CanTire && actor.StaminaPoints < STAMINA_MIN_FOR_ACTIVITY;
        }

        public bool CanActorMeleeAttack(Actor actor, Actor target)
        {
            string reason;
            return CanActorMeleeAttack(actor, target, out reason);
        }

        public bool CanActorMeleeAttack(Actor actor, Actor target, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////
            // Cant run if any is true:
            // 1. Target not adjacent in map or not sharing an exit.
            // 2. Stamina below min level.
            // 3. Target is dead (doh!).
            //////////////////////////////

            // 1. Target not adjacent in map or not sharing an exit.
            if (actor.Location.Map == target.Location.Map)
            {
                if (!IsAdjacent(actor.Location.Position, target.Location.Position))
                {
                    reason = "not adjacent";
                    return false;
                }
            }
            else
            {
                // check there is an exit at both positions.
                Exit fromExit = actor.Location.Map.GetExitAt(actor.Location.Position);
                if (fromExit == null)
                {
                    reason = "not reachable";
                    return false;
                }
                Exit toExit = target.Location.Map.GetExitAt(target.Location.Position);
                if (toExit == null)
                {
                    reason = "not reachable";
                    return false;
                }
                // check the target stands on the exit.
                if (fromExit.ToMap != target.Location.Map ||fromExit.ToPosition != target.Location.Position)
                {
                    reason = "not reachable";
                    return false;
                }                                
            }

            // 2. Stamina below min level.
            if (actor.StaminaPoints < STAMINA_MIN_FOR_ACTIVITY)
            {
                reason = "not enough stamina to attack";
                return false;
            }

            // 3. Target is dead (doh!).
            // oddly this can happen for the AI when simulating... not clear why...
            // so this is a lame fix to prevent KillingActor from throwing an exception :-)
            if (target.IsDead)
            {
                reason = "already dead!";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool HasActorJumpAbility(Actor actor)
        {
            return actor.Model.Abilities.CanJump || 
                actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) > 0 ||
                actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE) > 0;
        }

        public bool IsWalkableFor(Actor actor, Map map, int x, int y)
        {
            string reason;
            return IsWalkableFor(actor, map, x, y, out reason);
        }

        public bool IsWalkableFor(Actor actor, Map map, int x, int y, out string reason)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (actor == null)
                throw new ArgumentNullException("actor");

            ////////////////////////////////
            // Not walkable if any is true:
            // 1. Out of map.
            // 2. Tile not walkable.
            // 3. Map object not passable.
            // 4. An actor already there.
            // 5. Dragging a corpse when tired.
            ////////////////////////////////
            // 1. Out of map.
            if (!map.IsInBounds(x, y))
            {
                reason = "out of map";
                return false;
            }

            // 2. Tile not walkable.
            if (!map.GetTileAt(x, y).Model.IsWalkable)
            {
                reason = "blocked";
                return false;
            }

            // 3. Map object not passable.
            MapObject mapObj = map.GetMapObjectAt(x, y);
            if (mapObj != null)
            {
                if (!mapObj.IsWalkable)
                {
                    // jump?
                    if (mapObj.IsJumpable)
                    {
                        if (!HasActorJumpAbility(actor))
                        {
                            reason = "cannot jump";
                            return false;
                        }
                        if (actor.StaminaPoints < STAMINA_COST_JUMP)
                        {
                            reason = "not enough stamina to jump";
                            return false;
                        }
                    }
                    // small actor blocked only by closed doors.
                    else if (actor.Model.Abilities.IsSmall)
                    {
                        DoorWindow door = mapObj as DoorWindow;
                        if (door != null && door.IsClosed)
                        {
                            reason = "cannot slip through closed door";
                            return false;
                        }
                    }
                    else
                    {
                        // nope, blocking object.
                        reason = "blocked by object";
                        return false;
                    }
                }
            }

            // 4. An actor already there.
            if (map.GetActorAt(x, y) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. Dragging a corpse when tired.
            if (actor.DraggedCorpse != null && IsActorTired(actor))
            {
                reason = "dragging a corpse when tired";
                return false;
            }

            // all checks clear.
            reason = "";
            return true;
        }

        public bool IsWalkableFor(Actor actor, Location location)
        {
            string reason;
            return IsWalkableFor(actor, location, out reason);
        }

        public bool IsWalkableFor(Actor actor, Location location, out string reason)
        {
            return IsWalkableFor(actor, location.Map, location.Position.X, location.Position.Y, out reason);
        }

        public ActorAction IsBumpableFor(Actor actor, RogueGame game, Map map, int x, int y, out string reason)
        {
            if (map == null)
                throw new ArgumentNullException("map");
            if (actor == null)
                throw new ArgumentNullException("actor");
            reason = "";

            ///////////////////////////////
            // Out of map : leave district?
            ///////////////////////////////
            if (!map.IsInBounds(x, y))
            {
                if (CanActorLeaveMap(actor, out reason))
                {
                    reason = "";
                    return new ActionLeaveMap(actor, game, new Point(x, y));
                }
                else
                {
                    return null;
                }
            }

            //////////////////////////////////////////
            // 1. Movement?
            // 2. Actor interact: fight or chat?
            // 3. Object interact?
            // 4. No action possible.
            //////////////////////////////////////////
            Point to = new Point(x, y);

            // 1. Move?
            ActionMoveStep moveAction = new ActionMoveStep(actor, game, to);
            if (moveAction.IsLegal())
            {
                reason = "";
                return moveAction;
            }
            else 
                reason = moveAction.FailReason;

            // 2. Actor interact: fight/chat/(AI:switch place)
            #region
            Actor targetActor = map.GetActorAt(to);
            if (targetActor != null)
            {
                // attacking?
                if (AreEnemies(actor, targetActor))
                {
                    if (CanActorMeleeAttack(actor, targetActor, out reason))
                        return new ActionMeleeAttack(actor, game, targetActor);
                    else
                        return null;
                }

                // AI: switching place  // alpha10.1 handle bot like it was an AI
                if((!actor.IsPlayer || actor.IsBotPlayer) && 
                    !targetActor.IsPlayer 
                    && CanActorSwitchPlaceWith(actor, targetActor, out reason))
                    return new ActionSwitchPlace(actor, game, targetActor);

                // chatting?
                if (CanActorChatWith(actor, targetActor, out reason))
                    return new ActionChat(actor, game, targetActor);

                // nope, blocked.
                return null;
            }
            #endregion

            // 3. Object interact?
            #region
            MapObject mapObj = map.GetMapObjectAt(to);
            if (mapObj != null)
            {
                // 3.1 Door?
                #region
                DoorWindow door = mapObj as DoorWindow;
                if (door != null)
                {
                    if (door.IsClosed)  // closed: open/bash/barricade
                    {
                        if (IsOpenableFor(actor, door, out reason))
                            return new ActionOpenDoor(actor, game, door);
                        else if (IsBashableFor(actor, door, out reason))
                            return new ActionBashDoor(actor, game, door);
                        else
                            return null;
                    }
                    if (door.BarricadePoints > 0) // barricaded: bash
                    {
                        if (IsBashableFor(actor, door, out reason))
                            return new ActionBashDoor(actor, game, door);
                        else
                        {
                            reason = "cannot bash the barricade";
                            return null;
                        }
                    }
                }
                #endregion

                // 3.2 Container?
                if (CanActorGetItemFromContainer(actor, to, out reason))
                    return new ActionGetFromContainer(actor, game, to);

                // alpha10.1 removed break restriction, made civs ai get stuck in some rare cases but we punish break actions in baseai wander.
                //// 3.3 Break?
                if (IsBreakableFor(actor, mapObj, out reason))
                    return new ActionBreak(actor, game, mapObj);
                //// 3.3 Break? Only allow bashing actors to do that (don't want Civilians to bash stuff on their way)
                //if (actor.Model.Abilities.CanBashDoors && IsBreakableFor(actor, mapObj, out reason))
                //    return new ActionBreak(actor, game, mapObj);

                // 3.4 Power Generator?
                PowerGenerator powGen = mapObj as PowerGenerator;
                if (powGen != null)
                {
                    // Recharge battery powered item?
                    if (powGen.IsOn)
                    {
                        Item leftItem = actor.GetEquippedItem(DollPart.LEFT_HAND);
                        if (leftItem != null && CanActorRechargeItemBattery(actor, leftItem, out reason))
                            return new ActionRechargeItemBattery(actor, game, leftItem);
                        Item rightItem = actor.GetEquippedItem(DollPart.RIGHT_HAND);
                        if (rightItem != null && CanActorRechargeItemBattery(actor, rightItem, out reason))
                            return new ActionRechargeItemBattery(actor, game, rightItem);
                    }

                    // Switch?
                    if(IsSwitchableFor(actor, powGen, out reason))
                        // switch it.
                        return new ActionSwitchPowerGenerator(actor, game, powGen);
            
                    // Can do nothing by bumping.
                    return null;
                }

            }
            #endregion

            // 4. No action possible?
            //reason = "blocked";
            return null;

        }

        public ActorAction IsBumpableFor(Actor actor, RogueGame game, Location location)
        {
            string reason;
            return IsBumpableFor(actor, game, location, out reason);
        }

        public ActorAction IsBumpableFor(Actor actor, RogueGame game, Location location, out string reason)
        {
            return IsBumpableFor(actor, game, location.Map, location.Position.X, location.Position.Y, out reason);
        }
        #endregion

        #region Switching Map Objects
        public bool IsSwitchableFor(Actor actor, PowerGenerator powGen, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (powGen == null)
                throw new ArgumentNullException("powGen");


            ////////////////////////
            // Switchable only if:
            // 1. Actor has ability.
            // 2. Is not sleeping.
            ////////////////////////

            // 1. Actor has ability.
            if (!actor.Model.Abilities.CanUseMapObjects)
            {
                reason = "cannot use map objects";
                return false;
            }

            // 2. Is not sleeping.
            if (actor.IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }


            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Leaving maps
        public bool CanActorLeaveMap(Actor actor, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            /////////////////////////////////
            // Only if :
            // 1. Player and not bot // alpha10.1
            /////////////////////////////////
            if(!actor.IsPlayer || actor.IsBotPlayer)
            {
                reason = "can't leave maps";
                return false;
            }

            reason = "";
            return true;
        }
        #endregion

        #region Using exits
        public bool CanActorUseExit(Actor actor, Point exitPoint)
        {
            string reason;
            return CanActorUseExit(actor, exitPoint, out reason);
        }

        public bool CanActorUseExit(Actor actor, Point exitPoint, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            /////////////////////////////
            // Can't if any is true:
            // 1. No exit there.
            // 2. AI: can't use AI exits.
            // 3. Is sleeping.
            /////////////////////////////

            // 1. No exit there.
            if (actor.Location.Map.GetExitAt(exitPoint) == null)
            {
                reason = "no exit there";
                return false;
            }

            // 2. AI: can't use AI exits.
            // alpha10.1 handle bots
            if ((!actor.IsPlayer || actor.IsBotPlayer) && !actor.Model.Abilities.AI_CanUseAIExits)
            {
                reason = "this AI can't use exits";
                return false;
            }

            // 3. Is sleeping.
            if (actor.IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        #endregion

        #region Chatting & Trading
        public bool CanActorChatWith(Actor speaker, Actor target, out string reason)
        {
            if (speaker == null)
                throw new ArgumentNullException("speaker");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////
            // Can't if any is true:
            // 1. One of them can't talk.
            // 2. One of them is sleeping.
            //////////////////////////////

            // 1. One of them can't talk.
            if (!speaker.Model.Abilities.CanTalk)
            {
                reason = "can't talk";
                return false;
            }
            if (!target.Model.Abilities.CanTalk)
            {
                reason = String.Format("{0} can't talk", target.TheName);
                return false;
            }

            // 2. One of them is sleeping.
            if (speaker.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }
            if (target.IsSleeping)
            {
                reason = String.Format("{0} is sleeping", target.TheName);
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorInitiateTradeWith(Actor speaker, Actor target)
        {
            string reason;
            return CanActorInitiateTradeWith(speaker, target, out reason);
        }

        public bool CanActorInitiateTradeWith(Actor speaker, Actor target, out string reason)
        {
            if (speaker == null)
                throw new ArgumentNullException("speaker");
            if (target == null)
                throw new ArgumentNullException("target");

            /////////////////////////////
            // Can't if any is true:
            // 1. Target is player.
            // 2. Actors are not traders and not leader->follower relationship.
            // 3. Target is sleeping.
            // 4. No item to trade.
            /////////////////////////////

            // 1. Target is player.
            if (target.IsPlayer)
            {
                reason = "target is player";
                return false;
            }

            // 2. Actors are not traders and not leader->follower relationship.
            if (!speaker.Model.Abilities.CanTrade && target.Leader != speaker)
            {
                reason = "can't trade";
                return false;
            }
            if (!target.Model.Abilities.CanTrade && target.Leader != speaker)
            {
                reason = "target can't trade";
                return false;
            }

            if (AreEnemies(speaker, target))
            {
                reason = "is an enemy";
                return false;
            }

            // 3. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "is sleeping";
                return false;
            }

            // 4. No item to trade.
            if (speaker.Inventory == null || speaker.Inventory.IsEmpty)
            {
                reason = "nothing to offer";
                return false;
            }
            if (target.Inventory == null || target.Inventory.IsEmpty)
            {
                reason = "has nothing to trade";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorShout(Actor speaker)
        {
            string reason;
            return CanActorShout(speaker, out reason);
        }

        public bool CanActorShout(Actor speaker, out string reason)
        {
            if (speaker == null)
                throw new ArgumentNullException("speaker");

            //////////////////////////
            // Can't shout if:
            // 1. Actor is sleeping.
            // 2. Actor can't talk.
            //////////////////////////

            // 1. Actor is sleeping.
            if (speaker.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 2. Actor can't talk
            if (!speaker.Model.Abilities.CanTalk)
            {
                reason = "can't talk";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Doors
        public bool IsOpenableFor(Actor actor, DoorWindow door)
        {
            string reason;
            return IsOpenableFor(actor, door, out reason);
        }

        public bool IsOpenableFor(Actor actor, DoorWindow door, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (door == null)
                throw new ArgumentNullException("door");

            ////////////////////////////////////////
            // Not openable if any is true:
            // 1. Actor cannot use map objects.
            // 2. Door is not closed nor barricaded.
            ////////////////////////////////////////

            // 1. Actor cannot use map objects.
            if (!actor.Model.Abilities.CanUseMapObjects)
            {
                reason = "no ability to open";
                return false;
            }

            // 2. Door is not closed nor barricaded.
            if (!door.IsClosed || door.BarricadePoints > 0)
            {
                reason = "not closed nor barricaded";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public bool IsClosableFor(Actor actor, DoorWindow door)
        {
            string reason;
            return IsClosableFor(actor, door, out reason);
        }

        public bool IsClosableFor(Actor actor, DoorWindow door, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (door == null)
                throw new ArgumentNullException("door");

            ///////////////////////////////////////
            // Not close if any is true:
            // 1. Actor cannot use map objects.
            // 2. Door is not open.
            // 3. Another actor here.
            ///////////////////////////////////////

            // 1. Actor cannot use map objects.
            if (!actor.Model.Abilities.CanUseMapObjects)
            {
                reason = "can't use objects";
                return false;
            }

            // 2. Door is not open.
            if (!door.IsOpen)
            {
                reason = "not open";
                return false;
            }

            // 3. Another actor here.
            if (door.Location.Map.GetActorAt(door.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public bool CanActorBarricadeDoor(Actor actor, DoorWindow door)
        {
            string reason;
            return CanActorBarricadeDoor(actor, door, out reason);
        }

        public bool CanActorBarricadeDoor(Actor actor, DoorWindow door, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (door == null)
                throw new ArgumentNullException("door");

            ////////////////////////////////////
            // Not close if any is true:
            // 1. Actor cannot barricade doors.
            // 2. Door is not closed or broken.
            // 3. Barricading limit reached.
            // 4. An actor is there.
            // 5. No barricading material.
            ////////////////////////////////////

            // 1. Actor cannot barricade doors.
            if (!actor.Model.Abilities.CanBarricade)
            {
                reason = "no ability to barricade";
                return false;
            }

            // 2. Door is not closed or broken.
            if (door.State != DoorWindow.STATE_CLOSED && door.State != DoorWindow.STATE_BROKEN)
            {
                reason = "not closed or broken"; // alpha10 typo fix
                return false;
            }

            // 3. Barricading limit reached.
            if (door.BarricadePoints >= BARRICADING_MAX)
            {
                reason = "barricade limit reached";
                return false;
            }

            // 4. An actor is there.
            if (door.Location.Map.GetActorAt(door.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. No barricading material.
            if (actor.Inventory == null || actor.Inventory.IsEmpty)
            {
                reason = "no items";
                return false;
            }
            if (!actor.Inventory.HasItemOfType(typeof(ItemBarricadeMaterial)))
            {
                reason = "no barricading material";
                return false;
            }          

            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Bashing doors & Breaking stuff
        public bool IsBashableFor(Actor actor, DoorWindow door)
        {
            string reason;
            return IsBashableFor(actor, door, out reason);
        }

        public bool IsBashableFor(Actor actor, DoorWindow door, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (door == null)
                throw new ArgumentNullException("door");

            ///////////////////////////////
            // Not bashable:
            // 1. Actor cannot bash doors.
            // 2. Actor is tired.
            // 3. Door is not breakable.
            ///////////////////////////////

            // 1. Actor cannot bash doors.
            if (!actor.Model.Abilities.CanBashDoors)
            {
                reason = "can't bash doors";
                return false;
            }

            // 2. Actor is tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }

            // 2. Door is not breakable.
            if (door.BreakState != MapObject.Break.BREAKABLE && !door.IsBarricaded)
            {
                reason = "can't break this object";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public bool IsBreakableFor(Actor actor, MapObject mapObj)
        {
            string reason;
            return IsBreakableFor(actor, mapObj, out reason);
        }

        public bool IsBreakableFor(Actor actor, MapObject mapObj, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (mapObj == null)
                throw new ArgumentNullException("mapObj");

            //////////////////////////////////
            // Not breakable:
            // 1. Actor cannot break.
            // 2. Actor is tired.
            // 3. Map object is not breakable.
            // 4. Another actor there.
            //////////////////////////////////

            // 1. Actor cannot break.
            if (!actor.Model.Abilities.CanBreakObjects)
            {
                reason = "cannot break objects";
                return false;
            }

            // 2. Actor is tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }

            // 3. Map object is not breakable.
            DoorWindow door = mapObj as DoorWindow;
            bool isBarricadedDoor = (door != null && door.IsBarricaded);
            if (mapObj.BreakState != MapObject.Break.BREAKABLE && !isBarricadedDoor)
            {
                reason = "can't break this object";
                return false;
            }

            // 4. Another actor there.
            if (mapObj.Location.Map.GetActorAt(mapObj.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Pushing/Pulling objects & Shoving actors
        public bool HasActorPushAbility(Actor actor)
        {
            return actor.Model.Abilities.CanPush || 
                actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG) > 0 ||
                actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_STRONG) > 0;
        }

        public bool CanActorPush(Actor actor, MapObject mapObj)
        {
            string reason;
            return CanActorPush(actor, mapObj, out reason);
        }

        public bool CanActorPush(Actor actor, MapObject mapObj, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (mapObj == null)
                throw new ArgumentNullException("mapObj");

            //////////////////////////////////
            // Not movable:
            // 1. Actor cannot push/pull.
            // 2. Actor is tired.
            // 3. Map obj is not movable.
            // 4. Another actor there.
            // 5. Map obj is on fire.
            // 6. Actor is dragging a corpse.  // alpha10
            /////////////////////////////////

            // 1. Actor cannot push/pull.
            if (!HasActorPushAbility(actor))
            {
                reason = "cannot push objects";
                return false;
            }

            // 2. Actor is tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }

            // 3. Map obj is not movable.
            if (!mapObj.IsMovable)
            {
                reason = "cannot be moved";
                return false;
            }

            // 4. Another actor there.
            if (mapObj.Location.Map.GetActorAt(mapObj.Location.Position) != null)
            {
                reason = "someone is there";
                return false;
            }

            // 5. Map obj is on fire.
            if (mapObj.IsOnFire)
            {
                reason = "on fire";
                return false;
            }

            // 6. Actor is dragging a corpse.  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            // all clear
            reason = "";
            return true;
        }

        public bool CanPushObjectTo(MapObject mapObj, Point toPos)
        {
            string reason;
            return CanPushObjectTo(mapObj, toPos, out reason);
        }

        public bool CanPushObjectTo(MapObject mapObj, Point toPos, out string reason)
        {
            if (mapObj == null)
                throw new ArgumentNullException("mapObj");

            ///////////////////////////
            // Not pushable there:
            // 1. Out of bounds.
            // 2. Not walkable.
            // 3. Another object there.
            // 4. An actor there.
            ///////////////////////////

            // 1. Out of bounds.
            if (!mapObj.Location.Map.IsInBounds(toPos))
            {
                reason = "out of map";
                return false;
            }

            // 2. Not walkable.
            if (!mapObj.Location.Map.GetTileAt(toPos.X, toPos.Y).Model.IsWalkable)
            {
                reason = "blocked by an obstacle";
                return false;
            }

            // 3. Another object there.
            if (mapObj.Location.Map.GetMapObjectAt(toPos) != null)
            {
                reason = "blocked by an object";
                return false;
            }

            // 4. An actor there.
            if (mapObj.Location.Map.GetActorAt(toPos) != null)
            {
                reason = "blocked by someone";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        // alpha10
        public bool CanPullObject(Actor actor, MapObject mapObj, Point toPos)
        {
            string reason;
            return CanPullObject(actor, mapObj, toPos, out reason);
        }

        // alpha10
        public bool CanPullObject(Actor actor, MapObject mapObj, Point moveToPos, out string reason)
        {
            /////////////////////////////////////////////
            // Basically check if can push and can walk.
            // 1. Actor cannot push.
            // 2. Another object already there.
            // 3. Actor cannot walk to pos.
            /////////////////////////////////////////////

            // 1. Actor cannot push this object.
            if (!CanActorPush(actor, mapObj, out reason))
                return false;

            // 2. Another object already there. eg: actor standing on a bed.
            MapObject otherMobj = actor.Location.Map.GetMapObjectAt(actor.Location.Position);
            if (otherMobj != null)
            {
                reason = string.Format("{0} is blocking", otherMobj.TheName);
                return false;
            }

            // 3. Actor cannot walk to pos.
            if (!IsWalkableFor(actor, new Location(actor.Location.Map, moveToPos), out reason))
                return false;

            // all clear
            reason = "";
            return true;
        }

        // alpha10
        public bool CanPullActor(Actor actor, Actor other, Point moveToPos, out string reason)
        {
            /////////////////////////////////////////////
            // Basically check if can push and can walk.
            // 1. Actor cannot shove.
            // 2. Actor cannot walk to pos.
            /////////////////////////////////////////////

            // 1. Actor cannot shove.
            if (!CanActorShove(actor, other, out reason))
                return false;

            // 3. Actor cannot walk to pos.
            if (!IsWalkableFor(actor, new Location(actor.Location.Map, moveToPos), out reason))
                return false;

            // all clear
            reason = "";
            return true;
        }

        public bool CanActorShove(Actor actor, Actor other, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (other == null)
                throw new ArgumentNullException("other");

            ///////////////////////////////
            // Not "shovable"
            // 1. Actor cannot push/pull.
            // 2. Actor is tired.
            // 3. Actor is dragging corpse  // alpha10
            ///////////////////////////////

            // 1. Actor cannot push/pull.
            if (!HasActorPushAbility(actor))
            {
                reason = "cannot shove people";
                return false;
            }

            // 2. Actor is tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }

            // 3. Actor is dragging corpse  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            // FIXME: in theory, should test if tile is walkable for the pusher. in practice, assume if other can walk here, we can too...
            //        this will cause problems in zombie mode (eg: undead player pushing living down from a car = move on the car...)

            // all clear
            reason = "";
            return true;
        }

        public bool CanShoveActorTo(Actor actor, Point toPos, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            ///////////////////////////
            // Not pushable there:
            // 1. Out of bounds.
            // 2. Not walkable.
            // 3. Unwalkable object.
            // 4. An actor there.
            // 5. Actor is dragging corpse  // alpha10
            ///////////////////////////

            Map map = actor.Location.Map;

            // 1. Out of bounds.
            if (!map.IsInBounds(toPos))
            {
                reason = "out of map";
                return false;
            }

            // 2. Not walkable.
            if (!map.GetTileAt(toPos.X, toPos.Y).Model.IsWalkable)
            {
                reason = "blocked";
                return false;
            }

            // 3. Unwalkable object.
            MapObject obj = map.GetMapObjectAt(toPos);
            if (obj != null && !obj.IsWalkable)
            {
                reason = "blocked by an object";
                return false;
            }

            // 4. An actor there.
            if (map.GetActorAt(toPos) != null)
            {
                reason = "blocked by someone";
                return false;
            }

            // 5. Actor is dragging corpse  // alpha10
            if (actor.DraggedCorpse != null)
            {
                reason = "dragging a corpse";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Targeting, Firing and Throwing
        /// <summary>
        /// List enemies in fov, sorted by distance (closest first).
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="fov"></param>
        /// <returns></returns>
        public List<Actor> GetEnemiesInFov(Actor actor, HashSet<Point> fov)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (fov == null)
                throw new ArgumentNullException("fov");

            List<Actor> list = null;

            foreach (Point pt in fov)
            {
                Actor other = actor.Location.Map.GetActorAt(pt);
                if (other == null)
                    continue;
                if (other == actor)
                    continue;
                if (!AreEnemies(actor, other))
                    continue;

                if (list == null)
                    list = new List<Actor>(3);
                list.Add(other);
            }

            if (list != null)
            {
                list.Sort((a, b) =>
                    {
                        float dA = StdDistance(a.Location.Position, actor.Location.Position);
                        float dB = StdDistance(b.Location.Position, actor.Location.Position);

                        return dA < dB ? -1 :
                            dA > dB ? 1 :
                            0;
                    });
            }

            return list;
        }

        public bool CanActorFireAt(Actor actor, Actor target)
        {
            string reason;
            return CanActorFireAt(actor, target, out reason);
        }

        public bool CanActorFireAt(Actor actor, Actor target, out string reason)
        {
            return CanActorFireAt(actor, target, null, out reason);
        }

        public bool CanActorFireAt(Actor actor, Actor target, List<Point> LoF)
        {
            string reason;
            return CanActorFireAt(actor, target, LoF, out reason);
        }

        public bool CanActorFireAt(Actor actor, Actor target, List<Point> LoF, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");
            
            ///////////////////////////////////////
            // Cannot fire if:
            // 1. No ranged weapon or out of range.
            // 2. No ammo.
            // 3. No LoF.
            // 4. Target is dead (doh!).
            ///////////////////////////////////////
            if (LoF != null)
                LoF.Clear();

            // 1. No ranged weapon or out of range.
            ItemRangedWeapon rangedWeapon = actor.GetEquippedWeapon() as ItemRangedWeapon;
            if (rangedWeapon == null)
            {
                reason = "no ranged weapon equipped";
                return false;
            }
            if (actor.CurrentRangedAttack.Range < GridDistance(actor.Location.Position, target.Location.Position))
            {
                reason = "out of range";
                return false;
            }

            // 2. No ammo.
            if (rangedWeapon.Ammo <= 0)
            {
                reason = "no ammo left";
                return false;
            }

            // 3. No LoF.
            if (!LOS.CanTraceFireLine(actor.Location, target.Location.Position, actor.CurrentRangedAttack.Range, LoF))
            {
                reason = "no line of fire";
                return false;
            }

            // 4. Target is dead (doh!).
            // oddly this can happen for the AI when simulating... not clear why...
            // so this is a lame fix to prevent KillingActor from throwing an exception :-)
            if (target.IsDead)
            {
                reason = "already dead!";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorThrowTo(Actor actor, Point pos, List<Point> LoF)
        {
            string reason;
            return CanActorThrowTo(actor, pos, LoF, out reason);
        }

        public bool CanActorThrowTo(Actor actor, Point pos, List<Point> LoF, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            ///////////////////////////////////////
            // Cannot fire if:
            // 1. No throwable item or out of range.
            // 2. No LoT.
            ///////////////////////////////////////
            if (LoF != null)
                LoF.Clear();

            // 1. No throwable item or out of range.
            ItemGrenade unprimedGrenade = actor.GetEquippedWeapon() as ItemGrenade;
            ItemGrenadePrimed primedGrenade = actor.GetEquippedWeapon() as ItemGrenadePrimed;
            if (unprimedGrenade == null && primedGrenade == null)
            {
                reason = "no grenade equiped";
                return false;
            }
            ItemGrenadeModel model;
            if (unprimedGrenade != null)
                model = unprimedGrenade.Model as ItemGrenadeModel;
            else
                model = (primedGrenade.Model as ItemGrenadePrimedModel).GrenadeModel;
            int maxThrowDist = ActorMaxThrowRange(actor, model.MaxThrowDistance);
            if (GridDistance(actor.Location.Position, pos) > maxThrowDist)
            {
                reason = "out of throwing range";
                return false;
            }

            // 2. No LoT.
            if (!LOS.CanTraceThrowLine(actor.Location, pos, maxThrowDist, LoF))
            {
                reason = "no line of throwing";
                return false;
            }

            // all clear.
            reason = "";
            return true;

        }
        #endregion

        #region Hunger/Rot & Sleep & Sanity
        public bool IsActorHungry(Actor a)
        {
            return a.Model.Abilities.HasToEat && a.FoodPoints <= FOOD_HUNGRY_LEVEL;
        }

        public bool IsActorStarving(Actor a)
        {
            return a.Model.Abilities.HasToEat && a.FoodPoints <= 0;
        }

        public bool IsRottingActorHungry(Actor a)
        {
            return a.Model.Abilities.IsRotting && a.FoodPoints <= ROT_HUNGRY_LEVEL;
        }

        public bool IsRottingActorStarving(Actor a)
        {
            return a.Model.Abilities.IsRotting && a.FoodPoints <= 0;
        }

        public bool IsFoodStillFresh(ItemFood food, int turnCounter)
        {
            if (!food.IsPerishable)
                return true;
            return turnCounter < food.BestBefore.TurnCounter;
        }

        public bool IsFoodExpired(ItemFood food, int turnCounter)
        {
            return food.IsPerishable && turnCounter >= food.BestBefore.TurnCounter && turnCounter < 2 * food.BestBefore.TurnCounter;
        }

        public bool IsFoodSpoiled(ItemFood food, int turnCounter)
        {
            return food.IsPerishable && turnCounter >= 2 * food.BestBefore.TurnCounter;
        }

        public int FoodItemNutrition(ItemFood food, int turnCounter)
        {
            return (IsFoodStillFresh(food, turnCounter) ? food.Nutrition : 
                IsFoodExpired(food, turnCounter) ? 2 * food.Nutrition / 3 :
                food.Nutrition / 3);
        }

        public bool IsActorSleepy(Actor a)
        {
            return a.Model.Abilities.HasToSleep && a.SleepPoints <= SLEEP_SLEEPY_LEVEL;
        }

        public bool IsActorExhausted(Actor a)
        {
            return a.Model.Abilities.HasToSleep && a.SleepPoints <= 0;
        }
        
        public int SleepToHoursUntilSleepy(int sleep, bool isNight)
        {
            int left = sleep - Rules.SLEEP_SLEEPY_LEVEL;
            if (isNight)
                left /= 2;
            if (left <= 0)
                return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }

        public bool IsAlmostSleepy(Actor actor)
        {
            if (!actor.Model.Abilities.HasToSleep)
                return false;
            return SleepToHoursUntilSleepy(actor.SleepPoints, actor.Location.Map.LocalTime.IsNight) <= 3;
        }

        public bool CanActorSleep(Actor actor)
        {
            string reason;
            return CanActorSleep(actor, out reason);
        }

        public bool CanActorSleep(Actor actor, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            /////////////////////////
            // Can't sleep if:
            // 1. Already sleeping.
            // 2. Has not to sleep.
            // 3. Is hungry or worse.
            // 4. No need to sleep.
            /////////////////////////

            // 1. Already sleeping.
            if (actor.IsSleeping)
            {
                reason = "already sleeping";
                return false;
            }

            // 2. Has not to sleep.
            if (!actor.Model.Abilities.HasToSleep)
            {
                reason = "no ability to sleep";
                return false;
            }

            // 3. Is hungry or worse.
            if (IsActorHungry(actor) || IsActorStarving(actor))
            {
                reason = "hungry";
                return false;
            }

            // 4. No need to sleep.
            if (actor.SleepPoints >= ActorMaxSleep(actor) - WorldTime.TURNS_PER_HOUR)
            {
                reason = "not sleepy at all";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool IsOnCouch(Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            MapObject mapObj = actor.Location.Map.GetMapObjectAt(actor.Location.Position);
            if (mapObj == null)
                return false;

            return mapObj.IsCouch;
        }

        public bool IsActorDisturbed(Actor a)
        {
            return a.Model.Abilities.HasSanity && a.Sanity <= ActorDisturbedLevel(a);
        }

        public bool IsActorInsane(Actor a)
        {
            return a.Model.Abilities.HasSanity && a.Sanity <= 0;
        }

        public int SanityToHoursUntilUnstable(Actor a)
        {
            int left = a.Sanity - ActorDisturbedLevel(a);
            if (left <= 0) return 0;
            return left / WorldTime.TURNS_PER_HOUR;
        }
        #endregion

        #region Leading
        public bool CanActorTakeLead(Actor actor, Actor target)
        {
            string reason;
            return CanActorTakeLead(actor, target, out reason);
        }

        public bool CanActorTakeLead(Actor actor, Actor target, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");

            ///////////////////////////////////////
            // Can't if any is true:
            // 1. Target is undead or an enemy.
            // 2. Target is sleeping.
            // 3. Target has already a leader and can't steal the follower.  alpha10.1
            // 4. Target is a leader.
            // 5. Actor has reached max followers.
            // 6. Target is player!
            // 7. Faction restrictions.
            ///////////////////////////////////////

            // 1. Target is undead or an enemy.
            if (target.Model.Abilities.IsUndead)
            {
                reason = "undead";
                return false;
            }
            if (AreEnemies(actor, target))
            {
                reason = "enemy";
                return false;
            }

            // 2. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // 3. Target has already a leader and can't steal the follower.  alpha10.1
            if (target.HasLeader)
            {
                // alpha10.1
                // can "steal" followers only if actor has better charismatic skill than current leader
                int actorCharism = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
                int leaderCharism = target.Leader.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
                if (actorCharism <= leaderCharism)
                {
                    reason = "has already a leader at least as charismatic as you";
                    return false;
                }
            }

            // 4. Target is a leader.
            if (target.CountFollowers > 0)
            {
                reason = "is a leader";
                return false;
            }

            // 5. Actor has reached max followers.
            int maxFollowers = ActorMaxFollowers(actor);
            if (maxFollowers == 0)
            {
                reason = "can't lead";
                return false;
            }
            if (actor.CountFollowers >= maxFollowers)
            {
                reason = "too many followers";
                return false;
            }

            // 6. Target is player!
            if (target.IsPlayer)
            {
                reason = "is player";
                return false;
            }

            // 7. Faction restrictions.
            if (actor.Faction != target.Faction && target.Faction.LeadOnlyBySameFaction)
            {
                reason = String.Format("{0} can't lead {1}", actor.Faction.Name, target.Faction.Name);
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorCancelLead(Actor actor, Actor target, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////////
            // Can't if any is true:
            // 1. Target not a actor follower.
            // 2. Target is sleeping.
            //////////////////////////////////

            // 1. Target not a actor follower.
            if (target.Leader != actor)
            {
                reason = "not your follower";
                return false;
            }

            // 2. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorSwitchPlaceWith(Actor actor, Actor target)
        {
            string reason;
            return CanActorSwitchPlaceWith(actor, target, out reason);
        }

        public bool CanActorSwitchPlaceWith(Actor actor, Actor target, out string reason)       
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (target == null)
                throw new ArgumentNullException("target");

            //////////////////////////////////
            // Can't if any is true:
            // 1. Target not a actor follower.
            // 2. Target is sleeping.
            //////////////////////////////////

            // 1. Target not a actor follower.
            if (target.Leader != actor)
            {
                reason = "not your follower";
                return false;
            }

            // 2. Target is sleeping.
            if (target.IsSleeping)
            {
                reason = "sleeping";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }

        #endregion

        #region Trust
        public bool IsActorTrustingLeader(Actor actor)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            if (!actor.HasLeader)
                return false;

            return actor.TrustInLeader >= TRUST_TRUSTING_THRESHOLD;
        }

        public bool HasActorBondWith(Actor actor, Actor target)
        {
            if (actor.Leader == target)
                return actor.TrustInLeader >= TRUST_BOND_THRESHOLD;
            else if (target.Leader == actor)
                return target.TrustInLeader >= TRUST_BOND_THRESHOLD;
            else
                return false;
        }
        #endregion

        #region Building & Repairing
        public int CountBarricadingMaterial(Actor actor)
        {
            if (actor.Inventory == null || actor.Inventory.IsEmpty)
                return 0;

            int count = 0;
            foreach (Item it in actor.Inventory.Items)
                if (it is ItemBarricadeMaterial)
                    count += it.Quantity;
            return count;
        }

        public bool CanActorBuildFortification(Actor actor, Point pos, bool isLarge)
        {
            string reason;
            return CanActorBuildFortification(actor, pos, isLarge, out reason);
        }

        public bool CanActorBuildFortification(Actor actor, Point pos, bool isLarge, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            ///////////////////////////
            // Can't if any is true:
            // 1. No carpentry skill.
            // 2. Not walkable.
            // 3. Not engouh material.
            // 4. Tile occupied.
            ///////////////////////////

            // 1. No carpentry skill.
            if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) == 0)
            {
                reason = "no skill in carpentry";
                return false;
            }

            // 2. Not walkable.
            Map map = actor.Location.Map;
            if (!map.GetTileAt(pos).Model.IsWalkable)
            {
                reason = "cannot build on walls";
                return false;
            }

            // 3. Not enough material.
            int need = ActorBarricadingMaterialNeedForFortification(actor, isLarge);
            if (CountBarricadingMaterial(actor) < need)
            {
                reason = String.Format("not enough barricading material, need {0}.", need);
                return false;
            }

            // 4. Tile occupied.
            if (map.GetMapObjectAt(pos) != null || map.GetActorAt(pos) != null)
            {
                reason = "blocked";
                return false;
            }
                        
            // all clear.
            reason = "";
            return true;
        }

        public bool CanActorRepairFortification(Actor actor, Fortification fort, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");

            /////////////////////////////
            // Can't if any is true:
            // 1. Cannot use map objects.
            // 2. No material.
            /////////////////////////////

            // 1. Cannot use map objects.
            if (!actor.Model.Abilities.CanUseMapObjects)
            {
                reason = "cannot use map objects";
                return false;
            }

            // 2. No material.
            int material = CountBarricadingMaterial(actor);
            if (material <= 0)
            {
                reason = "no barricading material";
                return false;
            }

            // all clear.
            reason = "";
            return true;
        }
        #endregion

        #region Corpses
        public int ActorDamageVsCorpses(Actor a)
        {
            // base = melee HALVED.
            int dmg = a.CurrentMeleeAttack.DamageValue / 2;

            // Necrology.
            dmg += SKILL_NECROLOGY_CORPSE_BONUS * a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.NECROLOGY);

            return dmg;
        }

        public bool CanActorEatCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorEatCorpse(actor, corpse, out reason);
        }
        
        public bool CanActorEatCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ///////////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor is not undead or a starving/insane living.
            ///////////////////////////////////////////////

            // 1. Actor is not undead or a starving living.
            if (!actor.Model.Abilities.IsUndead)
            {
                if (!IsActorStarving(actor) && !IsActorInsane(actor))
                {
                    reason = "not starving or insane";
                    return false;
                }
            }

            // ok
            reason = "";
            return true;
        }

        public bool CanActorButcherCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorButcherCorpse(actor, corpse, out reason);
        }

        public bool CanActorButcherCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ////////////////////////////////////////
            // Can't if any is true:
            // 1. Actor tired.
            // 2. Corpse not in same tile as actor.
            ////////////////////////////////////////
            // 1. Actor tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }
            // 2. Corpse not in same tile as actor.
            if (corpse.Position != actor.Location.Position || !actor.Location.Map.HasCorpse(corpse))
            {
                reason = "not in same location";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public bool CanActorStartDragCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorStartDragCorpse(actor, corpse, out reason);
        }

        public bool CanActorStartDragCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ////////////////////////////////////////
            // Can't if any is true:
            // 1. Corpse already dragged.
            // 2. Actor tired.
            // 3. Corpse not in same tile as actor.
            // 4. Actor already dragging a corpse.
            ////////////////////////////////////////

            // 1. Corpse already dragged.
            if (corpse.IsDragged)
            {
                reason = "corpse is already being dragged";
                return false;
            }
            // 2. Actor tired.
            if (IsActorTired(actor))
            {
                reason = "tired";
                return false;
            }
            // 3. Corpse not in same tile as actor.
            if (corpse.Position != actor.Location.Position || !actor.Location.Map.HasCorpse(corpse))
            {
                reason = "not in same location";
                return false;
            }
            // 4. Actor already dragging a corpse.
            if (actor.DraggedCorpse != null)
            {
                reason = "already dragging a corpse";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public bool CanActorStopDragCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorStopDragCorpse(actor, corpse, out reason);
        }

        public bool CanActorStopDragCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");


            ////////////////////////////////////////
            // Can't if Corpse not being dragged by actor.
            ////////////////////////////////////////

            // Can't if Corpse not being dragged by actor.
            if (corpse.DraggedBy != actor)
            {
                reason = "not dragging this corpse";
                return false;
            }

            // ok
            reason = "";
            return true;
        }

        public bool CanActorReviveCorpse(Actor actor, Corpse corpse)
        {
            string reason;
            return CanActorReviveCorpse(actor, corpse, out reason);
        }

        public bool CanActorReviveCorpse(Actor actor, Corpse corpse, out string reason)
        {
            if (actor == null)
                throw new ArgumentNullException("actor");
            if (corpse == null)
                throw new ArgumentNullException("corpse");

            ///////////////////////////
            // Can't if 
            // 1. No medic skill.
            // 2. Not on same tile.
            // 3. Corpse not fresh.
            // 4. No medikit.
            //////////////////////////

            // 1. No medic skill.
            if (actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) == 0)
            {
                reason = "lack medic skill";
                return false;
            }
            
            // 2. Not on same tile.
            if (corpse.Position != actor.Location.Position)
            {
                reason = "not there";
                return false;
            }

            // 3. Corpse not fresh.
            if (CorpseRotLevel(corpse) > 0)
            {
                reason = "corpse not fresh";
                return false;
            }

            // 4. No medikit.
            if (!actor.Inventory.HasItemMatching((it) => it.Model.ID == (int)GameItems.IDs.MEDICINE_MEDIKIT))
            {
                reason = "no medikit";
                return false;
            }

            // ok
            reason = "";
            return true;
        }
        #endregion

        #endregion

        #region Distances

        public bool IsAdjacent(Location a, Location b)
        {
            if (a.Map != b.Map) return false;
            return IsAdjacent(a.Position, b.Position);
        }

        public bool IsAdjacent(Point pA, Point pB)
        {
            return Math.Abs(pA.X - pB.X) < 2 && Math.Abs(pA.Y - pB.Y) < 2;
        }

        public int GridDistance(Point pA, int bX, int bY)
        {
            return Math.Max(Math.Abs(pA.X - bX), Math.Abs(pA.Y - bY));
        }

        public int GridDistance(Point pA, Point pB)
        {
            return Math.Max(Math.Abs(pA.X - pB.X), Math.Abs(pA.Y - pB.Y));
        }

        /// <summary>
        /// Standard distance formula: square root of summed squares.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float StdDistance(Point from, Point to)
        {
            int dX = to.X - from.X;
            int dY = to.Y - from.Y;
            int square = dX * dX + dY * dY;
            return (float)Math.Sqrt(square);
        }

        public float StdDistance(Point v)
        {
            return (float)Math.Sqrt(v.X * v.X + v.Y * v.Y);
        }

        /// <summary>
        /// Distance to use in LOS computing. Based on standard distance and modified to have a nice circle FOV.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public float LOSDistance(Point from, Point to)
        {
            int dX = to.X - from.X;
            int dY = to.Y - from.Y;
            int square = dX * dX + dY * dY;
            return (float)Math.Sqrt(0.75f * square); // nice factor to have a smooth circle (0.90: too rough)
        }
        #endregion

        #region Actor turn ordering
        public Actor GetNextActorToAct(Map map, int turnCounter)
        {
            if (map == null)
                return null;

            int n = map.CountActors;
            for (int i = map.CheckNextActorIndex; i < n; i++)
            {
                Actor a = map.GetActor(i);
                if (a.ActionPoints > 0 && !a.IsSleeping)
                {
                    map.CheckNextActorIndex = i;
                    return a;
                }
            }

            return null;

#if false
            // old unoptimized algo : scan all actors.
            foreach (Actor actor in map.Actors)
            {                
                if (actor.ActionPoints > 0 && !actor.IsSleeping)
                    return actor;
            }

            return null;
#endif
        }

        public bool IsActorBeforeInMapList(Map map, Actor actor, Actor other)
        {
            foreach (Actor a in map.Actors)
            {
                if (a == actor)
                    return true;
                if (a == other)
                    return false;
            }

            // by default, assume yes.
            return true;
        }

        public bool CanActorActThisTurn(Actor actor)
        {
            if (actor == null)
                return false;

            return actor.ActionPoints > 0;
        }

        public bool CanActorActNextTurn(Actor actor)
        {
            if (actor == null)
                return false;

            return actor.ActionPoints + ActorSpeed(actor) > 0;
        }

        /// <summary>
        /// If actor spend a turn now, will actor get the chance to act before other?
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool WillActorActAgainBefore(Actor actor, Actor other)
        {
            // if other can still act, nope.
            if (other.ActionPoints > 0)
                return false;

            // if other will be able to act next turn BEFORE actor, nope.
            if (other.ActionPoints + ActorSpeed(other) > 0 && IsActorBeforeInMapList(actor.Location.Map, other, actor))
                return false;

            // safe!
            return true;
        }

        public bool WillOtherActTwiceBefore(Actor actor, Actor other)
        {
            if (IsActorBeforeInMapList(actor.Location.Map, actor, other))
            {
                if (other.ActionPoints > BASE_ACTION_COST)
                    return true;
                else
                    return false;
            }
            else
            {
                if (other.ActionPoints + ActorSpeed(other) > BASE_ACTION_COST)
                    return true;
                else
                    return false;
            }
        }
        #endregion

        #region Actors relations
        // alpha10 refactored and rewrote
        /// <summary>
        /// Check if both actors are enemies.
        /// - enemy factions
        /// - enemy gangs
        /// - personal enemies
        /// - group enemies (if checkGroups)
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="actorB"></param>
        /// <param name="checkGroups"></param>
        /// <returns></returns>
        public bool AreEnemies(Actor actorA, Actor actorB, bool checkGroups=true)
        {
            if (actorA == null || actorB == null)
                return false;
            if (actorA == actorB)  // alpha10 silly fix
                return false;

            // Enemy factions? (symetrical)
            if (actorA.Faction.IsEnemyOf(actorB.Faction))
                return true;

            // Enemy gangs? (symetrical)
            if (actorA.Faction == actorB.Faction && actorA.IsInAGang && actorB.IsInAGang && actorA.GangID != actorB.GangID)
                    return true;

            // alpha10 
            // Personal enemies? (symetrical)
            if (ArePersonalEnemies(actorA, actorB))
                return true;

            // alpha10
            // Enemy of groups (symetrical)
            if (checkGroups && AreGroupEnemies(actorA, actorB))
                return true;

            // Not enemies.
            return false;
        }

        // alpha10
        /// <summary>
        /// Check if they are in an agressor-selfdefence reliation.
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="actorA"></param>
        /// <param name="actorB"></param>
        /// <returns></returns>
        public bool ArePersonalEnemies(Actor actorA, Actor actorB)
        {
            if (actorA == null || actorB == null)
                return false;
            if (actorA == actorB)
                return false;

            if (actorA.IsAggressorOf(actorB))
                return true;

            if (actorA.IsSelfDefenceFrom(actorB))
                return true;

            // doesnt need to check for target as the relation is symetrical (aggressor of <-> self defence from)
            return false;
        }

        // alpha10
        /// <summary>
        /// Check if they are enmemies through group relations : 
        /// - my leader enemies are my enemies
        /// - my mates enemies are my enemies.
        /// - my follower enemies are my enemies.
        /// Symetrical, don't need to call for actorB,actorA.
        /// </summary>
        /// <param name="actorA"></param>
        /// <param name="actorB"></param>
        /// <returns></returns>
        public bool AreGroupEnemies(Actor actorA, Actor actorB)
        {
            if (actorA == null || actorB == null)
                return false;
            if (actorA == actorB)
                return false;

            // my leader enemies are my enemies.
            // my mates enemies are my enemies.
            bool IsEnemyOfMyLeaderOrMates(Actor groupActor, Actor target)
            {
                if (AreEnemies(groupActor.Leader, target, false))
                    return true;
                foreach (Actor mate in groupActor.Leader.Followers)
                    if (mate != groupActor && AreEnemies(mate, target, false))
                        return true;
                return false;
            }

            // my followers enemies are my enemies
            bool IsEnemyOfMyFollowers(Actor groupActor, Actor target)
            {
                foreach (Actor follower in groupActor.Followers)
                    if (AreEnemies(follower, target, false))
                        return true;
                return false;
            }

            // check A group
            if (actorA.HasLeader)
                if (IsEnemyOfMyLeaderOrMates(actorA, actorB))
                    return true;
            if (actorA.CountFollowers > 0)
                if (IsEnemyOfMyFollowers(actorA, actorB))
                    return true;

            // check B group
            if (actorB.HasLeader)
                if (IsEnemyOfMyLeaderOrMates(actorB, actorA))
                    return true;
            if (actorB.CountFollowers > 0)
                if (IsEnemyOfMyFollowers(actorB, actorA))
                    return true;

            // nope
            return false;
        }

        public bool IsMurder(Actor killer, Actor victim)
        {
            if (killer == null || victim == null)
                return false;

            // killing an undead is never a murder (doh!)
            if (victim.Model.Abilities.IsUndead)
                return false;

            // a law enforcer killing a murderer is not a murder.
            if (killer.Model.Abilities.IsLawEnforcer && victim.MurdersCounter > 0)
                return false;

            // killing between enemy factions is allowed.
            if (killer.Faction.IsEnemyOf(victim.Faction))
                return false;

            // killing in self defence is not a murder.
            if (killer.IsSelfDefenceFrom(victim))
                return false;

            // all other cases are murders!
            return true;
        }
        #endregion

        #region Stats affected by Skills & Status effects
        public int ActorSpeed(Actor actor)
        {
            float speed = actor.Doll.Body.Speed;

            // stamina.
            if (IsActorTired(actor))
                speed *= 2f / 3f;

            // sleep.
            if (IsActorExhausted(actor))
                speed /= 2f;
            else if (IsActorSleepy(actor))
                speed *= 2f / 3f;

            // wearing armor.
            ItemBodyArmor armor = actor.GetEquippedItem(DollPart.TORSO) as ItemBodyArmor;
            if (armor != null)
                speed -= armor.Weight;

            // dragging corpses.
            if (actor.DraggedCorpse != null)
                speed /= 2f;

            // done, speed must be >= 0.
            return Math.Max((int)speed, 0);
        }

        public int ActorMaxHPs(Actor actor)
        {
            int skillBonus = (SKILL_TOUGH_HP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.TOUGH))
                + (SKILL_ZTOUGH_HP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_TOUGH));

            return actor.Sheet.BaseHitPoints + skillBonus;
        }

        public int ActorMaxSTA(Actor actor)
        {
            int skillBonus = (SKILL_HIGH_STAMINA_STA_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HIGH_STAMINA));

            return actor.Sheet.BaseStaminaPoints + skillBonus;
        }

        public int ActorItemNutritionValue(Actor actor, int baseValue)
        {
            int skillBonus = (int)(baseValue * SKILL_LIGHT_EATER_FOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER));

            return baseValue + skillBonus;
        }

        public int ActorMaxFood(Actor actor)
        {
            int skillBonus = (int)(actor.Sheet.BaseFoodPoints * SKILL_LIGHT_EATER_MAXFOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER));

            return actor.Sheet.BaseFoodPoints + skillBonus;
        }

        public int ActorMaxRot(Actor actor)
        {
            int skillBonus = (int)(actor.Sheet.BaseFoodPoints * SKILL_ZLIGHT_EATER_MAXFOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_EATER));

            return actor.Sheet.BaseFoodPoints + skillBonus;
        }

        public int ActorMaxSleep(Actor actor)
        {
            int skillBonus = (int)(actor.Sheet.BaseSleepPoints * SKILL_AWAKE_SLEEP_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AWAKE));

            return (actor.Sheet.BaseSleepPoints + skillBonus);
        }

        public int ActorSleepRegen(Actor actor, bool isOnCouch)
        {
            int baseRegen = isOnCouch ? SLEEP_COUCH_SLEEPING_REGEN : SLEEP_NOCOUCH_SLEEPING_REGEN;
            int skillBonus = (int)(baseRegen * SKILL_AWAKE_SLEEP_REGEN_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AWAKE));

            return baseRegen + skillBonus;
        }

        public int ActorMaxSanity(Actor actor)
        {
            return actor.Sheet.BaseSanity;
        }

        public int ActorDisturbedLevel(Actor actor)
        {
            float factor = 1.0f - SKILL_STRONG_PSYCHE_LEVEL_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG_PSYCHE);
            return (int)(SANITY_UNSTABLE_LEVEL * factor);
        }

        public int ActorMaxInv(Actor actor)
        {
            int skillBonus = SKILL_HAULER_INV_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HAULER);

            return actor.Sheet.BaseInventoryCapacity + skillBonus;
        }

        public int ActorDamageBonusVsUndeads(Actor actor)
        {
            return SKILL_NECROLOGY_UNDEAD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.NECROLOGY);
        }

        // alpha10 added mapobject param
        public Attack ActorMeleeAttack(Actor actor, Attack baseAttack, Actor target, MapObject objToBreak=null)
        {
            float hit = baseAttack.HitValue;
            float dmg = baseAttack.DamageValue;
            int disarmBonus = 0;  // alpha10

            // skills bonuses.
            int hitBonus = SKILL_AGILE_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) +
                            SKILL_ZAGILE_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE);
            int dmgBonus = SKILL_STRONG_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG) +
                            SKILL_ZSTRONG_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_STRONG);
            // martial arts apply only if no weapon equipped.
            if (actor.GetEquippedWeapon() == null)
            {
                int ma = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MARTIAL_ARTS);
                if (ma > 0)
                {
                    hitBonus += SKILL_MARTIAL_ARTS_ATK_BONUS * ma;
                    dmgBonus += SKILL_MARTIAL_ARTS_DMG_BONUS * ma;
                    disarmBonus += SKILL_MARTIAL_ARTS_DISARM_BONUS * ma;
                }
            }

            // necrology vs undeads.
            if (target != null && target.Model.Abilities.IsUndead)
                dmgBonus += ActorDamageBonusVsUndeads(actor);

            // alpha10
            // add tool damage bonus vs map objects
            if (objToBreak != null)
            {
                ItemMeleeWeapon eqMw = actor.GetEquippedMeleeWeapon();
                if (eqMw != null)
                {
                    dmgBonus += eqMw.ToolBashDamageBonus;
                }
            }

            hit += hitBonus;
            dmg += dmgBonus;

            // alpha10
            // disarm chance.
            float disarmChance = MELEE_DISARM_BASE_CHANCE;
            disarmChance += disarmBonus;
            // defender strong resist disarm
            if (target != null)
                disarmChance -= SKILL_STRONG_RESIST_DISARM_BONUS * target.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG);

            // sleepiness penalties.
            if (IsActorExhausted(actor))
            {
                hit /= 2f;
                disarmChance /= 2f;
            }
            else if (IsActorSleepy(actor))
            {
                hit *= 3f / 4f;
                disarmChance *= 3f / 4f;
            }

            // done.
            return Attack.MeleeAttack(baseAttack.Verb, (int)hit, (int)dmg, baseAttack.StaminaPenalty, (int)disarmChance);
        }

        public Attack ActorRangedAttack(Actor actor, Attack baseAttack, int distance, Actor target)
        {
            int hitMod = 0;
            int dmgBonus = 0;

            // skill bonuses.
            switch (baseAttack.Kind)
            {
                case AttackKind.BOW:
                    hitMod = SKILL_BOWS_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.BOWS);
                    dmgBonus = SKILL_BOWS_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.BOWS);
                    break;
                case AttackKind.FIREARM:
                    hitMod = SKILL_FIREARMS_ATK_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.FIREARMS);
                    dmgBonus = SKILL_FIREARMS_DMG_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.FIREARMS);
                    break;
            }

            if (target != null && target.Model.Abilities.IsUndead)
                dmgBonus += ActorDamageBonusVsUndeads(actor);

            // distance vs range penalties/bonus.
            int efficientRange = baseAttack.EfficientRange;
            // alpha10 distance as % modifier instead of flat bonus
            float distanceMod = 1;
            if (distance != efficientRange)
            {
                float distanceScale = (efficientRange - distance) / (float)baseAttack.Range;
                // bigger effect (penalty) beyond efficient range
                if (distance > efficientRange)
                    distanceScale *= 2;

                distanceMod = 1 + distanceScale;
            }
            float hit = (baseAttack.HitValue + hitMod) * distanceMod;
            float rapidHit1 = (baseAttack.Hit2Value + hitMod) * distanceMod;
            float rapidHit2 = (baseAttack.Hit3Value + hitMod) * distanceMod;

            float dmg = baseAttack.DamageValue + dmgBonus;

            // sleep penalty.
            if (IsActorExhausted(actor))
            {
                hit *= FIRING_WHEN_SLP_EXHAUSTED;
                rapidHit1 *= FIRING_WHEN_SLP_EXHAUSTED;
                rapidHit2 *= FIRING_WHEN_SLP_EXHAUSTED; 
            }
            else if (IsActorSleepy(actor))
            {
                hit *= FIRING_WHEN_SLP_SLEEPY;
                rapidHit1 *= FIRING_WHEN_SLP_SLEEPY;
                rapidHit2 *= FIRING_WHEN_SLP_SLEEPY;
            }

            // stamina penalty.
            if (IsActorTired(actor))
            {
                hit *= FIRING_WHEN_STA_TIRED;
                rapidHit1 *= FIRING_WHEN_STA_TIRED;
                rapidHit2 *= FIRING_WHEN_STA_TIRED;
            }
            else if (actor.StaminaPoints < ActorMaxSTA(actor))
            {
                hit *= FIRING_WHEN_STA_NOT_FULL;
                rapidHit1 *= FIRING_WHEN_STA_NOT_FULL;
                rapidHit2 *= FIRING_WHEN_STA_NOT_FULL;
            }

            // return attack.
            return Attack.RangedAttack(baseAttack.Kind, baseAttack.Verb, (int)hit, (int)rapidHit1, (int)rapidHit2, (int)dmg, baseAttack.Range);
        }

        // alpha10
        /// <summary>
        /// Estimate chances to hit with a ranged attack. <br></br>
        /// Simulate a large number of rolls attack vs defence and returns % of hits.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="target"></param>
        /// <param name="shotCounter">0 for normal shot, 1 for 1st rapid fire shot, 2 for 2nd rapid fire shot</param>
        /// <returns>[0..100]</returns>
        public int ComputeChancesRangedHit(Actor actor, Actor target, int shotCounter)
        {
            Attack attack = ActorRangedAttack(actor, actor.CurrentRangedAttack, GridDistance(actor.Location.Position, target.Location.Position), target);
            Defence defence = ActorDefence(target, target.CurrentDefence);

            int hitValue = (shotCounter == 0 ? attack.HitValue : shotCounter == 1 ? attack.Hit2Value : attack.Hit3Value);
            int defValue = defence.Value;

            const int ROLLS = 1000;
            int hits = 0;
            for (int i = 0; i < ROLLS; i++)
            {
                int atkRoll = RollSkill(hitValue);
                int defRoll = RollSkill(defValue);
                if (atkRoll > defRoll)
                    hits++;
            }

            int percent = (100 * hits) / ROLLS;
            return percent;
        }

        public int ActorMaxThrowRange(Actor actor, int baseRange)
        {
            int bonus = SKILL_STRONG_THROW_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.STRONG);

            return baseRange + bonus;
        }

        public Defence ActorDefence(Actor actor, Defence baseDefence)
        {
            // Sleeping actors are defenceless.
            if (actor.IsSleeping)
                return new Defence(0, 0, 0);

            // Base value + skill.
            int defBonus = SKILL_AGILE_DEF_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.AGILE) +
                SKILL_ZAGILE_DEF_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_AGILE);
            float def = baseDefence.Value + defBonus;

            // Sleepy effect.
            if (IsActorExhausted(actor))
                def /= 2f;
            else if (IsActorSleepy(actor))
                def *= 3f / 4f;

            // done.
            return new Defence((int)def, baseDefence.Protection_Hit, baseDefence.Protection_Shot);
        }

        public int ActorMedicineEffect(Actor actor, int baseEffect)
        {
            int effectBonus = (int)(Math.Ceiling(SKILL_MEDIC_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) * baseEffect));
            return baseEffect + effectBonus;
        }

        public int ActorHealChanceBonus(Actor actor)
        {
            int chanceBonus = SKILL_HARDY_HEAL_CHANCE_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.HARDY);
            return chanceBonus;
        }

        public int ActorBarricadingPoints(Actor actor, int baseBarricadingPoints)
        {
            int barBonus = 0;
            
            // carpentry skill
            barBonus += (int)(baseBarricadingPoints * SKILL_CARPENTRY_BARRICADING_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY));

            // alpha10
            // tool build bonus
            ItemMeleeWeapon eqMw = actor.GetEquippedMeleeWeapon();
            if (eqMw != null && eqMw.ToolBuildBonus != 0)
            {
                barBonus += (int)(baseBarricadingPoints * eqMw.ToolBuildBonus);
            }

            return baseBarricadingPoints + barBonus;
        }

        public int ActorMaxFollowers(Actor actor)
        {
            return SKILL_LEADERSHIP_FOLLOWER_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LEADERSHIP);
        }

        public int ActorFOV(Actor actor, WorldTime time, Weather weather)
        {
            // Sleeping actors have no FOV.
            if (actor.IsSleeping)
                return 0;

            // base value.
            int FOV = actor.Sheet.BaseViewRange;

            // lighting/weather
            Lighting light = actor.Location.Map.Lighting;
            switch (light)
            {
                case Lighting.DARKNESS:
                    // FoV in darkness depends on actor.
                    FOV = DarknessFov(actor);
                    break;
                case Lighting.LIT:
                    // nothing to do, unmodified base FOV.
                    break;
                case Lighting.OUTSIDE:
                    // night & weather penalty
                    FOV -= NightFovPenalty(actor, time);
                    FOV -= WeatherFovPenalty(actor, weather);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("unhandled lighting");
            }

            // sleep penalty.
            if (IsActorExhausted(actor))
                FOV -= 2;
            else if (IsActorSleepy(actor))
                FOV -= 1;

            // light equipped or standing next to someone with light.
            // works only in darkness or during the night.
            if (light == Lighting.DARKNESS || (light == Lighting.OUTSIDE && time.IsNight))
            {
                int lightBonus = 0;

                lightBonus = GetLightBonusEquipped(actor);
                if (lightBonus == 0)
                {
                    Map map = actor.Location.Map;
                    if (map.HasAnyAdjacentInMap(actor.Location.Position,
                        (pt) =>
                        {
                            Actor other = map.GetActorAt(pt);
                            if (other == null)
                                return false;
                            return HasLightOnEquipped(other);
                        }))
                        lightBonus = 1;
                }
                FOV += lightBonus;
            }

            // standing on some map objects.
            MapObject mobj = actor.Location.Map.GetMapObjectAt(actor.Location.Position);
            if (mobj != null && mobj.StandOnFovBonus)
                ++FOV;

            // done.
            FOV = Math.Max(MINIMAL_FOV, FOV);
            return FOV;
        }

        public float ActorSmell(Actor actor)
        {
            return (1.0f + SKILL_ZTRACKER_SMELL_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_TRACKER)) * actor.Model.StartingSheet.BaseSmellRating;
        }

        public int ActorSmellThreshold(Actor actor)
        {
            // sleeping actors can't smell.
            if (actor.IsSleeping)
                return -1;

            // actor model base value.
            float smellRating = ActorSmell(actor);
            int minSmell = 1 + OdorScent.MAX_STRENGTH - (int)(smellRating * OdorScent.MAX_STRENGTH);
            return minSmell;
        }

        bool HasLightOnEquipped(Actor actor)
        {
            ItemLight light = actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemLight;
            return (light != null && light.Batteries > 0);
        }

        int GetLightBonusEquipped(Actor actor)
        {
            ItemLight light = actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemLight;
            return light == null || light.Batteries <= 0 ? 0 : light.FovBonus;
        }

        public int ActorLoudNoiseWakeupChance(Actor actor, int noiseDistance)
        {
            int baseChance = LOUD_NOISE_BASE_WAKEUP_CHANCE;
            int skillBonus = SKILL_LIGHT_SLEEPER_WAKEUP_CHANCE_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_SLEEPER);
            int distBonus = Math.Max(0, (LOUD_NOISE_RADIUS - noiseDistance) * LOUD_NOISE_DISTANCE_BONUS);

            return baseChance + skillBonus + distBonus;
        }

        public int ActorBarricadingMaterialNeedForFortification(Actor builder, bool isLarge)
        {
            int baseCost = isLarge ? 4 : 2;
            int skillBonus = (builder.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CARPENTRY) >= 3 ? SKILL_CARPENTRY_LEVEL3_BUILD_BONUS : 0);
            return Math.Max(1, baseCost - skillBonus);
        }

        public int ActorTrustIncrease(Actor actor)
        {
            int skillBonus = SKILL_CHARISMATIC_TRUST_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);

            return TRUST_BASE_INCREASE + skillBonus;
        }

        public int ActorCharismaticTradeChance(Actor actor)
        {
            return SKILL_CHARISMATIC_TRADE_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC);
        }

        public int ActorUnsuspicousChance(Actor observer, Actor actor)
        {
            // base = unsuspicious skill.
            int baseChance = SKILL_UNSUSPICIOUS_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.UNSUSPICIOUS);

            // bonus.
            int bonus = 0;

            // wearing some outfit.
            ItemBodyArmor armor = actor.GetEquippedItem(DollPart.TORSO) as ItemBodyArmor;
            if (armor != null)
            {
                if (observer.Faction.ID == (int)GameFactions.IDs.ThePolice)
                {
                    if (armor.IsHostileForCops())
                        bonus -= UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
                    else if (armor.IsFriendlyForCops())
                        bonus += UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
                }
                else if (observer.Faction.ID == (int)GameFactions.IDs.TheBikers)
                {
                    if (armor.IsHostileForBiker((GameGangs.IDs)observer.GangID))
                        bonus -= UNSUSPICIOUS_BAD_OUTFIT_PENALTY;
                    else if (armor.IsFriendlyForBiker((GameGangs.IDs)observer.GangID))
                        bonus += UNSUSPICIOUS_GOOD_OUTFIT_BONUS;
                }
            }

            return baseChance + bonus;
        }

        public int ActorSpotMurdererChance(Actor spotter, Actor murderer)
        {
            int spotterBonus = MURDER_SPOTTING_MURDERCOUNTER_BONUS * murderer.MurdersCounter;
            int distancePenalty = MURDERER_SPOTTING_DISTANCE_PENALTY * GridDistance(spotter.Location.Position, murderer.Location.Position);

            return MURDERER_SPOTTING_BASE_CHANCE + spotterBonus - distancePenalty;
        }

#if false
        // alpha10  previous attempt
        // FIXME -- needing to pass game as arg is uggly. shouldnt be any reference to game in rules but we need to
        // call an ai method...
        public int ScoreNpcItemTradeValue(RogueGame game, Actor npc, Item it, bool fromNpcInventory, Actor otherTrader)
        {
            // base score
            int score = (npc.Controller as BaseAI).ScoreItemValue(game, it, fromNpcInventory);

            // modify by respective charismatic skills
            // a charismatic actor will make the opposing trader over-estimate the actor items.
            int addScore = 0;
            if (fromNpcInventory)
                addScore = (score * npc.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC) * SKILL_CHARISMATIC_TRADE_BONUS) / 100;
            else
                addScore = (score * otherTrader.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.CHARISMATIC) * SKILL_CHARISMATIC_TRADE_BONUS) / 100;
    
            score += addScore;

            return score;
        }
#endif 
        #endregion

        #region Day/Night, Weather & Lighting effects
        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="time"></param>
        /// <returns>penalty as number > 0</returns>
        public int NightFovPenalty(Actor actor, WorldTime time)
        {
            if (actor.Model.Abilities.IsUndead)
                return 0;
            else
            {
                switch (time.Phase)
                {
                    case DayPhase.SUNSET: return FOV_PENALTY_SUNSET;
                    case DayPhase.EVENING: return FOV_PENALTY_EVENING;
                    case DayPhase.MIDNIGHT: return FOV_PENALTY_MIDNIGHT;
                    case DayPhase.DEEP_NIGHT: return FOV_PENALTY_DEEP_NIGHT;
                    case DayPhase.SUNRISE: return FOV_PENALTY_SUNRISE;
                    default: return 0;
                }
            }
        }

        public int NightStaminaPenalty(Actor actor)
        {
            if (actor.Model.Abilities.IsUndead)
                return 0;
            else
                return NIGHT_STA_PENALTY;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="weather"></param>
        /// <returns>penalty as number > 0</returns>
        public int WeatherFovPenalty(Actor actor, Weather weather)
        {
            if (actor.Model.Abilities.IsUndead)
                return 0;
            else
            {
                switch (weather)
                {
                    case Weather.RAIN: return FOV_PENALTY_RAIN;
                    case Weather.HEAVY_RAIN: return FOV_PENALTY_HEAVY_RAIN;
                    default: return 0;
                }
            }
        }

        public bool IsWeatherRain(Weather weather)
        {
            switch (weather)
            {
                case Weather.CLEAR:
                case Weather.CLOUDY:
                    return false;
                case Weather.HEAVY_RAIN:
                case Weather.RAIN:
                    return true;
                default: throw new ArgumentOutOfRangeException("unhandled weather");
            }
        }

        public int DarknessFov(Actor actor)
        {
            if (actor.Model.Abilities.IsUndead)
                return actor.Sheet.BaseViewRange;
            else
                return MINIMAL_FOV;
        }       
        
        // alpha10
        public int OdorsDecay(Map map, Point pos, Weather weather)
        {
            int decay;

            // base decay
            decay = 1;

            // sewers?
            if (map == map.District.SewersMap)
            {
                decay += 2;
            }
            // outside? = weather affected.
            else if (!map.GetTileAt(pos).IsInside)  // alpha10 weather affect only outside tiles
            {
                switch (weather)
                {
                    case Weather.CLEAR:
                    case Weather.CLOUDY:
                        // default decay.
                        break;
                    case Weather.RAIN:
                        decay += 1;
                        break;
                    case Weather.HEAVY_RAIN:
                        decay += 2;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("unhandled weather");
                }
            }

            return decay;
        }
        
        // alpha10
        public bool CanActorSeeSky(Actor actor)
        {
            if (actor.IsDead)
                return false;
            if (actor.IsSleeping)
                return false;
            return actor.Location.Map.Lighting == Lighting.OUTSIDE;
        }

        public bool CanActorKnowTime(Actor actor)
        {
            if (actor.IsDead)
                return false;
            if (actor.IsSleeping)
                return false;
            if (actor.Location.Map.Lighting == Lighting.OUTSIDE)
                return true;

            ItemTracker eqTracker = actor.GetEquippedItem(DollPart.LEFT_HAND) as ItemTracker;
            if (eqTracker != null && eqTracker.HasClock && eqTracker.Batteries > 0)
                return true;

            return false;
        }
        #endregion

        #region Map Power rating
        /// <summary>
        /// Compute normalized % of power level on the map. Each activated PowerGenerator map object count for one power unit.
        /// </summary>
        /// <param name="map"></param>
        /// <returns>0.0 = not powered at all to 1.0 = 100% power.</returns>
        public float ComputeMapPowerRatio(Map map)
        {
            int totalOn, totalOff;

            totalOff = totalOn = 0;

            foreach (MapObject obj in map.MapObjects)
            {
                PowerGenerator powGen = obj as PowerGenerator;
                if (powGen == null)
                    continue;

                if (powGen.IsOn)
                    ++totalOn;
                else
                    ++totalOff;
            }

            int totalCount = totalOn + totalOff;
            if (totalCount == 0)
                return 0.0f;

            float ratio = (float)totalOn / (float)(totalCount);
            return ratio;
        }
        #endregion

        #region Explosions
        public int BlastDamage(int distance, BlastAttack attack)
        {
            if (distance < 0 || distance > attack.Radius)
                throw new ArgumentOutOfRangeException(String.Format("blast distance {0} out of range", distance));

            return attack.Damage[distance];
        }
        #endregion

        #region Undead Regen/Food, Infection and Corpses
        public int ActorBiteHpRegen(Actor a, int dmg)
        {
            int bonus = (int)(Rules.SKILL_ZEATER_REGEN_BONUS * a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_EATER) * dmg);
            return dmg + bonus;
        }

        public int ActorBiteNutritionValue(Actor actor, int baseValue)
        {
            float zskillFactor = SKILL_ZLIGHT_EATER_FOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_EATER);
            float skillFactor = SKILL_LIGHT_EATER_FOOD_BONUS * actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_EATER);

            return (int)(CORPSE_EATING_NUTRITION_FACTOR + zskillFactor + skillFactor) * baseValue;
        }

        public int CorpseEeatingInfectionTransmission(int infection)
        {
            return (int)(CORPSE_EATING_INFECTION_FACTOR * infection);
        }

        public int ActorInfectionHPs(Actor a)
        {
            return ActorMaxHPs(a) + ActorMaxSTA(a);
        }

        public static int InfectionForDamage(Actor infector, int dmg)
        {
            float factor = INFECTION_BASE_FACTOR + infector.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_INFECTOR) * SKILL_ZINFECTOR_BONUS;
            return (int)(factor * dmg);
        }

        public int ActorInfectionPercent(Actor a)
        {
            return (int)(100 * a.Infection / ActorInfectionHPs(a));
        }

        public int InfectionEffectTriggerChance1000(int infectionPercent)
        {
            return INFECTION_EFFECT_TRIGGER_CHANCE_1000 + infectionPercent / 5;
        }

        public int CorpseFreshnessPercent(Corpse c)
        {
            return (int)(100 * c.HitPoints / ActorMaxHPs(c.DeadGuy));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <returns>[0..5]</returns>
        public int CorpseRotLevel(Corpse c)
        {
            int freshP = CorpseFreshnessPercent(c);
            if (freshP < 5)
                return 5;
            if (freshP < 25)
                return 4;
            if (freshP < 50)
                return 3;
            if (freshP < 75)
                return 2;
            if (freshP < 90)
                return 1;
            return 0;
        }

        public static float CorpseDecayPerTurn(Corpse c)
        {
            return CORPSE_DECAY_PER_TURN;
        }

        public int CorpseZombifyChance(Corpse c, WorldTime timeNow, bool checkDelay = true)
        {
            float chance;

            // delay zombification.
            int dT = timeNow.TurnCounter - c.Turn;
            if (checkDelay && dT < CORPSE_ZOMBIFY_DELAY)
                return 0;

            // compute infection P
            int infP = ActorInfectionPercent(c.DeadGuy);

            // check only every X turns, higher frequency as infP rise.
            if (checkDelay)
            {
                int freq = (infP >= 100 ? 1 : 100 / (1 + infP));
                if (timeNow.TurnCounter % freq != 0)
                    return 0;
            }

            // base chance.
            chance = CORPSE_ZOMBIFY_BASE_CHANCE;

            // living infection
            chance += CORPSE_ZOMBIFY_INFECTIONP_FACTOR * infP;

            // less likey as time passes.
            chance -= (int)(CORPSE_ZOMBIFY_TIME_FACTOR * dT);

            // factor day & night.
            if (timeNow.IsNight)
                chance *= CORPSE_ZOMBIFY_NIGHT_FACTOR;
            else
                chance *= CORPSE_ZOMBIFY_DAY_FACTOR;

            // ok.
            int intChance = Math.Max(0, Math.Min(100, (int)chance));
            return intChance;
        }

        public int CorpseReviveChance(Actor actor, Corpse corpse)
        {
            if (!CanActorReviveCorpse(actor, corpse))
                return 0;
            int baseChance = CorpseFreshnessPercent(corpse) / 4;
            int skillBonus = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC) * SKILL_MEDIC_REVIVE_BONUS;
            return baseChance + skillBonus;
        }

        public int CorpseReviveHPs(Actor actor, Corpse corpse)
        {
            int baseHps = 5;
            int skillBonus = actor.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.MEDIC);
            return baseHps + skillBonus;
        }
        #endregion

        #region Traps
        // alpha10
        public int GetTrapTriggerChance(ItemTrap trap, Actor a)
        {
            // alpha10.1 bugfix - correctly has 0 chance to trigger safe traps (eg: followers traps etc...)
            if (IsSafeFromTrap(trap, a))
                return 0;

            int baseChance;
            int avoidBonus;

            baseChance = trap.TrapModel.TriggerChance * trap.Quantity;

            avoidBonus = 0;
            if (a.Model.Abilities.IsUndead)
                avoidBonus -= TRAP_UNDEAD_ACTOR_TRIGGER_PENALTY;
            if (a.Model.Abilities.IsSmall)
                avoidBonus += TRAP_SMALL_ACTOR_AVOID_BONUS;
            avoidBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_FEET) * SKILL_LIGHT_FEET_TRAP_BONUS;
            avoidBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_FEET) * SKILL_ZLIGHT_FEET_TRAP_BONUS;

            return baseChance - avoidBonus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="a"></param>
        /// <returns>true if trap triggers</returns>
        public bool CheckTrapTriggers(ItemTrap trap, Actor a)
        {
            // alpha10 extracted and modified trigger chance formula
            int chance = GetTrapTriggerChance(trap, a);
            return chance > 0 ? RollChance(chance) : false;
        }

        public bool CheckTrapTriggers(ItemTrap trap, MapObject mobj)
        {
            return RollChance(trap.TrapModel.TriggerChance * mobj.Weight);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trap"></param>
        /// <param name="mobj">can be null</param>
        /// <returns></returns>
        public bool CheckTrapStepOnBreaks(ItemTrap trap, MapObject mobj = null)
        {
            int chance = trap.TrapModel.BreakChance;
            if (mobj != null) chance *= mobj.Weight;
            return RollChance(chance);
        }

        public bool CheckTrapEscapeBreaks(ItemTrap trap, Actor a)
        {
            return RollChance(trap.TrapModel.BreakChanceWhenEscape);
        }

        // alpha10
        public bool IsSafeFromTrap(ItemTrap trap, Actor a)
        {
            if (trap.Owner == null)
                return false;
            if (trap.Owner == a)
                return true;
            return a.IsInGroupWith(trap.Owner);
        }

        public bool CheckTrapEscape(ItemTrap trap, Actor a)
        {
            // alpha10
            if (IsSafeFromTrap(trap, a))
                return true;

            int escapeBonus = 0;

            escapeBonus += a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.LIGHT_FEET) * SKILL_LIGHT_FEET_TRAP_BONUS
                + a.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_LIGHT_FEET) * SKILL_ZLIGHT_FEET_TRAP_BONUS;

            return RollChance(escapeBonus + (100 - trap.TrapModel.BlockChance * trap.Quantity));
        }

        public bool IsTrapCoveringMapObjectThere(Map map, Point pos)
        {
            MapObject mobj = map.GetMapObjectAt(pos);
            if (mobj == null) return false;
            // mobj is either walkable and not a door (eg:bed) or jumpable (eg:table,car...)
            return mobj.IsJumpable || (mobj.IsWalkable && !(mobj is DoorWindow));
        }

        public bool IsTrapTriggeringMapObjectThere(Map map, Point pos)
        {
            MapObject mobj = map.GetMapObjectAt(pos);
            if (mobj == null) return false;
            // mobj is NOT walkable and a door and NOT jumpable (eg:shelves,large fort)
            return !mobj.IsWalkable && !mobj.IsJumpable && !(mobj is DoorWindow);
        }
        #endregion

        #region Grabbing
        public int ZGrabChance(Actor grabber, Actor victim)
        {
            int zGrabLevel = grabber.Sheet.SkillTable.GetSkillLevel((int)Skills.IDs.Z_GRAB);
            return zGrabLevel * SKILL_ZGRAB_CHANCE;
        }
        #endregion

        #region Game modes
        public static bool HasImmediateZombification(GameMode mode)
        {
            return mode == GameMode.GM_STANDARD;
        }

        public static bool HasInfection(GameMode mode)
        {
            return mode != GameMode.GM_STANDARD;
        }

        public static bool HasCorpses(GameMode mode)
        {
            return mode != GameMode.GM_STANDARD;
        }

        public static bool HasEvolution(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasAllZombies(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasZombiesInBasements(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }

        public static bool HasZombiesInSewers(GameMode mode)
        {
            return mode != GameMode.GM_VINTAGE;
        }
        #endregion
    }
}
