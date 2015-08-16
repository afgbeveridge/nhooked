using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class NHookedContext : DbContext {

        public NHookedContext() : base("NHookedContext") {
        }

        public DbSet<PersistentTopic> Topics { get; set; }
        public DbSet<PersistentSubscription> Subscriptions { get; set; }
        public DbSet<PersistentMessageSink> MessageSinks { get; set; }
        public DbSet<PersistentQualityAttributes> QualityAttributes { get; set; }
        public DbSet<PersistentSinkQualityAttributes> SinkQualityAttributes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
