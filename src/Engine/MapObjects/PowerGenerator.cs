using System;
using System.Collections.Generic;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.MapObjects
{
    [Serializable]
    class PowerGenerator : StateMapObject
    {
        #region Power Generator states
        public const int STATE_OFF = 0;
        public const int STATE_ON = 1;
        #endregion

        #region Fields
        string m_OffImageID;
        string m_OnImageID;
        #endregion

        #region Properties
        public bool IsOn
        {
            get { return this.State == STATE_ON; }
        }
        #endregion

        #region Init
        public PowerGenerator(string name, string offImageID, string onImageID)
            : base(name, offImageID)
        {
            m_OffImageID = offImageID;
            m_OnImageID = onImageID;
        }
        #endregion

        #region StateMapObject
        public override void SetState(int newState)
        {
            base.SetState(newState);

            switch (newState)
            {
                case STATE_OFF:
                    this.ImageID = m_OffImageID;
                    break;

                case STATE_ON:
                    this.ImageID = m_OnImageID;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("unhandled state");
            }
        }
        #endregion

        #region Switching On/Off
        public void TogglePower()
        {
            SetState(this.State == STATE_OFF ? STATE_ON : STATE_OFF);
        }
        #endregion
    }
}
