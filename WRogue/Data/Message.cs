using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    class Message
    {
        #region Fields
        string m_Text;
        Color m_Color;
        readonly int m_Turn;
        #endregion

        #region Properties
        public string Text
        {
            get { return m_Text; }
            set { m_Text = value; }
        }

        public Color Color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }

        public int Turn
        {
            get { return m_Turn; }
        }
        #endregion

        #region Init
        public Message(string text, int turn, Color color)
        {
            if (text == null)
                throw new ArgumentNullException("text");

            m_Text = text;
            m_Color = color;
            m_Turn = turn;
        }

        public Message(string text, int turn)
            : this(text, turn, Color.White)
        {
        }
        #endregion
    }
}
