using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine
{
    class MessageManager
    {
        #region Fields
        readonly List<Message> m_Messages = new List<Message>();
        int m_LinesSpacing;
        int m_FadeoutFactor;
        readonly List<Message> m_History;
        int m_HistorySize;
        #endregion

        #region Properties
        public int Count
        {
            get { return m_Messages.Count; }
        }

        public IEnumerable<Message> History
        {
            get { return m_History; }
        }
        #endregion

        #region Init
        public MessageManager(int linesSpacing, int fadeoutFactor, int historySize)
        {
            if (linesSpacing < 0)
                throw new ArgumentOutOfRangeException("linesSpacing < 0");
            if (fadeoutFactor < 0)
                throw new ArgumentOutOfRangeException("fadeoutFactor < 0");

            m_LinesSpacing = linesSpacing;
            m_FadeoutFactor = fadeoutFactor;
            m_HistorySize = historySize;
            m_History = new List<Message>(historySize);
        }
        #endregion

        #region Managing messages
        public void Clear()
        {
            m_Messages.Clear();
        }

        public void ClearHistory()
        {
            m_History.Clear();
        }

        public void Add(Message msg)
        {
            m_Messages.Add(msg);
            m_History.Add(msg);
            if (m_History.Count > m_HistorySize)
            {
                m_History.RemoveAt(0);
            }
        }

        public void RemoveLastMessage()
        {
            if (m_Messages.Count == 0)
                return;
            m_Messages.RemoveAt(m_Messages.Count - 1);
        }
        #endregion

        #region Drawing
        public void Draw(IRogueUI ui, int freshMessagesTurn, int gx, int gy)
        {
            for(int i = 0; i < m_Messages.Count; i++)
            {
                Message msg = m_Messages[i];

                int alpha = Math.Max(64, 255 - m_FadeoutFactor * (m_Messages.Count - 1 - i));
                bool isLatest = (m_Messages[i].Turn >= freshMessagesTurn);
                Color dimmedColor = Color.FromArgb(alpha, msg.Color);

                if(isLatest)
                    ui.UI_DrawStringBold(dimmedColor, msg.Text, gx, gy);
                else
                    ui.UI_DrawString(dimmedColor, msg.Text, gx, gy);

                gy += m_LinesSpacing;
            }
        }
        #endregion
    }
}
