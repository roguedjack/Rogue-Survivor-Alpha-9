using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using djack.RogueSurvivor.Data;

namespace djack.RogueSurvivor.Engine.Tasks
{
    [Serializable]
    class TaskRemoveDecoration : TimedTask
    {
        private int m_X, m_Y;
        private string m_imageID;

        public TaskRemoveDecoration(int turns, int x, int y, string imageID)
            : base(turns)
        {
            m_X = x;
            m_Y = y;
            m_imageID = imageID;
        }

        public override void Trigger(Map m)
        {
            m.GetTileAt(m_X, m_Y).RemoveDecoration(m_imageID);
        }
    }
}
