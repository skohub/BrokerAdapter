using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Core;
using CloudFactoryTask.Advanced.Domain.BrokerAdapters;
using CloudFactoryTask.Advanced.Domain.KeyGenerators;
using CloudFactoryTask.Advanced.Domain.RequestProcessors;
using CloudFactoryTask.Advanced.Infrastructure;
using Microsoft.Extensions.Configuration;
using Module = Autofac.Module;

namespace CloudFactoryTask.Advanced.Web
{
    public class ProcessingModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleLogger>().As<ILogger>();
            builder.RegisterType<RequestKeyGenerator>().As<IRequestKeyGenerator>();
            builder.RegisterType<RequestKeyGenerator>().As<IWaitListKeyGenerator>();

            builder.RegisterType<RequestProcessor>().Named<IRequestProcessor>("underlying");
            builder.RegisterDecorator<IRequestProcessor>(
                (ctx, inner) => new BufferedRequestProcessor(inner, ctx.Resolve<IWaitListKeyGenerator>(), ctx.Resolve<ILogger>()), 
                fromKey: "underlying");

            builder.RegisterType<FileSystemBrokerAdapter>().As<IBrokerAdapter>()
                .WithParameter(
                    new ResolvedParameter(
                        (pi, ctx) => pi.Name == "brokerPath",
                        (pi, ctx) =>
                        {
                            var configuration = ctx.Resolve<IConfiguration>();
                            var brokerConfig = configuration.GetSection(BrokerConfig.SectionName).Get<BrokerConfig>();
                            var pwd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                            return Path.Combine(pwd, brokerConfig.Path);
                        }));
        }
    }
}
