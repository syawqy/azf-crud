using System;
using System.Reflection;
using AzureFunctions.Extensions.Swashbuckle;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Fluent;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using Microsoft.Azure.Management.Media;

[assembly: FunctionsStartup(typeof(SwaggerUIAzureFunc.Startup))]
namespace SwaggerUIAzureFunc
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddSwashBuckle(Assembly.GetExecutingAssembly());
            Configure(new FunctionsHostBuilder(builder.Services));
        }

        private static void Configure(IFunctionsHostBuilder builder)
        {
            // register other services here
            builder.Services.AddSingleton((s) => {
                var connectionString = Environment.GetEnvironmentVariable("cosmos-db-bl");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "Please specify a valid CosmosDBConnection in the appSettings.json file or your Azure Functions Settings.");
                }
                return new CosmosClientBuilder(connectionString).Build();
            });
            
        }
    }

    internal class FunctionsHostBuilder : IFunctionsHostBuilder
    {
        public FunctionsHostBuilder(IServiceCollection services)
        {
            var serviceCollection = services;
            Services = serviceCollection ?? throw new ArgumentNullException(nameof(services));
        }

        public IServiceCollection Services { get; }
    }
}