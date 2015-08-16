using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class PersistentSubscription : IIdentifiable, IHydrationAware {

        public int PersistentSubscriptionId { get; set; }

        public string DehydratedState { get; set; }

        public virtual PersistentTopic Topic { get; set; }

        public string ChannelMonicker { get; set; }

        public virtual PersistentQualityAttributes Qualities { get; set; }

        public virtual PersistentMessageSink TargetSink { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string UniqueId { get; set; }
    }

}
