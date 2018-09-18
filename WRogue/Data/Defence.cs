using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    struct Defence
    {
        #region Blank defence
        [NonSerialized]
        public static readonly Defence BLANK = new Defence(0, 0, 0);
        #endregion

        #region Properties
        public int Value { get; private set; }
        public int Protection_Hit { get; private set; }
        public int Protection_Shot { get; private set; }
        #endregion

        #region Init
        public Defence(int value, int protection_hit, int protection_shot)
            : this()
        {
            this.Value = value;
            this.Protection_Hit = protection_hit;
            this.Protection_Shot = protection_shot;
        }
        #endregion

        #region Operators
        public static Defence operator+(Defence lhs, Defence rhs)
        {
            return new Defence(lhs.Value + rhs.Value, lhs.Protection_Hit + rhs.Protection_Hit, lhs.Protection_Shot + rhs.Protection_Shot);
        }

        public static Defence operator-(Defence lhs, Defence rhs)
        {
            return new Defence(lhs.Value - rhs.Value, lhs.Protection_Hit - rhs.Protection_Hit, lhs.Protection_Shot - rhs.Protection_Shot);
        }
        #endregion
    }
}
