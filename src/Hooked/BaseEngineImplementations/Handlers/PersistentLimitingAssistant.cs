using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Engine;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using AutoMapper;

namespace ComplexOmnibus.Hooked.BaseEngineImplementations.Engine {
    
    /// <summary>
    /// This is a 'hard' assistant; it keeps nothing in memory, and is thus slower than an in memory solution.
    /// TODO: Automapper maps
    /// </summary>
    public class PersistentLimitingAssistant : IResourceLimitingAssistant {

        public void Initializing() {
        }

        public IRequestResult Accept(IProcessableUnit bundle) {
            Prototype = bundle;
            MessagesPending++;
            return this.ExecuteWithResult(() => {
                new ContextHelper().InContext(ctx => {
                    var unit = new PersistentUnit { 
                        DateCreated = DateTime.Now,
                        Subscription = ctx.Subscriptions.First(s => s.UniqueId == bundle.Subscription.UniqueId),
                        DehydratedMessage = bundle.Message.Serialize().ToString()
                    };
                    ctx.PersistentUnits.Add(unit);
                    ctx.SaveChanges();
                });
            });
        }

        public bool Any() {
            return MessagesPending > 0;
        }

        public IProcessableUnit NextUnit() {
            return new ContextHelper()
                        .InContext(ctx => {
                            var next = GetUnit(ctx);
                            var result = default(IProcessableUnit);
                            if (next.IsNotNull()) { 
                                // TODO: Temporary cheat
                                var subs = ctx.Subscriptions.Include("Qualities.SinkQuality").First(s => s.UniqueId == next.Subscription.UniqueId);
                                result = new ProcessableUnit { 
                                    Subscription = Mapper.Map<PersistentSubscription, ISubscription>(subs),
                                    Message = next.DehydratedMessage.Deserialize<IMessage>()
                                };
                            }
                            return result;
                        });
        }

        public void Remove(IProcessableUnit unit, bool delivered) {
            new ContextHelper()
                        .InContext(ctx => {
                            var next = GetUnit(ctx);
                            next.IsNotNull(() => {
                                ctx.PersistentUnits.Remove(next);
                                ctx.SaveChanges();
                            });
                        });
            MessagesPending = Math.Max(0, MessagesPending - 1);
        }

        public IRequestResult Hydrate(IHydrationObject memento) {
            MessagesPending = memento.IsNull() || memento.State.IsNull() ? 0 : int.Parse(memento.State);
            return RequestResult.Create();
        }

        public IRequestResult<IHydrationObject> Dehydrate() {
            return RequestResult<IHydrationObject>.Create(new HydrationObject(GetType(), MessagesPending.ToString()));
        }

        private IProcessableUnit Prototype { get; set; }

        private int MessagesPending { get; set; }

        private PersistentUnit GetUnit(NHookedContext ctx) {
            // The order by looks superfluous but is not; changes between 2008 and 2012 mean that ordering reverts to what was originally intended for RDBMS - no guaranteed ordering
            return ctx.PersistentUnits.Where(u => u.Subscription.UniqueId == Prototype.Subscription.UniqueId).OrderBy(u => u.PersistentUnitId).FirstOrDefault();
        }

    }

}
