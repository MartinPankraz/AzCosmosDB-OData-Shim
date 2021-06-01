using System;
using GenericODataWebAPI.Core;
using Microsoft.AspNetCore.Mvc;

namespace GenericODataWebAPI.Controllers
{
    [ApiController]
    [Route("/geode")]
    public class GeodeCheckController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get() {
            var geode = System.Environment.GetEnvironmentVariable("geode-name") ?? "no geode assigned";
            return new OkObjectResult(geode);
        }
    }
}