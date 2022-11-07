using DemoPolly.ExternalServices;
using Microsoft.AspNetCore.Mvc;

namespace DemoPolly.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PollyController : ControllerBase
    {
        private readonly IAnyApiService _service;

        public PollyController(IAnyApiService service)
        {
            _service = service;
        }

        [HttpGet]
        [Route("error")]
        public async Task<ActionResult<object>> GetWithError()
        {
            var result = await _service.GetSomethingWithException();

            return NotFound();
        }

        [HttpGet]
        [Route("sucess")]
        public async Task<ActionResult<object>> GetWithSuccess()
        {
            var result = await _service.GetSomethingWithSuccess();

            return Ok();
        }
    }
}