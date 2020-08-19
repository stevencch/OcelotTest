using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProductApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            if (isService)
            {
                CreateHostBuilder(args).UseWindowsService().Build().Run();
            }
            else
            {
                CreateHostBuilder(args).Build().Run();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("hostsettings.json", optional: true, reloadOnChange: true)
            .Build();
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>().UseConfiguration(config);
                });
        }
    }
}
