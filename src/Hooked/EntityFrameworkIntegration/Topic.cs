using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class PersistentTopic : IIdentifiable, IHydrationAware {

        public int PersistentTopicId { get; set; }

        public string DehydratedState { get; set; }
        
        public virtual ICollection<PersistentSubscription> Subscriptions { get; set; }

        public bool Deprecated { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UniqueId { get; set; }
    }
}
