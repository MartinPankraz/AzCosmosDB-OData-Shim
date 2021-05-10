using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents;

namespace AzCosmosDB_OData_Shim.Controllers
{

    public class SflightController : ControllerBase
    {

        private readonly ILogger<SflightController> _logger;
        private readonly IDocumentDBRepository<AzCosmosDB_OData_Shim.Sflight> Respository;
        public SflightController(IDocumentDBRepository<AzCosmosDB_OData_Shim.Sflight> Respository, ILogger<SflightController> logger)
        {
            this.Respository = Respository;
            _logger = logger;
        }

        [EnableQuery()]
        public async Task<AzCosmosDB_OData_Shim.Sflight> Get()
        {
            string id = "006";
            return await Respository.GetItemAsync(id);
        }
    }
}
