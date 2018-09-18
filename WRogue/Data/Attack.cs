using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    enum AttackKind
    {
        PHYSICAL,
        FIREARM,
        BOW
    }

    [Serializable]
    enum FireMode
    {
        DEFAULT = 0,
        RAPID,
        _COUNT
    }

    [Serializable]
    struct Attack
    {
        #region Blank attack
        [NonSerialized]
        public static readonly Attack BLANK = new Attack(AttackKind.PHYSICAL, new Verb("<blank>"), 0, 0, 0, 0, 0, 0, 0);
        #endregion

        #region Properties
        public AttackKind Kind { get; private set; }

        public Verb Verb { get; private set; }

        /// <summary>
        /// Hit roll value.
        /// </summary>
        public int HitValue { get; private set; }

        /// <summary>
        /// Damage roll value.
        /// </summary>
        public int DamageValue { get; private set; }

        /// <summary>
        ///  Additional stamina cost for performing the attack.
        /// </summary>
        public int StaminaPenalty { get; private set; }

        public int Range { get; private set; }

        public int EfficientRange
        {
            get { return this.Range / 2; }
        }

        // alpha10

        public int DisarmChance { get; private set; }

        /// <summary>
        /// Secondary hit roll value.
        /// Eg: rapid fire 1st shot
        /// </summary>
        public int Hit2Value { get; private set; }

        /// <summary>
        /// Tertiary hit roll value.
        /// Eg: rapid fire 2nd shot
        /// </summary>
        public int Hit3Value { get; private set; }
        #endregion

        #region Init
        public Attack(AttackKind kind, Verb verb, int hitValue, int hit2Value, int hit3Value, int damageValue, int staminaPenalty, int disarmChance, int range)
            : this()
        {
            if (verb == null)
                throw new ArgumentNullException("verb");

            this.Kind = kind;
            this.Verb = verb;
            this.HitValue = hitValue;
            this.Hit2Value = hit2Value;
            this.Hit3Value = hit3Value;
            this.DamageValue = damageValue;
            this.StaminaPenalty = staminaPenalty;
            this.DisarmChance = disarmChance;
            this.Range = range;
        }

        /// <summary>
        /// Constructor for actor base melee attacks.
        /// </summary>
        /// <param name="kind"></param>
        /// <param name="verb"></param>
        /// <param name="hitValue"></param>
        /// <param name="damageValue"></param>
        public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue)
            : this(kind, verb, hitValue, hitValue, hitValue, damageValue, 0, 0, 0)
        {
        }

        // alpha10 removed other constructors to avoid confusion and replaced with static methods

        public static Attack MeleeAttack(Verb verb, int hitValue, int damageValue, int staminaPenalty, int disarmChance)
        {
            return new Attack(AttackKind.PHYSICAL, verb, hitValue, hitValue, hitValue, damageValue, staminaPenalty, disarmChance, 0);
        }

        public static Attack RangedAttack(AttackKind kind, Verb verb, int normalHitValue, int rapidFire1HitValue, int rapidFire2HitValue, int damageValue, int range)
        {
            return new Attack(kind, verb, normalHitValue, rapidFire1HitValue, rapidFire2HitValue, damageValue, 0, 0, range);
        }
        #endregion
    }
}
