using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;

namespace ComplexOmnibus.Hooked.BaseImplementations.Core {

    public class ContextHelper {

        public void InContext(Action<NHookedContext> f) {
            using (NHookedContext ctx = new NHookedContext()) {
                f(ctx);
            }
        }

        public T InContext<T>(Func<NHookedContext, T> f) {
            T result = default(T);
            using (NHookedContext ctx = new NHookedContext()) {
                result = f(ctx);
            }
            return result;
        }
    }
}
