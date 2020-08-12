using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Common
{
    public static class AppExtensions
    {
        public static IServiceCollection AddConsulConfig(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = configuration.GetValue<string>("Consul:Host");
                consulConfig.Address = new Uri(address);
            }));
            return services;
        }

        public static IApplicationBuilder UseConsul(this IApplicationBuilder app)
        {
            var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger("AppExtensions");
            var lifetime = app.ApplicationServices.GetRequiredService<IApplicationLifetime>();

            if (!(app.Properties["server.Features"] is FeatureCollection features)) return app;

            var addresses = features.Get<IServerAddressesFeature>();
            var addressHttps = addresses.Addresses.Where(x=>x.Contains("https")).First();
            var addressHttp = addresses.Addresses.Where(x => !x.Contains("https")).First();

            Console.WriteLine($"address={addressHttps}");

            var uri = new Uri(addressHttps);
            var registration = new AgentServiceRegistration()
            {
                ID = $"MyService-{uri.Port}",
                // servie name  
                Name = "MyService",
                Address = $"{uri.Host}",
                Port = uri.Port,
            };
            registration.Check = new AgentServiceCheck()
            {
                //服务停止多久 后反注册(注销) 
                DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(5),
                //健康检查时间间隔，或者称为心跳 间隔 
                Interval = TimeSpan.FromSeconds(10),
                //健康检查地址 
                HTTP = $"{addressHttp}/health",
                Timeout = TimeSpan.FromSeconds(5)
            };

            logger.LogInformation("Registering with Consul");
            consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            consulClient.Agent.ServiceRegister(registration).ConfigureAwait(true);

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogInformation("Unregistering from Consul");
                consulClient.Agent.ServiceDeregister(registration.ID).ConfigureAwait(true);
            });

            return app;
        }
    }
}
