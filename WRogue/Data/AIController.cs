using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace djack.RogueSurvivor.Data
{
    [Serializable]
    abstract class AIController : ActorController
    {
        public abstract ActorOrder Order { get; }
        public abstract ActorDirective Directives { get; set; }

        public abstract void SetOrder(ActorOrder newOrder);
    }
}
