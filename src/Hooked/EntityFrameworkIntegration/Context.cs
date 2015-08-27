#region License
/*
Copyright (c) 2015 Tony Beveridge

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without 
restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to 
whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE 
AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace ComplexOmnibus.Hooked.EntityFrameworkIntegration {

    public class NHookedContext : DbContext {

        private const int MaxIdentifierLength = 64;
        private const int MaxDescriptionLength = 1024;

        public NHookedContext() : base("NHookedContext") {
        }

        public DbSet<PersistentTopic> Topics { get; set; }
        public DbSet<PersistentSubscription> Subscriptions { get; set; }
        public DbSet<PersistentMessageSink> MessageSinks { get; set; }
        public DbSet<PersistentQualityAttributes> QualityAttributes { get; set; }
        public DbSet<PersistentSinkQualityAttributes> SinkQualityAttributes { get; set; }
        public DbSet<PersistentState> PersistedStates { get; set; }
        public DbSet<AuditedMessage> Audit { get; set; }
        public DbSet<PersistentUnit> PersistentUnits { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            // Idents
            SetMaxLength<PersistentTopic>(modelBuilder, t => t.UniqueId);
            SetMaxLength<PersistentSubscription>(modelBuilder, t => t.UniqueId);
            SetMaxLength<PersistentState>(modelBuilder, t => t.ContainerId);
            // Names and descriptions
            SetMaxLength<PersistentTopic>(modelBuilder, t => t.Name);
            SetMaxLength<PersistentSubscription>(modelBuilder, t => t.Name);
            SetMaxLength<PersistentTopic>(modelBuilder, t => t.Description, MaxDescriptionLength);
            SetMaxLength<PersistentSubscription>(modelBuilder, t => t.Description, MaxDescriptionLength);
        }

        private void SetMaxLength<TEntity>(DbModelBuilder modelBuilder, Expression<Func<TEntity, string>> f, int limit = MaxIdentifierLength) where TEntity : class { 
            modelBuilder
                .Entity<TEntity>()
                .Property(f)
                .HasMaxLength(limit)
                .IsRequired();
        }
    }

}
