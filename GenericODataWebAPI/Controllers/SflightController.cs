using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web.Resource;
using GenericODataWebAPI.Core;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Deltas;

namespace GenericODataWebAPI.Controllers
{
    //uncomment to start using AAD
    //[Authorize(Roles = "Sflight")]
    [ApiController]
    [Route("api/[controller]")]
    public class SflightController : ControllerBase
    {
        private readonly IDataRepository<Sflight> Repository;
        public SflightController(IDataRepository<Sflight> Repository)
        {
            this.Repository = Repository;
        }
        
        [EnableQuery]
        //[Authorize(Roles = "Reader")]
        public async Task<IEnumerable<Sflight>> Get()
        {
            return await Repository.GetItemsAsync();
        }

        
        [EnableQuery]
        //[Authorize(Roles = "Reader")]
        [HttpGet("odata/Sflight({key})")]
        public async Task<Sflight> Get([FromRoute]string key)
        {
            return await Repository.GetItemAsync(key);       
        }

        [EnableQuery]
        //[Authorize(Roles = "Writer")]
        public async Task<Sflight> Post([FromBody]Sflight flight)
        {
            return await Repository.CreateItemAsync(flight);
        }
        
        [EnableQuery]
        //[Authorize(Roles = "Writer")]
        [HttpGet("odata/Sflight({key})")]
        public async Task<Sflight> Put([FromRoute]string key, [FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(key, flight);
        }

        [EnableQuery]
        //[Authorize(Roles = "Writer")]
        [HttpGet("odata/Sflight({key})")]
        public async Task<Sflight> Patch([FromRoute]string key, Delta<Sflight> flight)
        {
            return await Repository.PatchItemAsync(key, flight);
        }

        [EnableQuery]
        //[Authorize(Roles = "Writer")]
        [HttpGet("odata/Sflight({key})")]
        public async Task Delete([FromRoute]string key)
        {
            await Repository.DeleteItemAsync(key);
        }
    }
}
