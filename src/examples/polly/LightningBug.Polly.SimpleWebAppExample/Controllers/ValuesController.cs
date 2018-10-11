using System.Collections.Generic;
using LightningBug.Polly.SimpleWebAppExample.Services;
using Microsoft.AspNetCore.Mvc;

namespace LightningBug.Polly.SimpleWebAppExample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly IValuesProvider _valuesProvider;

        public ValuesController(IValuesProvider valuesProvider)
        {
            _valuesProvider = valuesProvider;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            var result = _valuesProvider.GetValues();
            return Ok(result);
        }
    }
}
