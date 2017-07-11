using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
//using MySQL.Data.EntityFrameworkCore.Extensions;
using MenuCatalog.Models;
using Steeltoe.Discovery.Client;
using Steeltoe.Extensions.Configuration;
using Criteo.Profiling.Tracing;
using Criteo.Profiling.Tracing.Middleware;
using Criteo.Profiling.Tracing.Transport.Http;
using Criteo.Profiling.Tracing.Tracers.Zipkin;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace MenuCatalog
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .AddConfigServer(env);
            

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc();

            services.AddDiscoveryClient(Configuration);

            //This code for using InMemory database
            //services.AddEntityFramework()
            //       .AddDbContext<MenuContext>(options => options.UseInMemoryDatabase());

            //This code for using Pomelo.EntityFrameworkCore.MySql package.
            services.AddDbContext<MenuContext>(options => options.UseMySql(Configuration["MySQL:ConnectionString"]));
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

			var lifetime = app.ApplicationServices.GetService<IApplicationLifetime>();
			lifetime.ApplicationStarted.Register(() => {
				TraceManager.SamplingRate = 1.0f;
				var logger = new TracingLogger(loggerFactory, "Criteo.Profiling.Tracing");
				var httpSender = new HttpZipkinSender(Configuration["zipkin:url"], "application/json");
				var tracer = new ZipkinTracer(httpSender, new JSONSpanSerializer());
				TraceManager.RegisterTracer(tracer);
				TraceManager.Start(logger);

			});
			lifetime.ApplicationStopped.Register(() =>
			{
				TraceManager.Stop();
			});
			app.UseTracing(Configuration["spring:application:name"]);

			var tokenValidationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				ValidateIssuer = true,
				ValidIssuer = "http://localhost:5000",
				IssuerSigningKey = new X509SecurityKey(new X509Certificate2(Path.Combine(".", "Certs", "IdentityServer4Auth.cer"))),
			};


			app.UseJwtBearerAuthentication(
				new JwtBearerOptions()
				{
					SaveToken = true,
					RequireHttpsMetadata = false,
					Audience = "menu_system_APIs",
					TokenValidationParameters = tokenValidationParameters
				}
				);

			app.UseMvc();
            app.UseDiscoveryClient();
            SampleData.InitializeMenuDatabase(app.ApplicationServices).Wait();

        }
    }
}
