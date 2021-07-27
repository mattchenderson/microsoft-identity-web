// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

[assembly: FunctionsStartup(typeof(SampleFunc.Startup))]

namespace SampleFunc
{
    public class Startup : FunctionsStartup
    {
        public Startup()
        {
        }
        IConfiguration Configuration { get; set; }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            FunctionsHostBuilderContext context = builder.GetContext();
            builder.ConfigurationBuilder
               .AddJsonFile(Path.Combine(context.ApplicationRootPath, "appsettings.json"), optional: true, reloadOnChange: false)
               .AddEnvironmentVariables();
        }


        public override void Configure(IFunctionsHostBuilder builder)
        {
            IConfiguration configuration = builder.GetContext().Configuration;
            builder.Services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = Microsoft.Identity.Web.Constants.Bearer;
                sharedOptions.DefaultChallengeScheme = Microsoft.Identity.Web.Constants.Bearer;
            })
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"))
                    .EnableTokenAcquisitionToCallDownstreamApi()
                    .AddDownstreamWebApi("DownstreamApi", configuration.GetSection("DownstreamApi"))
                    .AddMicrosoftGraph(configuration.GetSection("DownstreamApi"))
                    .AddInMemoryTokenCaches();

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = options.DefaultPolicy;
            });
        }
    }
}
