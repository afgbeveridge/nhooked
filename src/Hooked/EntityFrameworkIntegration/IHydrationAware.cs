using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {
    
    public interface IHydrationAware {
        string DehydratedState { get; set; }
    }
}
