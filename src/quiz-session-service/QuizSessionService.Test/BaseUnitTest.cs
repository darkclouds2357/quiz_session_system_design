using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizSessionService.Test
{
    public abstract class BaseUnitTest : IClassFixture<TestApplicationFactory<TestStartup>>
    {
        private readonly TestApplicationFactory<TestStartup> _factory;
        protected readonly IServiceProvider ServiceProvider;

        protected BaseUnitTest(TestApplicationFactory<TestStartup> factory)
        {
            this._factory = factory;
            ServiceProvider = factory.Services;
        }
    }
}
