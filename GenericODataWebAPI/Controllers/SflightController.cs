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
        [HttpGet("api/odata/Sflight({id})")]
        public async Task<Sflight> Get(string id)
        {
            return await Repository.GetItemAsync(getStringFromOdataPath(id));       
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        [HttpPost]
        public async Task<string> Post([FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(flight.id, flight);
        }
        
        [EnableQuery]
        [Authorize(Roles = "Writer")]
        [HttpPut("api/odata/Sflight({id})")]
        public async Task<string> Put(string id, [FromBody]Sflight flight)
        {
            return await Repository.UpdateItemAsync(getStringFromOdataPath(id), flight);
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        [HttpPatch("api/odata/Sflight({id})")]
        public async Task<string> Patch(string id, [FromBody]Sflight flight)
        {
            return await Repository.PatchItemAsync(getStringFromOdataPath(id), flight);
        }

        [EnableQuery]
        [Authorize(Roles = "Writer")]
        [HttpDelete]
        public async Task Delete([FromODataUri]string key)
        {
            await Repository.DeleteItemAsync(key);
        }

        /**
            Typical id is given to service as https://<your service url>>/odata/<entity>('001'). That gets parsed as string including the quote.
            We need to drop it to avoid parsing error.
        */
        private string getStringFromOdataPath(string extractedId){
            string id = "";
            //cut leading and trailing quotes from string. Data service expects the content only.
            if(extractedId.StartsWith("'") && extractedId.EndsWith("'")){
                var length = extractedId.Length - 2;
                id = extractedId.Substring(1,length);
            }
            return id;
        }
    }
}
