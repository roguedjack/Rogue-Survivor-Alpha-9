using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Gameplay
{
    static class GameSounds
    {
        static readonly string PATH = @"Resources\Sfx\";

        public static readonly string UNDEAD_EAT = "undead eat";
        public static readonly string UNDEAD_EAT_FILE = PATH + "sfx - undead eat";

        public static readonly string UNDEAD_RISE = "undead rise";
        public static readonly string UNDEAD_RISE_FILE = PATH + "sfx - undead rise";

        public static readonly string NIGHTMARE = "nightmare";
        public static readonly string NIGHTMARE_FILE = PATH + "sfx - nightmare";

    }
}
