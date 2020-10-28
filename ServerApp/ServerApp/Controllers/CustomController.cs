using System;
using System.Threading.Tasks;
using CommonFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServerApp.Utils;

namespace ServerApp.Controllers
{
    /// <summary>
    /// Some dummy controller
    /// </summary>
    [Route(Routes.CUSTOM_ROUTE)]
    public class CustomController : Controller
    {

        private readonly ILogger<CustomController> _logger;

        public CustomController(ILogger<CustomController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Pretend doing something
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("handle")]
        public async Task<IActionResult> Handle([FromBody] CustomRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new ArgumentException("Invalid arguments are passed");
                }
                
                // pretending we are busy
                await Task.Delay(1000);
                // Doing something
                var length = request.Text.Length;
                var result = new CustomResponse
                {
                    Length =  length,
                    Sum = request.Int + length
                };
                
                _logger.LogInformation($"Successfully handled request from {HttpContext.Items[HeaderOptions.USER_ID] ?? string.Empty}");

                return Ok(result);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
                return BadRequest();
            }
        }
    }
}