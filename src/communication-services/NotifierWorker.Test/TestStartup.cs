using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotifierWorker.Test
{
    public class TestStartup: Startup
    {

        public TestStartup(IConfiguration configuration) : base(configuration)
        {
            Environment.SetEnvironmentVariable("ENV", "unittest");
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
        }
    }
}
