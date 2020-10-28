using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClientApp.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ClientApp
{
    public class Worker : BackgroundService
    {
        private const int MISTAKE_COUNT = 6;
        private readonly ILogger<Worker> _logger;
        private readonly VerificationService _verificationService;

        public Worker(ILogger<Worker> logger, VerificationService verificationService)
        {
            _logger = logger;
            _verificationService = verificationService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await _verificationService.GetNewIdentity();
                var counter = 0;
                
                while (! stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Start sending new request to server ...");
                    await _verificationService.SendSomeRequestAsync(++counter == MISTAKE_COUNT);
                    await Task.Delay(1000 * 8);

                    // Zero our counter
                    counter = counter == MISTAKE_COUNT ? 0 : counter;
                }
            }
            catch (Exception exc)
            {
                _logger.LogCritical(exc.Message);
                _logger.LogError(exc.HelpLink);
                _logger.LogError(exc.InnerException.Message);
                _logger.LogError(exc.InnerException.InnerException.Message);
                _logger.LogCritical("Unable to connect to host and get new identity. Terminating");
                Environment.Exit(1);
            }
        }
    }
}