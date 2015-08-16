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

    public class PersistentTopicStore : BasePersistentStore<PersistentTopic, ITopic>, ITopicStore {

        public PersistentTopicStore() : base(ctx => ctx.Topics) {
        }

    }
}
