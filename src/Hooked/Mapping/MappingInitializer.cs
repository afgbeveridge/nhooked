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

            #region Core objects
            // To EF

            Mapper.CreateMap<ISinkQualityAttributes, PersistentSinkQualityAttributes>()
               .ForMember(dest => dest.PersistentSinkQualityAttributesId, opt => opt.Ignore());

            Mapper
               .CreateMap<IQualityAttributes, PersistentQualityAttributes>()
               .ForMember(dest => dest.PersistentQualityAttributesId, opt => opt.Ignore())
               .ForMember(dest => dest.SinkQuality, opt => opt.MapFrom(src => src.SinkQuality));

            Mapper
                .CreateMap<ISubscription, PersistentSubscription>()
                .ForMember(dest => dest.PersistentSubscriptionId, opt => opt.Ignore())
                .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => Dehydrate(src)))
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.Topic))
                .ForMember(dest => dest.Qualities, opt => opt.MapFrom(src => src.QualityConstraints))
                .ForMember(dest => dest.TargetSink, opt => opt.MapFrom(src => src.Sink));

            Mapper
                .CreateMap<ITopic, PersistentTopic>()
                .ForMember(dest => dest.PersistentTopicId, opt => opt.Ignore())
                .ForMember(dest => dest.Subscriptions, opt => opt.Ignore())
                .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => src.Ancillary));

            Mapper
               .CreateMap<IMessageSink, PersistentMessageSink>()
               .ForMember(dest => dest.PersistentMessageSinkId, opt => opt.Ignore())
               .ForMember(dest => dest.DehydratedState, opt => opt.MapFrom(src => Dehydrate(src)));

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

            #endregion

            #region Ancillary objects

            Mapper
                .CreateMap<IProcessableUnit, AuditedMessage>()
                .ForMember(dest => dest.AuditedMessageId, opt => opt.Ignore())
                .ForMember(dest => dest.Topic, opt => opt.MapFrom(src => src.Subscription.Topic.UniqueId))
                .ForMember(dest => dest.ChannelMonicker, opt => opt.MapFrom(src => src.Subscription.ChannelMonicker))
                .ForMember(dest => dest.DehydratedUnit, opt => opt.MapFrom(src => src.Serialize().ToString()));

            #endregion

            Mapper.AssertConfigurationIsValid();
        }

        private static string Dehydrate(object src) {
            string result = null;
            if (src is IHydratableDependent) 
                result = ((IHydratableDependent) src).Dehydrate().Containee.Serialize().ToString();
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
