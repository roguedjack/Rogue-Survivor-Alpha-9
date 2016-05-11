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
        public static readonly Attack BLANK = new Attack(AttackKind.PHYSICAL, new Verb("<blank>"), 0, 0, 0, 0);
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
        #endregion

        #region Init
        public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue, int staminaPenalty, int range)
            : this()
        {
            if (verb == null)
                throw new ArgumentNullException("verb");

            this.Kind = kind;
            this.Verb = verb;
            this.HitValue = hitValue;
            this.DamageValue = damageValue;
            this.StaminaPenalty = staminaPenalty;
            this.Range = range;
        }

        public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue)
            : this(kind, verb, hitValue, damageValue, 0, 0)
        {
        }

        public Attack(AttackKind kind, Verb verb, int hitValue, int damageValue, int staminaPenalty)
            : this(kind, verb, hitValue, damageValue, staminaPenalty, 0)
        {
        }
        #endregion
    }
}
