using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Infra.Extensions;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class PersistentQualityAttributes {

        public int PersistentQualityAttributesId { get; set; }

        public bool GuaranteeOrder { get; set; }

        public bool GuaranteeDelivery { get; set; }

        public int? TTL { get; set; }

        public int? BackOffPeriod { get; set; }

        public int? MultiThreadingLimit { get; set; }

        public int MaxRetry { get; set; }

        public int? EndureQuietude { get; set; }

        public PersistentSinkQualityAttributes SinkQuality { get; set; }
    }
}
