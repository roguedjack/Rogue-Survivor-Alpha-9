using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    abstract class ActorModelDB
    {
        public abstract ActorModel this[int id]
        {
            get;
        }
    }
}
