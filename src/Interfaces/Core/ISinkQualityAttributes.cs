using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexOmnibus.Hooked.Interfaces.Core {
    
    public interface ISinkQualityAttributes {
        uint? RequestTimeout { get; set; }
    }
}
