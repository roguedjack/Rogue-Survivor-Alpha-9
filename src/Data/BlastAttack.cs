using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    struct BlastAttack
    {
        public int Radius { get; private set; }
        public int[] Damage { get; private set; }
        public bool CanDamageObjects { get; private set; }
        public bool CanDestroyWalls { get; private set; }

        public BlastAttack(int radius, int[] damage, bool canDamageObjects, bool canDestroyWalls)
            : this()
        {
            if (damage.Length != radius + 1)
                throw new ArgumentException("damage.Length != radius + 1");

            this.Radius = radius;
            this.Damage = damage;
            this.CanDamageObjects = canDamageObjects;
            this.CanDestroyWalls = canDestroyWalls;
        }
    }
}
