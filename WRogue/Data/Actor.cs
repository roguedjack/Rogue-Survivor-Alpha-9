using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Gameplay;
using djack.RogueSurvivor.Engine.Items;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class TrustRecord
    {
        public Actor Actor { get; set; }
        public int Trust { get; set; }
    }

    [Serializable]
    class Actor
    {
        #region Flags
        [Flags]
        enum Flags
        {
            NONE = 0,
            IS_UNIQUE = (1 << 0),
            IS_PROPER_NAME = (1 << 1),
            IS_PLURAL_NAME = (1 << 2),
            IS_DEAD = (1 << 3),
            IS_RUNNING = (1 << 4),
            IS_SLEEPING = (1 << 5)
        }
        #endregion

        #region Fields
        Flags m_Flags;

        #region Definition
        int m_ModelID;
        /*bool m_IsUnique;*/
        int m_FactionID;
        int m_GangID;
        string m_Name;
        /*bool m_IsProperName;
        bool m_IsPluralName;*/
        ActorController m_Controller;
        bool m_isBotPlayer;  // alpha10.1
        ActorSheet m_Sheet;
        int m_SpawnTime;
        #endregion

        #region State
        Inventory m_Inventory = null;
        Doll m_Doll;
        int m_HitPoints;
        int m_previousHitPoints;
        int m_StaminaPoints;
        int m_previousStamina;
        int m_FoodPoints;
        int m_previousFoodPoints;
        int m_SleepPoints;
        int m_previousSleepPoints;
        int m_Sanity;
        int m_previousSanity;
        Location m_Location;
        int m_ActionPoints;
        int m_LastActionTurn;
        Activity m_Activity = Activity.IDLE;
        Actor m_TargetActor;
        int m_AudioRangeMod;
        Attack m_CurrentMeleeAttack;
        Attack m_CurrentRangedAttack;
        Defence m_CurrentDefence;
        Actor m_Leader;
        List<Actor> m_Followers = null;
        int m_TrustInLeader;
        List<TrustRecord> m_TrustList = null;
        int m_KillsCount;
        List<Actor> m_AggressorOf = null;
        List<Actor> m_SelfDefenceFrom = null;
        int m_MurdersCounter;
        int m_Infection;
        Corpse m_DraggedCorpse;
        // alpha10 moved out of Actor
        //List<Item> m_BoringItems = null;
        // alpha10
        bool m_IsInvincible;
        int m_OdorSuppressorCounter;
        #endregion
        #endregion

        #region Properties
        #region Definition

        /// <summary>
        /// Gets or sets model. Setting model reset inventory and all stats to the model default values.
        /// </summary>
        public ActorModel Model
        {
            get { return Models.Actors[m_ModelID]; }
            set
            {
                m_ModelID = value.ID;
                OnModelSet();
            }
        }

        public bool IsUnique
        {
            get { return GetFlag(Flags.IS_UNIQUE); }
            set { SetFlag(Flags.IS_UNIQUE, value); }
        }

        public Faction Faction
        {
            get { return Models.Factions[m_FactionID]; }
            set { m_FactionID = value.ID; }
        }

        /// <summary>
        /// Appends "(YOU) " if the actor is the player.
        /// </summary>
        public string Name
        {
            get { return IsPlayer ? "(YOU) " + m_Name : m_Name; }
            set
            {
                m_Name = value;
                if (value != null)
                    m_Name.Replace("(YOU) ", "");
            }
        }

        /// <summary>
        /// Raw name without "(YOU) " for the player.
        /// </summary>
        public string UnmodifiedName
        {
            get { return m_Name; }
        }

        public bool IsProperName
        {
            get { return GetFlag(Flags.IS_PROPER_NAME); }
            set { SetFlag(Flags.IS_PROPER_NAME, value); }
        }

        public bool IsPluralName
        {
            get { return GetFlag(Flags.IS_PLURAL_NAME); }
            set { SetFlag(Flags.IS_PLURAL_NAME, value); }
        }

        public string TheName
        {
            get { return IsProperName || IsPluralName ? Name : "the " + m_Name; }
        }

        public ActorController Controller
        {
            get { return m_Controller; }
            set
            {
                if (m_Controller != null)
                    m_Controller.LeaveControl();
                m_Controller = value;
                if (m_Controller != null)
                    m_Controller.TakeControl(this);
            }
        }

        /// <summary>
        /// Gets if this actor is controlled by the player or bot.
        /// </summary>
        public bool IsPlayer
        {
            get { return m_Controller != null && m_Controller is PlayerController; }
        }

        // alpha10.1 bot
        /// <summary>
        /// Gets or set if this actor is declared as controlled by a bot.
        /// The controller is STILL PlayerController and IsPlayer will still return true.
        /// We need this because in some rare parts of the code outside of RogueGame we need to know if the player is a bot or not.
        /// See RogueGame.
        /// </summary>
        public bool IsBotPlayer
        {
            get { return m_isBotPlayer; }
            set { m_isBotPlayer = value;  }
        }

        public int SpawnTime
        {
            get { return m_SpawnTime; }
        }

        public int GangID
        {
            get { return m_GangID; }
            set { m_GangID = value; }
        }

        public bool IsInAGang
        {
            get { return m_GangID != (int)GameGangs.IDs.NONE; }
        }
        #endregion

        #region State
        public Doll Doll
        {
            get { return m_Doll; }
        }

        public bool IsDead
        {
            get { return GetFlag(Flags.IS_DEAD); }
            set { SetFlag(Flags.IS_DEAD, value); }
        }

        public bool IsSleeping
        {
            get { return GetFlag(Flags.IS_SLEEPING); }
            set { SetFlag(Flags.IS_SLEEPING, value); }
        }

        public bool IsRunning
        {
            get { return GetFlag(Flags.IS_RUNNING); }
            set { SetFlag(Flags.IS_RUNNING, value); }
        }

        public Inventory Inventory
        {
            get { return m_Inventory; }
            set { m_Inventory = value; }
        }

        public int HitPoints
        {
            get { return m_HitPoints; }
            set
            {
                if (m_IsInvincible && value < m_HitPoints) // alpha10
                    return;
                m_HitPoints = value;
            }
        }

        public int PreviousHitPoints
        {
            get { return m_previousHitPoints; }
            set { m_previousHitPoints = value; }
        }

        public int StaminaPoints
        {
            get { return m_StaminaPoints; }
            set
            {
                if (m_IsInvincible && value < m_StaminaPoints) // alpha10
                    return;
                m_StaminaPoints = value;
            }
        }

        public int PreviousStaminaPoints
        {
            get { return m_previousStamina; }
            set { m_previousStamina = value; }
        }

        public int FoodPoints
        {
            get { return m_FoodPoints; }
            set
            {
                if (m_IsInvincible && value < m_FoodPoints) // alpha10
                    return;
                m_FoodPoints = value;
            }
        }

        public int PreviousFoodPoints
        {
            get { return m_previousFoodPoints; }
            set { m_previousFoodPoints = value; }
        }

        public int SleepPoints
        {
            get { return m_SleepPoints; }
            set
            {
                if (m_IsInvincible && value < m_SleepPoints) // alpha10
                    return;
                m_SleepPoints = value;
            }
        }

        public int PreviousSleepPoints
        {
            get { return m_previousSleepPoints; }
            set { m_previousSleepPoints = value; }
        }

        public int Sanity
        {
            get { return m_Sanity; }
            set
            {
                if (m_IsInvincible && value < m_Sanity) // alpha10
                    return;
                m_Sanity = value;
            }
        }

        public int PreviousSanity
        {
            get { return m_previousSanity; }
            set { m_previousSanity = value; }
        }

        public ActorSheet Sheet
        {
            get { return m_Sheet; }
        }

        public int ActionPoints
        {
            get { return m_ActionPoints; }
            set { m_ActionPoints = value; }
        }

        public int LastActionTurn
        {
            get { return m_LastActionTurn; }
            set { m_LastActionTurn = value; }
        }

        public Location Location
        {
            get { return m_Location; }
            set { m_Location = value; }
        }

        public Activity Activity
        {
            get { return m_Activity; }
            set { m_Activity = value; }
        }

        public Actor TargetActor
        {
            get { return m_TargetActor; }
            set { m_TargetActor = value; }
        }

        public int AudioRange
        {
            get { return m_Sheet.BaseAudioRange + m_AudioRangeMod; }
        }

        public int AudioRangeMod
        {
            get { return m_AudioRangeMod; }
            set { m_AudioRangeMod = value; }
        }

        public Attack CurrentMeleeAttack
        {
            get { return m_CurrentMeleeAttack; }
            set { m_CurrentMeleeAttack = value; }
        }

        public Attack CurrentRangedAttack
        {
            get { return m_CurrentRangedAttack; }
            set { m_CurrentRangedAttack = value; }
        }

        public Defence CurrentDefence
        {
            get { return m_CurrentDefence; }
            set { m_CurrentDefence = value; }
        }

        public Actor Leader
        {
            get { return m_Leader; }
        }

        /// <summary>
        /// Gets if has a leader and he is alive.
        /// </summary>
        public bool HasLeader
        {
            get { return m_Leader != null && !m_Leader.IsDead; }
        }

        public int TrustInLeader
        {
            get { return m_TrustInLeader; }
            set { m_TrustInLeader = value; }
        }

        public IEnumerable<Actor> Followers
        {
            get { return m_Followers; }
        }

        public int CountFollowers
        {
            get
            {
                if (m_Followers == null)
                    return 0;
                return m_Followers.Count;
            }
        }

        public int KillsCount
        {
            get { return m_KillsCount; }
            set { m_KillsCount = value; }
        }

        public IEnumerable<Actor> AggressorOf
        {
            get { return m_AggressorOf; }
        }

        public int CountAggressorOf
        {
            get
            {
                if (m_AggressorOf == null)
                    return 0;
                return m_AggressorOf.Count;
            }
        }

        public IEnumerable<Actor> SelfDefenceFrom
        {
            get { return m_SelfDefenceFrom; }
        }

        public int CountSelfDefenceFrom
        {
            get
            {
                if (m_SelfDefenceFrom == null)
                    return 0;
                return m_SelfDefenceFrom.Count;
            }
        }

        public int MurdersCounter
        {
            get { return m_MurdersCounter; }
            set { m_MurdersCounter = value; }
        }

        public int Infection
        {
            get { return m_Infection; }
            set
            {
                if (m_IsInvincible && value > m_Infection) // alpha10
                    return;
                m_Infection = value;
            }
        }

        public Corpse DraggedCorpse
        {
            get { return m_DraggedCorpse; }
            set { m_DraggedCorpse = value; }
        }

        // alpha 10
        public bool IsInvincible
        {
            get { return m_IsInvincible; }
            set { m_IsInvincible = value; }
        }
        
        public int OdorSuppressorCounter
        {
            get { return m_OdorSuppressorCounter; }
            set { m_OdorSuppressorCounter = value; }
        }
        #endregion
        #endregion

        #region Init
        public Actor(ActorModel model, Faction faction, string name, bool isProperName, bool isPluralName, int spawnTime)
        {
            if (model == null)
                throw new ArgumentNullException("model");
            if (faction == null)
                throw new ArgumentNullException("faction");
            if (name == null)
                throw new ArgumentNullException("name");

            m_ModelID = model.ID;
            m_FactionID = faction.ID;
            m_GangID = (int)GameGangs.IDs.NONE;
            m_Name = name;
            this.IsProperName = isProperName;
            this.IsPluralName = isPluralName;
            m_Location = new Location();
            m_SpawnTime = spawnTime;
            this.IsUnique = false;
            this.IsDead = false;

            OnModelSet();
        }

        public Actor(ActorModel model, Faction faction, int spawnTime)
            : this(model, faction, model.Name, false, false, spawnTime)
        {
        }

        void OnModelSet()
        {
            ActorModel model = this.Model;

            m_Doll = new Doll(model.DollBody);
            m_Sheet = new ActorSheet(model.StartingSheet);

            // starting points maxed.
            m_ActionPoints = m_Doll.Body.Speed;
            m_HitPoints = m_previousHitPoints = m_Sheet.BaseHitPoints;
            m_StaminaPoints = m_previousStamina = m_Sheet.BaseStaminaPoints;
            m_FoodPoints = m_previousFoodPoints = m_Sheet.BaseFoodPoints;
            m_SleepPoints = m_previousSleepPoints = m_Sheet.BaseSleepPoints;
            m_Sanity = m_previousSanity = m_Sheet.BaseSanity;

            // create inventory.
            if (model.Abilities.HasInventory)
                m_Inventory = new Inventory(model.StartingSheet.BaseInventoryCapacity);

            // starting attacks.
            m_CurrentMeleeAttack = model.StartingSheet.UnarmedAttack;
            m_CurrentDefence = model.StartingSheet.BaseDefence;
            m_CurrentRangedAttack = Attack.BLANK;
        }
        #endregion

        #region Group, Followers & Trust
        public void AddFollower(Actor other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (m_Followers != null && m_Followers.Contains(other))
                throw new ArgumentException("other is already a follower");

            if (m_Followers == null)
                m_Followers = new List<Actor>(1);
            m_Followers.Add(other);

            if (other.Leader != null)
                other.Leader.RemoveFollower(other);
            other.m_Leader = this;
        }

        public void RemoveFollower(Actor other)
        {
            if (other == null)
                throw new ArgumentNullException("other");
            if (m_Followers == null)
                throw new InvalidOperationException("no followers");

            m_Followers.Remove(other);
            if (m_Followers.Count == 0)
                m_Followers = null;

            other.m_Leader = null;

            // reset directives & order.
            AIController ai = other.Controller as AIController;
            if (ai != null)
            {
                ai.Directives.Reset();
                ai.SetOrder(null);
            }
        }

        public void RemoveAllFollowers()
        {
            while (m_Followers != null && m_Followers.Count > 0)
            {
                RemoveFollower(m_Followers[0]);
            }
        }

        public void SetTrustIn(Actor other, int trust)
        {
            if (m_TrustList == null)
            {
                m_TrustList = new List<TrustRecord>(1) { new TrustRecord() { Actor = other, Trust = trust } };
                return;
            }

            foreach (TrustRecord r in m_TrustList)
            {
                if (r.Actor == other)
                {
                    r.Trust = trust;
                    return;
                }
            }

            m_TrustList.Add(new TrustRecord() { Actor = other, Trust = trust });
        }

        public int GetTrustIn(Actor other)
        {
            if (m_TrustList == null) return 0;
            foreach (TrustRecord r in m_TrustList)
            {
                if (r.Actor == other)
                    return r.Trust;
            }
            return 0;
        }
        
        // alpha10
        /// <summary>
        /// Is this other actor our leader, a follower or a mate.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsInGroupWith(Actor other)
        {
            // my leader?
            if (HasLeader && m_Leader == other)
                return true;

            // a mate?
            if (other.HasLeader && other.Leader == m_Leader)
                return true;

            // a follower?
            if (m_Followers != null)
                if (m_Followers.Contains(other))
                    return true;

            // nope
            return false;
        }
        #endregion

        #region Aggressor & Self Defence
        public void MarkAsAgressorOf(Actor other)
        {
            if (other == null || other.IsDead)
                return;

            if (m_AggressorOf == null)
                m_AggressorOf = new List<Actor>(1);
            else if (m_AggressorOf.Contains(other))
                return;
            m_AggressorOf.Add(other);
        }

        public void MarkAsSelfDefenceFrom(Actor other)
        {
            if (other == null || other.IsDead)
                return;

            if (m_SelfDefenceFrom == null)
                m_SelfDefenceFrom = new List<Actor>(1);
            else if (m_SelfDefenceFrom.Contains(other))
                return;
            m_SelfDefenceFrom.Add(other);
        }

        public bool IsAggressorOf(Actor other)
        {
            if (m_AggressorOf == null)
                return false;
            return m_AggressorOf.Contains(other);
        }

        public bool IsSelfDefenceFrom(Actor other)
        {
            if (m_SelfDefenceFrom == null)
                return false;
            return m_SelfDefenceFrom.Contains(other);
        }

        public void RemoveAggressorOf(Actor other)
        {
            if (m_AggressorOf == null)
                return;
            m_AggressorOf.Remove(other);
            if (m_AggressorOf.Count == 0)
                m_AggressorOf = null;
        }

        public void RemoveSelfDefenceFrom(Actor other)
        {
            if (m_SelfDefenceFrom == null)
                return;
            m_SelfDefenceFrom.Remove(other);
            if (m_SelfDefenceFrom.Count == 0)
                m_SelfDefenceFrom = null;
        }

        public void RemoveAllAgressorSelfDefenceRelations()
        {
            while (m_AggressorOf != null)
            {
                Actor other = m_AggressorOf[0];
                RemoveAggressorOf(other);
                other.RemoveSelfDefenceFrom(this);
            }
            while (m_SelfDefenceFrom != null)
            {
                Actor other = m_SelfDefenceFrom[0];
                RemoveSelfDefenceFrom(other);
                other.RemoveAggressorOf(this);
            }
        }

        // alpha10 obsolete, moved to Rules
#if false

        /// <summary>
        /// Check for agressor/self defence relation.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AreDirectEnemies(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            if (m_AggressorOf != null)
            {
                if (m_AggressorOf.Contains(other))
                    return true;
            }
            if (m_SelfDefenceFrom != null)
            {
                if (m_SelfDefenceFrom.Contains(other))
                    return true;
            }

            if (other.IsAggressorOf(this))
                return true;
            if (other.IsSelfDefenceFrom(this))
                return true;

            // nope.
            return false;
        }

        /// <summary>
        /// Check for direct enemies through leader/followers.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool AreIndirectEnemies(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            // check my leader and my mates, if any.
            if (this.HasLeader)
            {
                // my leader.
                if (m_Leader.AreDirectEnemies(other))
                    return true;
                // my mates = my leader followers.
                foreach (Actor mate in m_Leader.Followers)
                    if (mate != this && mate.AreDirectEnemies(other))
                        return true;
            }

            // check my followers, if any.
            if (this.CountFollowers > 0)
            {
                foreach (Actor fo in m_Followers)
                    if (fo.AreDirectEnemies(other))
                        return true;
            }

            // check their leader and mates.
            if (other.HasLeader)
            {
                // his leader.
                if (other.Leader.AreDirectEnemies(this))
                    return true;
                // his mates = his leader followers.
                foreach (Actor mate in other.Leader.Followers)
                    if (mate != other && mate.AreDirectEnemies(this))
                        return true;
            }

            // nope.
            return false;
        }
