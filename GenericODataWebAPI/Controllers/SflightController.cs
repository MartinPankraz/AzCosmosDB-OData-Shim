using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using GenericODataWebAPI.Core;
namespace GenericODataWebAPI.Controllers
{
    [ODataRouting]
    public class SflightController : ControllerBase
    {
        private readonly IDataRepository<Sflight> Repository;
        public SflightController(IDataRepository<Sflight> Repository)
        {
            this.Repository = Repository;
        }

        [EnableQuery()]
        public async Task<IEnumerable<Sflight>> Get()
        {
            return await Repository.GetItemsAsync();
        }

        
        [EnableQuery]
        [HttpGet("odata/Sflight({id})")]
        public async Task<Sflight> Get(string id)
        {
            //cut leading and trailing quotes from string. Data service expects the content only.
            if(id.StartsWith("'") && id.EndsWith("'")){
                var length = id.Length - 2;
                id = id.Substring(1,length);
            }
            return await Repository.GetItemAsync(id);       
        }

        [EnableQuery]
        [HttpPost]
        public async Task<string> Post([FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(flight.id, flight);
        }
        
        [EnableQuery]
        [HttpPut]
        public async Task<string> Put([FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(flight.id, flight);
        }

        [EnableQuery]
        [HttpDelete]
        public async Task Delete([FromODataUri]string key)
        {
            await Repository.DeleteItemAsync(key);
        }
    }
}
