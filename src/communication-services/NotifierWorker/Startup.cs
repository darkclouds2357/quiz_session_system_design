using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using NotifierWorker.Impl;
using StackExchange.Redis.Extensions.Core.Configuration;
using StackExchange.Redis.Extensions.Newtonsoft;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Reflection;

namespace NotifierWorker
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            AddMeditor(services);
            AddCorsService(services, Configuration);
            AddApiVersioningService(services);

            AddOptionConfigurationServices(services);
            AddSwaggerGenService(services, this.GetType().Assembly);

            services.AddControllers();
            services
                .AddMvcCore(option => { });
            //.AddNewtonsoftJson(options => { });
            //.SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddHealthChecks();
            services.AddRazorPages();

            var setting = new RedisSetting
            {
                Host = Env.REDIS_HOST,
                Port = Env.REDIS_PORT,
                //InstanceName = !string.IsNullOrWhiteSpace(Env.HOST_PREFIX) ? $"{Env.HOST_PREFIX}" : string.Empty,
                Password = Env.REDIS_PASSWORD,
                Ssl = Env.REDIS_SSL,
                Database = Env.REDIS_DB,
                SslProtocols = Env.REDIS_SSL ? System.Security.Authentication.SslProtocols.Tls12 : null
            };

            var redisConfigurationInstance = new RedisConfiguration
            {
                IsDefault = true,
                AbortOnConnectFail = false,
                AllowAdmin = false,
                Database = setting.Database,
                Hosts =
                    [
                        new()
                        {
                            Host = setting.Host,
                            Port = setting.Port
                        }
                    ],
                ConnectTimeout = setting.ConnectTimeout,
                SyncTimeout = setting.SyncTimeout,
                ConnectRetry = setting.ConnectRetry,
                ServerEnumerationStrategy = new ServerEnumerationStrategy
                {
                    Mode = ServerEnumerationStrategy.ModeOptions.All,
                    TargetRole = ServerEnumerationStrategy.TargetRoleOptions.Any,
                    UnreachableServerAction = ServerEnumerationStrategy.UnreachableServerActionOptions.Throw
                },
                KeyPrefix = setting.InstanceName,
                Password = setting.Password,
                Ssl = setting.Ssl,
                SslProtocols = setting.SslProtocols,
            };
            //services.AddSingleton(redisConfigurationInstance);
            services.AddStackExchangeRedisExtensions<NewtonsoftSerializer>(new RedisConfiguration[] { redisConfigurationInstance });

            services.AddHostedService<RedisSubscribeService>();
            services.AddSingleton<IMessageBus, RedisPubSubMessageBus>();

        }

        private IServiceCollection AddMeditor(IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Startup).Assembly));

            return services;
        }

        private IServiceCollection AddApiVersioningService(IServiceCollection services)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.DefaultApiVersion = new ApiVersion(1, 0); // specify the default api version
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new MediaTypeApiVersionReader("api-version"); // read the version number from the accept header
            }).AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'V'VVV";
            });
            return services;
        }
        private IServiceCollection AddCorsService(IServiceCollection services, IConfiguration configuration)
        {
            var domains = configuration["Cors:Domains"].Split(',').ToArray();
            if (domains.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .WithOrigins(domains)
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
                });
            }
            else
            {
                services.AddCors(options =>
                {
                    options.AddPolicy("CorsPolicy",
                        builder => builder
                        .SetIsOriginAllowed((host) => true)
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials());
                });
            }

            return services;
        }
        private IServiceCollection AddOptionConfigurationServices(IServiceCollection services)
        {
            services.AddOptions();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var problemDetails = new ValidationProblemDetails(context.ModelState)
                    {
                        Instance = context.HttpContext.Request.Path,
                        Status = StatusCodes.Status400BadRequest,
                        Detail = "Please refer to the errors property for additional details."
                    };

                    return new BadRequestObjectResult(problemDetails)
                    {
                        ContentTypes = { "application/problem+json", "application/problem+xml" }
                    };
                };
            });

            return services;
        }
        private IServiceCollection AddSwaggerGenService(IServiceCollection services, Assembly assembly)
        {
            services.AddSwaggerGen(options =>
            {
                var provider = services.BuildServiceProvider()
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var apiVersion in provider.ApiVersionDescriptions)
                {
                    // ConfigureVersionedDescription(options, apiVersion);
                    options.SwaggerDoc(apiVersion.GroupName, new OpenApiInfo()
                    {
                        Title = $"API - version {apiVersion.ApiVersion}",
                        Version = apiVersion.ApiVersion.ToString(),
                        Description = apiVersion.IsDeprecated ? $"API - DEPRECATED" : "API",
                    });
                }
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "Bearer",
                            Name = "Authorization",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
                var servicePrefix = Environment.GetEnvironmentVariable("SERVICE_PREFIX");
                if (!string.IsNullOrWhiteSpace(servicePrefix))
                {
                    options.DocumentFilter<ServicePrefixInsertDocumentFilter>(servicePrefix);
                }

                //// Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{assembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    options.IncludeXmlComments(xmlPath);
            });

            return services;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IApiVersionDescriptionProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            UseCorsz(app);
            UseSwaggerz(app, provider);

            var healthCheckPattern = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HEALTH_CHECK_ENDPOINT")) ? Environment.GetEnvironmentVariable("HEALTH_CHECK_ENDPOINT") : "/ping";
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks(healthCheckPattern);
            });
        }

        public void UseCorsz(IApplicationBuilder app)
        {
            app.UseCors("CorsPolicy");
        }

        public void UseSwaggerz(IApplicationBuilder app, IApiVersionDescriptionProvider provider)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersion in provider.ApiVersionDescriptions
                    .OrderBy(version => version.ToString()))
                {
                    c.SwaggerEndpoint(
                        $"swagger/{apiVersion.GroupName.ToUpperInvariant()}/swagger.json", apiVersion.GroupName.ToUpperInvariant()
                    );
                }

                c.RoutePrefix = string.Empty;
            });
        }
    }
}

public class ServicePrefixInsertDocumentFilter : IDocumentFilter
{
    private readonly string _prefix;

    public ServicePrefixInsertDocumentFilter(string prefix)
    {
        _prefix = prefix;
    }

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var paths = swaggerDoc.Paths.Keys.ToList();
        foreach (var path in paths)
        {
            var partPrefix = $"/{_prefix}{path}";

            if (!path.StartsWith("/"))
                partPrefix = $"/{_prefix}/{path}";

            var pathToChange = swaggerDoc.Paths[path];
            swaggerDoc.Paths.Remove(path);
            swaggerDoc.Paths.Add(partPrefix, pathToChange);
        }
    }
}