using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ComplexOmnibus.Hooked.Interfaces.Core;
using ComplexOmnibus.Hooked.Interfaces.Infra;
using ComplexOmnibus.Hooked.BaseImplementations.Infra;
using ComplexOmnibus.Hooked.Infra.Extensions;
using ComplexOmnibus.Hooked.EntityFrameworkIntegration;
using AutoMapper;
using ComplexOmnibus.Hooked.BaseImplementations.Core;
using ComplexOmnibus.Hooked.BaseImplementations.Core.Sinks;

namespace ComplexOmnibus.Hooked.Mapping
{
    public static class MappingInitializer {

        public static void Execute() {

            // To EF

            Mapper
                .CreateMap<ISubscription, PersistentSubscription>()
                .ForMember(dest => dest.PersistentSubscriptionId, opt => opt.Ignore())
                .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => Dehydrate(src)))
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => Mapper.Map<ITopic, PersistentTopic>(src.Topic)))
                .ForMember(dest => dest.Qualities, opt => opt.MapFrom(src => Mapper.Map<IQualityAttributes, PersistentQualityAttributes>(src.QualityConstraints)))
                .ForMember(dest => dest.TargetSink, opt => opt.MapFrom(src => Mapper.Map<IMessageSink, PersistentMessageSink>(src.Sink)));

            Mapper
                .CreateMap<ITopic, PersistentTopic>()
                .ForMember(dest => dest.PersistentTopicId, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
                .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => src.Ancillary));

            Mapper
               .CreateMap<IMessageSink, PersistentMessageSink>()
               .ForMember(dest => dest.PersistentMessageSinkId, opt => opt.Ignore())
               .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => Dehydrate(src)));

            Mapper
               .CreateMap<IQualityAttributes, PersistentQualityAttributes>()
               .ForMember(dest => dest.PersistentQualityAttributesId, opt => opt.Ignore());

            Mapper.CreateMap<ISinkQualityAttributes, PersistentSinkQualityAttributes>()
               .ForMember(dest => dest.PersistentSinkQualityAttributesId, opt => opt.Ignore());

            // From EF

            Mapper
                .CreateMap<PersistentSubscription, ISubscription>()
                .ConstructUsing((Func<PersistentSubscription, ISubscription>) (ps => Hydrate<ISubscription, Subscription>(ps)))
                .ForMember(dest => dest.Ancillary, opt => opt.Ignore())
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => Mapper.Map<PersistentTopic, ITopic>(src.Topic)))
                .ForMember(dest => dest.QualityConstraints, opt => opt.MapFrom(src => Mapper.Map<PersistentQualityAttributes, IQualityAttributes>(src.Qualities)))
                .ForMember(dest => dest.Sink, opt => opt.MapFrom(src => Mapper.Map<PersistentMessageSink, IMessageSink>(src.TargetSink))); 

            Mapper
                .CreateMap<PersistentTopic, ITopic>()
                .ConstructUsing((Func<PersistentTopic, ITopic>)(ps => Hydrate<ITopic, Topic>(ps)))
                .ForMember(dest => dest.Ancillary, opt => opt.Ignore());

            Mapper
                .CreateMap<PersistentMessageSink, IMessageSink>()
                .ConstructUsing((Func<PersistentMessageSink, IMessageSink>)(ps => Hydrate<IMessageSink, ConsoleMessageSink>(ps)));

            Mapper
                .CreateMap<PersistentQualityAttributes, IQualityAttributes>()
                .ConstructUsing((Func<PersistentQualityAttributes, IQualityAttributes>)(ps => new QualityAttributes()));

            Mapper
                .CreateMap<PersistentSinkQualityAttributes, ISinkQualityAttributes>()
                .ConstructUsing((Func<PersistentSinkQualityAttributes, ISinkQualityAttributes>)(ps => new SinkQualityAttributes()));

            Mapper.AssertConfigurationIsValid();
        }

        //private static TObject Hydrate<TObject>() { 
        //    IHydratableDependent

        //}

        private static string Dehydrate(object src) {
            string result = null;
            if (src is IHydratableDependent) {
                result = ((IHydratableDependent) src).Dehydrate().Containee.Serialize().ToString();
            }
            return result;
        }

        private static TType Hydrate<TType, TDefaultType>(IHydrationAware src) {
            TType result = default(TType);
            if (src.DehydratedState.IsNull())
                result = (TType) Activator.CreateInstance(typeof(TDefaultType));
            else {
                HydrationObject obj = src.DehydratedState.Deserialize<HydrationObject>();
                if (obj.OriginType != null) {
                    var interim = Activator.CreateInstance(obj.OriginType) as IHydratableDependent;
                    interim.Hydrate(obj);
                    result = (TType)interim;
                }
            }
            return result;
        }

    }

}
