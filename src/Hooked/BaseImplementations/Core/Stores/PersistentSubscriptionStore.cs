using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;
using ComplexOmnibus.Hooked.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using AutoMapper;

namespace ComplexOmnibus.Hooked.BaseImplementations.Core.Stores {

    public class PersistentSubscriptionStore : BasePersistentStore<PersistentSubscription, ISubscription>, ISubscriptionStore {

        public PersistentSubscriptionStore() : base(ctx => ctx.Subscriptions) {
        }

        public IEnumerable<ISubscription> SubscriptionsForTopic(ITopic topic) {
            return new ContextHelper().InContext(ctx => {
                var target = ctx.Topics.FirstOrDefault(t => t.UniqueId == topic.UniqueId);
                Assert.False(target == null, () => "Not topic with id " + topic.UniqueId);
                return target.Subscriptions.Select(MapFromPersistentForm); 
            }); 
        }

        public IRequestResult RemoveSubscriptionsForTopic(ITopic topic) {
            throw new NotImplementedException("Cannot remove subscriptions for this topic");
        }

    }
}
