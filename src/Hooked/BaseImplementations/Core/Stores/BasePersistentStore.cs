﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;
using ComplexOmnibus.Hooked.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using AutoMapper;

namespace ComplexOmnibus.Hooked.BaseImplementations.Core.Stores {

    public class BasePersistentStore<TDataType, TObject> : IBaseStore<TObject>
        where TDataType : class, IIdentifiable, IHydrationAware
        where TObject : IIdentifiable {

        public BasePersistentStore(Func<NHookedContext, DbSet<TDataType>> set) {
            SetAccessor = set;
        }

        public IRequestResult Add(TObject obj) {
            Validate(obj);
            return this.ExecuteWithResult(() => new ContextHelper().InContext(ctx => {
                var target = SetAccessor(ctx).FirstOrDefault(o => o.UniqueId == obj.UniqueId);
                Assert.True(target == null, () => "An object with this id already exists: " + obj.UniqueId);
                var addee = Mapper.Map<TObject, TDataType>(obj);
                PreCommit(ctx, addee);
                SetAccessor(ctx).Add(addee); 
                ctx.SaveChanges(); 
            }));
        }

        public IRequestResult Remove(TObject obj) {
            Validate(obj);
            return this.ExecuteWithResult(() => { 
                new ContextHelper().InContext(ctx => {
                    var target = SetAccessor(ctx).First(o => o.UniqueId == obj.UniqueId);
                    if (target != null) {
                        SetAccessor(ctx).Remove(target);
                    ctx.SaveChanges();
                }});
            });
        }

        public IRequestResult Update(TObject obj) {
            Validate(obj);
            return this.ExecuteWithResult(() => {
                new ContextHelper().InContext(ctx => {
                    var target = Find(ctx, obj.UniqueId);
                    Assert.False(target == null, () => "No object with id " + obj.UniqueId);
                    PreCommit(ctx, target);
                    Mapper.Map<TObject, TDataType>(obj, target);
                    ctx.SaveChanges();
                });
            });
        }

        public IRequestResult<TObject> FindById(string id) {
            Assert.True(id != null, () => "Cannot use a null id to find an object");
            return new ContextHelper().InContext(ctx => {
                var obj = Find(ctx, id);
                return RequestResult<TObject>.Create(MapFromPersistentForm(obj), obj != null);
            });
        }

        private TDataType Find(NHookedContext ctx, string id) {
            var includes = LoadingIncludes();
            IQueryable<TDataType> query = SetAccessor(ctx);
            query = includes.Aggregate(query, (q, s) => q.Include(s));
            return query.FirstOrDefault(c => c.UniqueId == id);
        }

        public IEnumerable<TObject> All {
            get {
                return new ContextHelper().InContext(ctx => SetAccessor(ctx).Select(MapFromPersistentForm).ToArray()); 
            }
        }

        public int Count() {
            return new ContextHelper().InContext(ctx => SetAccessor(ctx).Count());
        }

        protected TObject MapFromPersistentForm(TDataType src) {
            return src == default(TDataType) ? default(TObject) : Mapper.Map<TDataType, TObject>(src);
        }

        // Get the dehydrated state from the persistent object, instantiate object using type, rehydrate
        protected TObject InstantiateDomainObject(TDataType src) {
            return default(TObject);
        }

        protected virtual IEnumerable<string> LoadingIncludes() {
            return Enumerable.Empty<string>();
        }

        protected virtual void PreCommit(NHookedContext ctx, TDataType obj) { 
        }

        protected virtual void Validate(TObject obj) { 
            Assert.True(obj != null, () => "Cannot process a null object");
            Assert.True(!String.IsNullOrWhiteSpace(obj.UniqueId), () => "Object must have an id");
        }

        private Func<NHookedContext, DbSet<TDataType>> SetAccessor { get; set; }

    }
}
