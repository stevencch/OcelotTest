using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Consul;

namespace OcelotTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            var authenticationProviderKey = "TestKey";
            //Action<IdentityServerAuthenticationOptions> options = o =>
            //{
            //    o.Authority = "https://localhost:5001";
            //    o.ApiName = "client";
            //    o.SupportedTokens = SupportedTokens.Both;
            //    o.SaveToken = true;
            //    o.ApiSecret = "secret";
            //};

            //services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
            //    .AddIdentityServerAuthentication(authenticationProviderKey, options);

            services.AddAuthentication()
        .AddJwtBearer(authenticationProviderKey, x =>
        {
            x.Authority = Configuration["IdentityServer"];
            x.Audience = "api1";
            x.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false
            };
        });


            services.AddOcelot()
                .AddConsul();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            
            app.UseRouting();
            app.UseAuthentication();
            

            app.MapWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
            {
                appBuilder.UseOcelot().Wait();
            });

            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
