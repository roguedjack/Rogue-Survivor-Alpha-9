﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace djack.RogueSurvivor.Data
{
    abstract class FactionDB
    {
        public abstract Faction this[int id]
        {
            get;
        }
    }
}
