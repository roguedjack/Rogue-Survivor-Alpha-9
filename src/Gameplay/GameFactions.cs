using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;
using djack.RogueSurvivor.Engine;

namespace djack.RogueSurvivor.Gameplay
{
    class GameFactions : FactionDB
    {
        #region IDs
        public enum IDs
        {
            _FIRST,

            TheCHARCorporation = _FIRST,
            TheCivilians,
            TheUndeads,
            TheArmy,
            TheBikers,
            TheGangstas,
            ThePolice,
            TheBlackOps,
            ThePsychopaths,
            TheSurvivors,
            TheFerals,

            _COUNT
        }
        #endregion

        #region Fields
        Faction[] m_Factions = new Faction[(int)IDs._COUNT];
        #endregion

        #region Properties
        public override Faction this[int id]
        {
            get { return m_Factions[id]; }
        }

        public Faction this[IDs id]
        {
            get { return this[(int)id]; }
            private set
            {
                m_Factions[(int)id] = value;
                m_Factions[(int)id].ID = (int)id;
            }
        }
        public Faction TheArmy { get { return this[IDs.TheArmy]; } }
        public Faction TheBikers { get { return this[IDs.TheBikers]; } }
        public Faction TheBlackOps { get { return this[IDs.TheBlackOps]; } }
        public Faction TheCHARCorporation { get { return this[IDs.TheCHARCorporation]; } }
        public Faction TheCivilians { get { return this[IDs.TheCivilians]; } }
        public Faction TheGangstas { get { return this[IDs.TheGangstas]; } }
        public Faction ThePolice { get { return this[IDs.ThePolice]; } }
        public Faction TheUndeads { get { return this[IDs.TheUndeads]; } }
        public Faction ThePsychopaths { get { return this[IDs.ThePsychopaths]; } }
        public Faction TheSurvivors { get { return this[IDs.TheSurvivors]; } }
        public Faction TheFerals { get { return this[IDs.TheFerals]; } }
        #endregion

        public static readonly GameItems.IDs[] BAD_POLICE_OUTFITS = new GameItems.IDs[] 
        { 
            GameItems.IDs.ARMOR_FREE_ANGELS_JACKET, GameItems.IDs.ARMOR_HELLS_SOULS_JACKET 
        };
        public static readonly GameItems.IDs[] GOOD_POLICE_OUTFITS = new GameItems.IDs[] 
        { 
            GameItems.IDs.ARMOR_POLICE_JACKET, GameItems.IDs.ARMOR_POLICE_RIOT 
        };

        public GameFactions()
        {
            // bind
            Models.Factions = this;

            // factions
            this[IDs.TheArmy] = new Faction("Army", "soldier") { LeadOnlyBySameFaction = true };
            this[IDs.TheBikers] = new Faction("Bikers", "biker") { LeadOnlyBySameFaction = true };
            this[IDs.TheBlackOps] = new Faction("BlackOps", "blackOp") { LeadOnlyBySameFaction = true };
            this[IDs.TheCHARCorporation] = new Faction("CHAR Corp.", "CHAR employee") { LeadOnlyBySameFaction = true };
            this[IDs.TheCivilians] = new Faction("Civilians", "civilian");
            this[IDs.TheGangstas] = new Faction("Gangstas", "gangsta") { LeadOnlyBySameFaction = true };
            this[IDs.ThePolice] = new Faction("Police", "police officer") { LeadOnlyBySameFaction = true };
            this[IDs.TheUndeads] = new Faction("Undeads", "undead");
            this[IDs.ThePsychopaths] = new Faction("Psychopaths", "psychopath");
            this[IDs.TheSurvivors] = new Faction("Survivors", "survivor");
            this[IDs.TheFerals] = new Faction("Ferals", "feral") { LeadOnlyBySameFaction = true };

            // relations.
            this[IDs.TheArmy].AddEnemy(this[IDs.TheBikers]);
            this[IDs.TheArmy].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheArmy].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.TheArmy].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheArmy].AddEnemy(this[IDs.ThePsychopaths]);
            
            this[IDs.TheBikers].AddEnemy(this[IDs.TheArmy]);
            this[IDs.TheBikers].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheBikers].AddEnemy(this[IDs.TheCHARCorporation]);
            this[IDs.TheBikers].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.TheBikers].AddEnemy(this[IDs.ThePolice]);
            this[IDs.TheBikers].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheBikers].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheArmy]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheBikers]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheCHARCorporation]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheCivilians]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.ThePolice]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.ThePsychopaths]);
            this[IDs.TheBlackOps].AddEnemy(this[IDs.TheSurvivors]);

            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheArmy]);
            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheBikers]);
            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheCHARCorporation].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.TheCivilians].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheCivilians].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheCivilians].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.TheGangstas].AddEnemy(this[IDs.TheArmy]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.TheBikers]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.TheCHARCorporation]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.ThePolice]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheGangstas].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.ThePolice].AddEnemy(this[IDs.TheBikers]);
            this[IDs.ThePolice].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.ThePolice].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.ThePolice].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.ThePolice].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.TheUndeads].AddEnemy(this[IDs.TheArmy]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheBikers]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheCHARCorporation]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheCivilians]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.ThePolice]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.ThePsychopaths]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheSurvivors]);
            this[IDs.TheUndeads].AddEnemy(this[IDs.TheFerals]);

            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheArmy]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheBikers]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheCHARCorporation]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheCivilians]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheGangstas]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.ThePolice]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.ThePsychopaths].AddEnemy(this[IDs.TheSurvivors]);

            this[IDs.TheSurvivors].AddEnemy(this[IDs.TheBlackOps]);
            this[IDs.TheSurvivors].AddEnemy(this[IDs.TheUndeads]);
            this[IDs.TheSurvivors].AddEnemy(this[IDs.ThePsychopaths]);

            this[IDs.TheFerals].AddEnemy(this[IDs.TheUndeads]);

            // make sure relations are symetric!
            foreach (Faction f in m_Factions)
            {
                foreach (Faction fe in f.Enemies)
                {
                    if (!fe.IsEnemyOf(fe))
                        fe.AddEnemy(f);
                }
            }
        }
    }

}
