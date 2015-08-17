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

        protected override IEnumerable<string> LoadingIncludes() {
            return new [] { "Qualities.SinkQuality" };
        }

        protected override void PreCommit(NHookedContext ctx, PersistentSubscription obj) {
            var topic = ctx.Topics.FirstOrDefault(t => t.UniqueId == obj.Topic.UniqueId);
            topic.IsNotNull(() => obj.Topic = topic);
        }

        protected override void Validate(ISubscription obj) {
            base.Validate(obj);
            Assert.True(obj.Topic != null, () => "Cannot process a null topic");
            Assert.True(!String.IsNullOrWhiteSpace(obj.Topic.UniqueId), () => "Topic must have an id");
            Assert.True(obj.Sink != null, () => "Cannot process a null sink");
            Assert.True(obj.QualityConstraints != null, () => "Cannot process null quality attributes");
        }

    }
}
