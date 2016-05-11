using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class ActorSheet
    {
        #region Fields
        SkillTable m_SkillTable = new SkillTable();
        #endregion

        #region Blank sheet
        [NonSerialized]
        public static readonly ActorSheet BLANK = new ActorSheet(0, 0, 0, 0, 0, Attack.BLANK, Defence.BLANK, 0, 0, 0, 0);
        #endregion

        #region Properties
        public int BaseHitPoints { get; private set; }
        public int BaseStaminaPoints { get; private set; }
        public int BaseFoodPoints { get; private set; }
        public int BaseSleepPoints { get; private set; }
        public int BaseSanity { get; private set; }
        public Attack UnarmedAttack { get; private set; }
        public Defence BaseDefence { get; private set; }
        public int BaseViewRange { get; private set; }
        public int BaseAudioRange { get; private set; }
        public float BaseSmellRating { get; private set; }
        public int BaseInventoryCapacity { get; private set; }

        public SkillTable SkillTable
        {
            get { return m_SkillTable; }
            set { m_SkillTable = value; }
        }
        #endregion

        #region Init
        public ActorSheet(int baseHitPoints, int baseStaminaPoints, 
            int baseFoodPoints, int baseSleepPoints, int baseSanity,
            Attack unarmedAttack, Defence baseDefence,
            int baseViewRange, int baseAudioRange, int smellRating,
            int inventoryCapacity)
        {
            this.BaseHitPoints = baseHitPoints;
            this.BaseStaminaPoints = baseStaminaPoints;
            this.BaseFoodPoints = baseFoodPoints;
            this.BaseSleepPoints = baseSleepPoints;
            this.BaseSanity = baseSanity;
            this.UnarmedAttack = unarmedAttack;
            this.BaseDefence = baseDefence;
            this.BaseViewRange = baseViewRange;
            this.BaseAudioRange = baseAudioRange;
            this.BaseSmellRating = smellRating / 100.0f;
            this.BaseInventoryCapacity = inventoryCapacity;
        }

        public ActorSheet(ActorSheet copyFrom)
        {
            if (copyFrom == null)
                throw new ArgumentNullException("copyFrom");

            this.BaseHitPoints = copyFrom.BaseHitPoints;
            this.BaseStaminaPoints = copyFrom.BaseStaminaPoints;
            this.BaseFoodPoints = copyFrom.BaseFoodPoints;
            this.BaseSleepPoints = copyFrom.BaseSleepPoints;
            this.BaseSanity = copyFrom.BaseSanity;
            this.UnarmedAttack = copyFrom.UnarmedAttack;
            this.BaseDefence = copyFrom.BaseDefence;
            this.BaseViewRange = copyFrom.BaseViewRange;
            this.BaseAudioRange = copyFrom.BaseAudioRange;
            this.BaseSmellRating = copyFrom.BaseSmellRating;
            this.BaseInventoryCapacity = copyFrom.BaseInventoryCapacity;

            if (copyFrom.SkillTable.Skills != null)
                m_SkillTable = new SkillTable(copyFrom.SkillTable.Skills);
        }
        #endregion

    }
}
