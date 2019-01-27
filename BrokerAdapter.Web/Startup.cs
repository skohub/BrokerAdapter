using System;
using System.Threading;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CloudFactoryTask.Advanced.Domain;
using CloudFactoryTask.Advanced.Domain.RequestProcessors;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CloudFactoryTask.Advanced.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddAutofac();
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(services);
            var container = ConfigureContainer(containerBuilder);
            return new AutofacServiceProvider(container);
        }

        public IContainer ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<ProcessingModule>();

            return builder.Build();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Use(async (context, next) =>
            {
                var requestProcessor = context.RequestServices.GetService<IRequestProcessor>();
                var result = await requestProcessor.Process(new BrokerRequest
                {
                    Method = context.Request.Method,
                    Path = context.Request.Path
                }, context.RequestAborted);

                if (result != null)
                {
                    context.Response.StatusCode = result.HttpCode;
                    await context.Response.WriteAsync(result.Content);
                }
                else
                {
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Error");
                }
            });
        }
    }
}
