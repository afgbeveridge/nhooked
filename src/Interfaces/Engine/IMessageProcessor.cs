using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;

namespace ComplexOmnibus.Hooked.Interfaces.Engine {

    public interface IMessageProcessor : IHydratable {
        IRequestResult Next();
        IRequestResult Terminate();
    }

}
