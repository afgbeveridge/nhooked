using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Ancillary;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.MessageSources {
    
    public abstract class BaseMessageSource : IMessageSource {

        protected BaseMessageSource(IContentParser parser, IConfigurationSource cfg, ILogger logger) {
            Parser = parser;
            Configuration = cfg;
            Logger = logger;
        }

        public abstract IRequestResult<IMessage> Retrieve { get; }
        
        public abstract IRequestResult<IMessage> Peek { get; }

        public abstract bool CanPeek { get; }

        public abstract IRequestResult Initialize();

        public abstract IRequestResult UnInitialize();

        public virtual IRequestResult Hydrate(IHydrationObject obj) {
            return RequestResult.Create();
        }

        public IRequestResult<IHydrationObject> Dehydrate() {
            return RequestResult<IHydrationObject>.Create(new HydrationObject(GetType(), null) { ServiceInterface = typeof(IMessageSource) });
        }

        protected IContentParser Parser { get; private set; }

        protected ILogger Logger { get; private set; }

        protected IConfigurationSource Configuration { get; private set; }

    }
}
