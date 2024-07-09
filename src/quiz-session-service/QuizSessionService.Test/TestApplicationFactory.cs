using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizSessionService.Test
{
    public class TestApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : Startup
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            var configuration = Program.GetConfiguration();

            builder
                .UseStartup<TStartup>()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(configuration);
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder();
        }
    }
}
