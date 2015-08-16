using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class PersistentMessageSink : IHydrationAware {

        public int PersistentMessageSinkId { get; set; }

        public string DehydratedState { get; set; }

    }

}
