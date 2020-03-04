using System;

namespace djack.RogueSurvivor.Data
{
    class ActorModel
    {
        #region Fields
        int m_ID;
        string m_ImageID;
        DollBody m_DollBody;
        string m_Name;
        string m_PluralName;
        Abilities m_Abilities;
        ActorSheet m_StartingSheet;
        Type m_DefaultController;
        string m_FlavorDescription;
        int m_ScoreValue;

        int m_CreatedCount;
        #endregion

        #region Properties
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

 
        public string ImageID
        {
            get { return m_ImageID; }
        }

        public DollBody DollBody
        {
            get { return m_DollBody; }
        }

        public string Name
        {
            get { return m_Name; }
        }

        public string PluralName
        {
            get { return m_PluralName; }
        }

        public ActorSheet StartingSheet
        {
            get { return m_StartingSheet; }
        }

        public Abilities Abilities
        {
            get { return m_Abilities; }
        }

        public Type DefaultController
        {
            get { return m_DefaultController; }
        }

        public int CreatedCount
        {
            get { return m_CreatedCount; }
        }

        public int ScoreValue
        {
            get { return m_ScoreValue; }
        }

        public string FlavorDescription
        {
            get { return m_FlavorDescription; }
            set { m_FlavorDescription = value; }
        }
        #endregion

        #region Init
        public ActorModel(string imageID, string name, string pluralName, int scoreValue, DollBody body, Abilities abilities, ActorSheet startingSheet, Type defaultController)
        {
            if (name == null)
                throw new ArgumentNullException("name");
            if (body == null)
                throw new ArgumentNullException("body");
            if (abilities == null)
                throw new ArgumentNullException("abilities");
            if (startingSheet == null)
                throw new ArgumentNullException("startingSheet");
            if (defaultController != null && !defaultController.IsSubclassOf(typeof(ActorController)))
                throw new ArgumentException("defaultController is not a subclass of ActorController");

            m_ImageID = imageID;
            m_DollBody = body;
            m_Name = name;
            m_PluralName = pluralName;
            m_StartingSheet = startingSheet;
            m_Abilities = abilities;
            m_DefaultController = defaultController;
            m_ScoreValue = scoreValue;

            m_CreatedCount = 0;
        }
        #endregion

        #region Factory
        Actor Create(Faction faction, int spawnTime)
        {
            ++m_CreatedCount;
            return new Actor(this, faction, spawnTime) { Controller = InstanciateController() };
        }

        ActorController InstanciateController()
        {
            if (m_DefaultController == null)
                return null;
            ActorController controller = m_DefaultController.GetConstructor(System.Type.EmptyTypes).Invoke(null) as ActorController;
            return controller;
        }

        public Actor CreateAnonymous(Faction faction, int spawnTime)
        {
            return Create(faction, spawnTime);
        }

        public Actor CreateNumberedName(Faction faction, int spawnTime)
        {
            Actor actor = Create(faction, spawnTime);
            String number = m_CreatedCount.ToString();
            actor.Name += number;
            actor.IsProperName = true;
            return actor;
        }

        public Actor CreateNamed(Faction faction, string properName, bool isPluralName, int spawnTime)
        {
            Actor actor = Create(faction, spawnTime);
            actor.Name = properName;
            actor.IsProperName = true;
            actor.IsPluralName = isPluralName;
            return actor;
        }
        #endregion
    }
}
