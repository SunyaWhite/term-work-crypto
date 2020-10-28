using System;
using System.Threading.Tasks;
using CommonFiles;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using ServerApp.Services;

namespace ServerApp.Middlewares
{
    public class VerificationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly VerificationService _verificationService;
        private readonly ILogger<VerificationMiddleware> _logger;

        public VerificationMiddleware(RequestDelegate next
            , VerificationService verificationService
            , ILogger<VerificationMiddleware> logger)
        {
            _next = next;
            _verificationService = verificationService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userId = context.Request.Headers[HeaderOptions.USER_ID].ToString();
            var signature = context.Request.Headers[HeaderOptions.SIGNATURE].ToString();
            
            // Can't verify data, if there is no userId
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("Must pass userId as a header parametter");
                
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Must pass userId as a header parametter");
                return;
            }

            if (string.IsNullOrEmpty(signature))
            {
                _logger.LogError("Must pass signature as a header parametter");
                
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Must pass signature as a header parametter");
                return;
            }

            // Allow to read Body from request multiple times
            context.Request.EnableBuffering();
            
            if (! await _verificationService.VerifyDataAsync(context.Request.Body, Guid.Parse(userId!), signature))
            {
                _logger.LogError("Signature verification is failed. Data is corrupted");
                
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Invalid signature. Corrupted data");
                return;
            }
            // Set body stream to zero position. Allow to read it again
            context.Request.Body.Position = 0;
            
            // Set userId in dictionary
            context.Items[HeaderOptions.USER_ID] = userId;

            _logger.LogInformation($"Signature verification is passed");
            
            await _next.Invoke(context);
        }
    }
}