#endif

#if false
        /// <summary>
        /// Make sure another actor is added to the list of personal enemies.
        /// </summary>
        /// <param name="e"></param>
        public void MarkAsPersonalEnemy(Actor e)
        {
            if (e == null || e.IsDead)
                return;

            if (m_PersonalEnemies == null)
                m_PersonalEnemies = new List<Actor>(1);
            else if (m_PersonalEnemies.Contains(e))
                return;

            m_PersonalEnemies.Add(e);
        }

        public void RemoveAsPersonalEnemy(Actor e)
        {
            if (m_PersonalEnemies == null)
                return;

            m_PersonalEnemies.Remove(e);

            // minimize data size.
            if (m_PersonalEnemies.Count == 0)
                m_PersonalEnemies = null; 
        }

        public void RemoveAllPersonalEnemies()
        {
            if (m_PersonalEnemies == null)
                return;

            while (m_PersonalEnemies.Count > 0)
            {
                Actor e = m_PersonalEnemies[0];
                e.RemoveAsPersonalEnemy(this);
                m_PersonalEnemies.Remove(e);
            }
        }

        /// <summary>
        /// Checks for own personal enemy as well as our band (leader & followers).
        /// So in a band we share all personal enemies.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool HasActorAsPersonalEnemy(Actor other)
        {
            if (other == null || other.IsDead)
                return false;

            // first check personal list.
            if (m_PersonalEnemies != null)
            {
                if (m_PersonalEnemies.Contains(other))
                    return true;
            }

            // check my leader and my mates, if any.
            if (this.HasLeader)
            {
                // my leader.
                if (m_Leader.HasDirectPersonalEnemy(other))
                    return true;
                // my mates = my leader followers.
                foreach (Actor mate in m_Leader.Followers)
                    if (mate != this && mate.HasDirectPersonalEnemy(other))
                        return true;
            }

            // check my followers, if any.
            if (this.CountFollowers > 0)
            {
                foreach (Actor fo in m_Followers)
                    if (fo.HasDirectPersonalEnemy(other))
                        return true;
            }

            // nope.
            return false;
        }

        /// <summary>
        /// Checks only personal enemy list, do not check our band.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        bool HasDirectPersonalEnemy(Actor other)
        {
            if (m_PersonalEnemies == null)
                return false;
            return m_PersonalEnemies.Contains(other);
        }

        public void MarkAsSelfDefence(Actor e)
        {
            if (e == null || e.IsDead)
                return;

            if (m_SelfDefence == null)
                m_SelfDefence = new List<Actor>(1);
            else if (m_SelfDefence.Contains(e))
                return;

            m_SelfDefence.Add(e);
        }

        public void RemoveAsSelfDefence(Actor e)
        {
            if (m_SelfDefence == null)
                return;

            m_SelfDefence.Remove(e);

            // minimize data size.
            if (m_SelfDefence.Count == 0)
                m_SelfDefence = null;
        }

        public void RemoveAllSelfDefence()
        {
            if (m_SelfDefence == null)
                return;

            while (m_SelfDefence.Count > 0)
            {
                Actor e = m_SelfDefence[0];
                e.RemoveAsSelfDefence(this);
                m_SelfDefence.Remove(e);
            }
        }

        public bool HasDirectSelfDefence(Actor other)
        {
            if (m_SelfDefence == null)
                return false;
            return m_SelfDefence.Contains(other);
        }
