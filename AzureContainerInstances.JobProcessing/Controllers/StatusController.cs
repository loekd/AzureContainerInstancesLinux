using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace AzureContainerInstances.JobProcessing.Controllers
{
    [Route("api/[controller]")]
    public class StatusController : Controller
    {
        // GET api/Status
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/Status/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/Status
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/Status/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/Status/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
