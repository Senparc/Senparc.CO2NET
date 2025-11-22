using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Senparc.CO2NET.Sample.net10.Controllers
{
    [Route("api/[controller]")]
    //[ApiController]
    public class ValuesController : ControllerBase
    {
        [HttpGet]
        [SwaggerOperationAttribute(Tags = new[] { "v1.0:版本1.0", "v2.0:版本2.0" })]
        [Route("/api/toget")]
        public string OnGet(string name, int value)
        {
            return $"{name}:{value}";
        }

        [HttpPost]
        [SwaggerOperationAttribute(Tags = new[] { "v2.0:版本2.0" })]
        public string OnPost(string name, int value)
        {
            return $"{name}:{value}";
        }

    }
}
