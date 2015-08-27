using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {
    
    public class PersistentUnit : DatedObject {

        public int PersistentUnitId { get; set; }

        public virtual PersistentSubscription Subscription { get; set; }

        public string DehydratedMessage { get; set; }

    }

}
