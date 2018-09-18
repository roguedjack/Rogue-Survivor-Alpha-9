using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    class Verb
    {
        #region Properties
        public string YouForm { get; protected set; }
        public string HeForm { get; protected set; }
        #endregion

        #region Init
        public Verb(string youForm, string heForm)
        {
            this.YouForm = youForm;
            this.HeForm = heForm;
        }

        public Verb(string youForm) : this(youForm, youForm+"s")
        {
        }
        #endregion
    }
}
