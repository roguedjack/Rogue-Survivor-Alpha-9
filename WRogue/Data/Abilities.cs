using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Abilities
    {
        #region Blank abilities
        [NonSerialized]
        public static readonly Abilities NONE = new Abilities();
        #endregion

        #region Properties
        /// <summary>
        /// Is an undead.
        /// </summary>
        public bool IsUndead { get; set; }

        /// <summary>
        /// Is master of undeads.
        /// </summary>
        public bool IsUndeadMaster { get; set; }

        /// <summary>
        /// Killing living actors will turn them into zombies; actor also regen its HP from damage inflicted on livings.
        /// </summary>
        public bool CanZombifyKilled { get; set; }

        /// <summary>
        /// Can loose stamina points and get tired.
        /// </summary>
        public bool CanTire { get; set; }

        /// <summary>
        /// Loose food points as time passes and needs to eat.
        /// </summary>
        public bool HasToEat { get; set; }

        /// <summary>
        /// Loose sleep points as time passes and needs to sleep.
        /// </summary>
        public bool HasToSleep { get; set; }

        /// <summary>
        /// Sanity gauge.
        /// </summary>
        public bool HasSanity { get; set; }

        /// <summary>
        /// Can run (move at x2 speed at the cost of STA).
        /// </summary>
        public bool CanRun { get; set; }

        /// <summary>
        /// Can use voice to chat, cry...
        /// </summary>
        public bool CanTalk { get; set; }

        /// <summary>
        /// Can use map objects such as doors...
        /// </summary>
        public bool CanUseMapObjects { get; set; }

        /// <summary>
        /// Can bash doors & windows and prefer to break through stuff rather than going around.
        /// </summary>
        public bool CanBashDoors { get; set; }

        /// <summary>
        /// Can attack or bump into objects to break them.
        /// </summary>
        public bool CanBreakObjects { get; set; }

        /// <summary>
        /// Can walk on "jumpable" map objects.
        /// </summary>
        public bool CanJump { get; set; }

        /// <summary>
        /// Small actors are blocked only by closed doors.
        /// </summary>
        public bool IsSmall { get; set; }

        /// <summary>
        /// Has an inventory (carry items around).
        /// </summary>
        public bool HasInventory { get; set; }

        /// <summary>
        /// Can equip/use items.
        /// </summary>
        public bool CanUseItems { get; set; }

        /// <summary>
        /// Can trade with other actors.
        /// </summary>
        public bool CanTrade { get; set; }

        /// <summary>
        /// Can barricade map objects.
        /// </summary>
        public bool CanBarricade { get; set; }

        /// <summary>
        /// Can push/pull movable map objects.
        /// </summary>
        public bool CanPush { get; set; }

        /// <summary>
        /// Has a chance to stumble when jumping.
        /// </summary>
        public bool CanJumpStumble { get; set; }

        /// <summary>
        /// A law enforcer can attack murderers that are not law enforces with impunity.
        /// AIs will make use of this.
        /// </summary>
        public bool IsLawEnforcer { get; set; }

        /// <summary>
        /// Can do intelligent assesments like avoiding traps.
        /// </summary>
        public bool IsIntelligent { get; set; }

        /// <summary>
        /// Is slowly rotting and needs to eat flesh.
        /// </summary>
        public bool IsRotting { get; set; }

        // alpha10
        public bool CanDisarm { get; set; }

        #region AI flags
        /// <summary>
        /// AI flag : tell some AIs t can use Exits with flag Exit.IsAnAIExit.
        /// </summary>
        public bool AI_CanUseAIExits { get; set; }

        /// <summary>
        /// AI flag : ranged weapons are not interesting items.
        /// </summary>
        public bool AI_NotInterestedInRangedWeapons { get; set; }

        // alpha10 obsolete, was unused in alpha9
        ///// <summary>
        ///// AI flag : tell some AIs to use the assault barricades behavior.
        ///// </summary>
        //public bool ZombieAI_AssaultBreakables { get; set; }

        /// <summary>
        /// AI flag : tell some AIs to use the explore behavior.
        /// </summary>
        public bool ZombieAI_Explore { get; set; }
        #endregion

        #endregion

        // alpha10
        // CanDisarm by default.
        public Abilities()
        {
            this.CanDisarm = true;
        }
    }
}
