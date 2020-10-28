using System;
using CommonFiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ServerApp.Services;

namespace ServerApp.Controllers
{
    [Route("/api/verification")]
    public class VerificationController : Controller
    {

        private readonly ILogger<VerificationController> _logger;
        private readonly VerificationService _verificationService;
        
        public VerificationController(VerificationService verificationService
            , IMemoryCache cache
            , ILogger<VerificationController> logger)
        {
            _logger = logger;
            _verificationService = verificationService;
        }

        [HttpGet("newId")]
        public IActionResult GetNewId()
        {
            try
            {
                var (id, key) = _verificationService.GenerateNewIdentityAsync(64);
                
                var response = new KeyResponse
                {
                    Id = id,
                    Key = key
                };
                
                return Ok(response);
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
                return BadRequest();
            }
        }
    }
}