using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    enum Odor
    {
        LIVING,
        UNDEAD_MASTER,

        // alpha 10
        SUPPRESSOR
        //PERFUME_LIVING_SUPRESSOR,
        //PERFUME_LIVING_GENERATOR
    }

    [Serializable]
    class OdorScent
    {
        public const int MIN_STRENGTH = 1;
        public const int MAX_STRENGTH = 9 * WorldTime.TURNS_PER_HOUR;

        public Odor Odor { get; private set; }
        public int Strength { get; private set; }
        public Point Position { get; private set; }

        public OdorScent(Odor odor, int strength, Point position)
        {
            this.Odor = odor;
            this.Strength = Math.Min(MAX_STRENGTH, strength);
            this.Position = position;
        }

        public void Change(int amount)
        {
            int str = this.Strength + amount;

            if (str < MIN_STRENGTH) str = 0;
            else if (str > MAX_STRENGTH) str = MAX_STRENGTH;

            this.Strength = str;
        }

        public void Set(int value)
        {
            int str = value;

            if (str < MIN_STRENGTH) str = 0;
            else if (str > MAX_STRENGTH) str = MAX_STRENGTH;

            this.Strength = str;

        }

    }
}
