using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    abstract class ItemModelDB
    {
        public abstract ItemModel this[int id]
        {
            get;
        }
    }
}
