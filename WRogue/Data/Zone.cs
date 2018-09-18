using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Zone
    {
        #region Fields
        string m_Name = "unnamed zone";
        Rectangle m_Bounds;
        Dictionary<string, object> m_Attributes = null;
        #endregion

        #region Properties
        public string Name
        {
            get { return m_Name; }
            set { m_Name = value; }
        }

        public Rectangle Bounds
        {
            get { return m_Bounds; }
            set { m_Bounds = value; }
        }
        #endregion

        #region Init
        public Zone(string name, Rectangle bounds)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            m_Name = name;
            m_Bounds = bounds;
        }
        #endregion

        #region Game attributes
        public bool HasGameAttribute(string key)
        {
            if (m_Attributes == null)
                return false;
            return m_Attributes.Keys.Contains(key);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value">must be serializable</param>
        public void SetGameAttribute<_T_>(string key, _T_ value)
        {
            if (m_Attributes == null)
                m_Attributes = new Dictionary<string, object>(1);

            if (m_Attributes.Keys.Contains(key))
                m_Attributes[key] = value;
            else
                m_Attributes.Add(key, value);
        }

        public _T_ GetGameAttribute<_T_>(string key)
        {
            if (m_Attributes == null)
                return default(_T_);

            object value;
            if (m_Attributes.TryGetValue(key, out value))
            {
                if (!(value is _T_))
                    throw new InvalidOperationException("game attribute is not of requested type");
                return (_T_) value;                
            }
            else
                return default(_T_);
        }
        #endregion
    }
}
