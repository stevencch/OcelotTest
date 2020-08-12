using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProductApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        public IActionResult Get()
        {
            Console.WriteLine("running health check" + DateTime.Now);
            //if (DateTime.Now.Minute % 2 == 0)
            //{
            //    return BadRequest("error");
            //}
            return Ok("OK");
        }
    }
}
