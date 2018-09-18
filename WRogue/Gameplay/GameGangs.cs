using System;

namespace djack.RogueSurvivor.Gameplay
{
    static class GameGangs
    {
        [Serializable]
        public enum IDs
        {
            NONE,
            BIKER_HELLS_SOULS,
            BIKER_FREE_ANGELS,
            GANGSTA_CRAPS,
            GANGSTA_FLOODS
        }

        public static readonly IDs[] GANGSTAS = 
        {
            IDs.GANGSTA_CRAPS,
            IDs.GANGSTA_FLOODS
        };

        public static readonly IDs[] BIKERS = 
        {
            IDs.BIKER_HELLS_SOULS,
            IDs.BIKER_FREE_ANGELS
        };

        public static readonly string[] NAMES = 
        {
            "(no gang)",
            "Hell's Souls",
            "Free Angels",
            "Craps",
            "Floods"
        };

        public static readonly GameItems.IDs[][] BAD_GANG_OUTFITS = new GameItems.IDs[][] 
        { 
            // none
            new GameItems.IDs[] {},
            // Biker Hells Souls
            new GameItems.IDs[] { GameItems.IDs.ARMOR_FREE_ANGELS_JACKET, GameItems.IDs.ARMOR_POLICE_JACKET, GameItems.IDs.ARMOR_POLICE_RIOT },
            // Biker Free Angels
            new GameItems.IDs[] { GameItems.IDs.ARMOR_HELLS_SOULS_JACKET, GameItems.IDs.ARMOR_POLICE_JACKET, GameItems.IDs.ARMOR_POLICE_RIOT },
            // Gangsta Craps
            new GameItems.IDs[] { GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,  GameItems.IDs.ARMOR_HELLS_SOULS_JACKET, GameItems.IDs.ARMOR_POLICE_JACKET, GameItems.IDs.ARMOR_POLICE_RIOT },
            // Gangs Floods
            new GameItems.IDs[] { GameItems.IDs.ARMOR_FREE_ANGELS_JACKET,  GameItems.IDs.ARMOR_HELLS_SOULS_JACKET, GameItems.IDs.ARMOR_POLICE_JACKET, GameItems.IDs.ARMOR_POLICE_RIOT }
        };

        public static readonly GameItems.IDs[][] GOOD_GANG_OUTFITS = new GameItems.IDs[][] 
        { 
            // none
            new GameItems.IDs[] {},
            // Biker Hells Souls
            new GameItems.IDs[] { GameItems.IDs.ARMOR_HELLS_SOULS_JACKET },
            // Biker Free Angels
            new GameItems.IDs[] { GameItems.IDs.ARMOR_FREE_ANGELS_JACKET },
            // Gangsta Craps
            new GameItems.IDs[] {},
            // Gangs Floods
            new GameItems.IDs[] {}
        };

    }
}