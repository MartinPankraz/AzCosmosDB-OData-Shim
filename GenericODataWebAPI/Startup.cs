using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using GenericODataWebAPI.Core;
using GenericODataWebAPI.Cosmos; 

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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.EnableDependencyInjection();
                endpoints.Select().Filter().OrderBy().Count().MaxTop(100);
                endpoints.MapODataRoute("odata", "odata", GetEdmModel());

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
