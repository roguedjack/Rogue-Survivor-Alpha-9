using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    static class Models
    {
        public static ActorModelDB Actors { get; set; }
        public static FactionDB Factions { get; set; }
        public static ItemModelDB Items { get; set; }
        public static TileModelDB Tiles { get; set; }      
    }
}