#endif
        #endregion

        // alpha10 made item centric: moved out of Actor to ItemEntertainment
#if false
        #region Boring items
        public void AddBoringItem(Item it)
        {
            if (m_BoringItems == null) m_BoringItems = new List<Item>(1);
            if (m_BoringItems.Contains(it)) return;
            m_BoringItems.Add(it);
        }

        public bool IsBoredOf(Item it)
        {
            if (m_BoringItems == null) return false;
            return m_BoringItems.Contains(it);
        }
        #endregion
#endif

        #region Equipment helpers
        public Item GetEquippedItem(DollPart part)
        {
            if (m_Inventory == null || part == DollPart.NONE)
                return null;

            foreach (Item it in m_Inventory.Items)
                if (it.EquippedPart == part)
                    return it;

            return null;
        }

        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public Item GetEquippedWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND);
        }

        // alpha10
        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public ItemMeleeWeapon GetEquippedMeleeWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND) as ItemMeleeWeapon;
        }

        // alpha10
        /// <summary>
        /// Assumed to be equiped at Right hand.
        /// </summary>
        /// <returns></returns>
        public ItemRangedWeapon GetEquippedRangedWeapon()
        {
            return GetEquippedItem(DollPart.RIGHT_HAND) as ItemRangedWeapon;
        }
        #endregion

        #region Flags helpers
        private bool GetFlag(Flags f) { return (m_Flags & f) != 0; }
        private void SetFlag(Flags f, bool value) { if (value) m_Flags |= f; else m_Flags &= ~f; }
        private void OneFlag(Flags f) { m_Flags |= f; }
        private void ZeroFlag(Flags f) { m_Flags &= ~f; }
        #endregion

        #region Pre-save
        public void OptimizeBeforeSaving()
        {
            // remove dead target.
            if (m_TargetActor != null && m_TargetActor.IsDead) m_TargetActor = null;

            // alpha10 moved out of Actor
            //// trim.
            //if (m_BoringItems != null) m_BoringItems.TrimExcess();

            // alpha10
            // remove trust entries with dead actors.
            // side effect: this means revived actor will forget their trust after a save game!
            if (m_TrustList != null)
            {
                for (int i = 0; i < m_TrustList.Count;)
                {
                    if (m_TrustList[i].Actor.IsDead)
                        m_TrustList.RemoveAt(i);
                    else
                        i++;
                }
                if (m_TrustList.Count == 0)
                    m_TrustList = null;
            }
            // remove & trim agressor/self-defence entries with dead actors.
            // side effect: this means revived actor will forget their agressor/self-defence after a save game!
            if (m_AggressorOf != null)
            {
                for (int i = 0; i < m_AggressorOf.Count; )
                {
                    if (m_AggressorOf[i].IsDead)
                        m_AggressorOf.RemoveAt(i);
                    else
                        i++;
                }
                if (m_AggressorOf.Count == 0)
                    m_AggressorOf = null;
                else
                    m_AggressorOf.TrimExcess();
            }
            if (m_SelfDefenceFrom != null)
            {
                for (int i = 0; i < m_SelfDefenceFrom.Count;)
                {
                    if (m_SelfDefenceFrom[i].IsDead)
                        m_SelfDefenceFrom.RemoveAt(i);
                    else
                        i++;
                }
                if (m_SelfDefenceFrom.Count == 0)
                    m_SelfDefenceFrom = null;
                else
                    m_SelfDefenceFrom.TrimExcess();
            }

            // alpha10 inventory
            if (m_Inventory != null)
                m_Inventory.OptimizeBeforeSaving();
        }
        #endregion
    }
}
