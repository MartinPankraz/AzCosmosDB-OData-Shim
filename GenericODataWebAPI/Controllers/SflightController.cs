using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GenericODataWebAPI;
using GenericODataWebAPI.Core;

namespace GenericODataWebAPI.Controllers
{

    public class SflightController : ControllerBase
    {

        private readonly ILogger<SflightController> _logger;
        private readonly IDataRepository<Sflight> Repository;
        public SflightController(IDataRepository<Sflight> Repository, ILogger<SflightController> logger)
        {
            this.Repository = Repository;
            _logger = logger;
        }

        [EnableQuery()]
        public async Task<IEnumerable<Sflight>> Get()
        {
            return await Repository.GetItemsAsync();
        }
    }
}
