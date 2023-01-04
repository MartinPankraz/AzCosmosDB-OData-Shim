using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OData.Edm;
using Microsoft.Identity.Web;
using GenericODataWebAPI.Core;
using GenericODataWebAPI.Cosmos;
using System.IdentityModel.Tokens.Jwt;

namespace GenericODataWebAPI
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
            //uncomment to start using AAD
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(Configuration, "AzureAd");

            services.AddControllers();
            
            services.AddHealthChecks();

            var Endpoint = Configuration.GetValue<string>("Modules:CosmosConfig:Endpoint");
            var Key = Configuration.GetValue<string>("Modules:CosmosConfig:Key");
            var DatabaseId = Configuration.GetValue<string>("Modules:CosmosConfig:DatabaseId");
            var CollectionId = Configuration.GetValue<string>("Modules:CosmosConfig:CollectionId");

            services.AddSingleton<IDataRepository<Sflight>>(new CosmosDBRepository<Sflight>(Endpoint,Key,DatabaseId,CollectionId));
            //services.AddSingleton<IDataRepository<Sflight>>(new AzureBlobRepository<Sflight>());
            services.AddOData();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // If we do this in the request we can look for the x-forwarded for header.
            // OR give configuration manual control like we have done here. YMMV
            //app.UseRequestRewriter(new RequestRewriterOptions(new List<string>(){$"{System.Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")}.azurewebsites.net"},System.Environment.GetEnvironmentVariable("RewriteModule:NewRoute")));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseODataBatching();
            app.UseRouting();

            //uncomment to start using AAD
            //app.UseAuthentication();
            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
                endpoints.Select().Filter().OrderBy().Count().MaxTop(100);

                /*consider tweaking the batch quota depending on your requirements!*/
                var odataBatchHandler  = new DefaultODataBatchHandler();
                odataBatchHandler .MessageQuotas.MaxNestingDepth = 2;
                odataBatchHandler .MessageQuotas.MaxOperationsPerChangeset = 10;
                odataBatchHandler .MessageQuotas.MaxReceivedMessageSize = 100;

                endpoints.MapODataRoute(
                    routeName: "odata",
                    routePrefix: "api/odata",
                    model: GetEdmModel(),
                    batchHandler: odataBatchHandler );

                endpoints.MapHealthChecks("/health");
            });
        }

        private IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            odataBuilder.EntitySet<Sflight>("Sflight");
            return odataBuilder.GetEdmModel();
        }
    }
}
