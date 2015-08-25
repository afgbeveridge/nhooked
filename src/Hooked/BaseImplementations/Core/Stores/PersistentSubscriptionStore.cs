#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
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

        public ISubscription FindByMonicker(string nickname) {
            return new ContextHelper().InContext(ctx => {
                var obj = Find(ctx, _ => _.ChannelMonicker == nickname);
                return obj.IsNull() ? null : MapFromPersistentForm(obj);
            });
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
