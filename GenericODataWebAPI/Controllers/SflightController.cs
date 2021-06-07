using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using GenericODataWebAPI.Core;

namespace GenericODataWebAPI.Controllers
{
    [Authorize(Roles = "Sflight")]
    [ODataRouting]
    public class SflightController : ControllerBase
    {
        private readonly IDataRepository<Sflight> Repository;
        public SflightController(IDataRepository<Sflight> Repository)
        {
            this.Repository = Repository;
        }
        
        [EnableQuery()]
        [Authorize(Roles = "Reader")]
        public async Task<IEnumerable<Sflight>> Get()
        {
            return await Repository.GetItemsAsync();
        }

        
        [EnableQuery]
        [Authorize(Roles = "Reader")]
        public async Task<Sflight> Get(string key)
        {
            return await Repository.GetItemAsync(key);       
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        public async Task<string> Post([FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(flight.id, flight);
        }
        
        [EnableQuery]
        [Authorize(Roles = "Writer")]
        public async Task<string> Put(string key, [FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(key, flight);
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        public async Task<string> Patch(string key, Delta<Sflight> flight)
        {
            return await Repository.PatchItemAsync(key, flight);
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        public async Task Delete([FromODataUri]string key)
        {
            await Repository.DeleteItemAsync(key);
        }
    }
}
