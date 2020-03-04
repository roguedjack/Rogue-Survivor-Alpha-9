using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;
using djack.RogueSurvivor.Engine.MapObjects;
using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Gameplay.AI;
using djack.RogueSurvivor.UI;

namespace djack.RogueSurvivor.Gameplay.Generators
{
    class StdTownGenerator : BaseTownGenerator
    {
        public StdTownGenerator(RogueGame game, BaseTownGenerator.Parameters parameters)
            : base(game, parameters)
        {
        }

        public override Map Generate(int seed)
        {
            Map map = base.Generate(seed);
            map.Name = "Std City";

            /////////////////////////////////
            // People and undeads in surface.
            /////////////////////////////////
            int maxTries = 10 * map.Width * map.Height;
#if true
            // civilians (includes police)
            for (int i = 0; i < RogueGame.Options.MaxCivilians; i++)
            {
                // policeman, civilian?
                if (m_DiceRoller.RollChance(this.Params.PolicemanChance))
                {
                    // create policeman.
                    Actor cop = CreateNewPoliceman(0);
                    // policeman on patrol starts outside.
                    base.ActorPlace(m_DiceRoller, maxTries, map, cop, (pt) => !map.GetTileAt(pt.X, pt.Y).IsInside);
                }
                else
                {
                    // create civilian with 1 skill.
                    Actor civilian = CreateNewCivilian(0, 0, 1);
                    // civilian starts inside.
                    base.ActorPlace(m_DiceRoller, maxTries, map, civilian, (pt) => map.GetTileAt(pt.X, pt.Y).IsInside);
                }
            }
            // dogs
            // alpha10 dogs entirely disabled for now, much more work to do on them.
            /*
            for (int i = 0; i < RogueGame.Options.MaxDogs; i++)
            {
                // feral.
                Actor dog = CreateNewFeralDog(0);
                base.ActorPlace(m_DiceRoller, maxTries, map, dog, (pt) => !map.GetTileAt(pt.X, pt.Y).IsInside);
            }
            */
#if true
            // start with day zero nb of undeads.
            int nbUndeads = (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100;
            for (int i = 0; i < nbUndeads; i++)
            {
                Actor undead = CreateNewUndead(0);
                base.ActorPlace(m_DiceRoller, maxTries, map, undead, (pt) => !map.GetTileAt(pt.X, pt.Y).IsInside);
            }
#endif

#endif
            return map;
        }

        public override Map GenerateSewersMap(int seed, District district)
        {
            Map sewers = base.GenerateSewersMap(seed, district);

            ////////////////////////////////
            // People and undeads in sewers
            ////////////////////////////////
            if (Rules.HasZombiesInSewers(m_Game.Session.GameMode))
            {
                int maxTries = 10 * sewers.Width * sewers.Height;
                // start with day zero nb of undeads.
                int nbUndeads = (int)(RogueGame.SEWERS_UNDEADS_FACTOR * (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100);
                for (int i = 0; i < nbUndeads; i++)
                {
                    Actor undead = CreateNewSewersUndead(0);
                    base.ActorPlace(m_DiceRoller, maxTries, sewers, undead);
                }
            }

            return sewers;
        }


        public override Map GenerateSubwayMap(int seed, District district)
        {
            Map subway = base.GenerateSubwayMap(seed, district);

#if false
            DISABLED
            ////////////////////////////////
            // People and undeads in subways.
            ////////////////////////////////
            // undeads, in rails zone.
            int nbUndeads = (int)(RogueGame.SUBWAY_UNDEADS_FACTOR * (RogueGame.Options.MaxUndeads * RogueGame.Options.DayZeroUndeadsPercent) / 100);
            for (int i = 0; i < nbUndeads; i++)
            {
                Actor undead = CreateNewSubwayUndead(0);
                base.ActorPlace(m_DiceRoller, 1000, subway, undead,
                    (pt) => subway.HasZonePartiallyNamedAt(pt, "rails"));
            }
#endif
            return subway;
        }
    }
}
