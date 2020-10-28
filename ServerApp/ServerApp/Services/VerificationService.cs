using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ServerApp.Services
{
    public class VerificationService : IDisposable
    {
        private readonly RNGCryptoServiceProvider _rngCryptoService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VerificationService> _logger;

        public VerificationService(IMemoryCache cache
            , ILogger<VerificationService> logger)
        {
            _cache = cache;
            _logger = logger;
            _rngCryptoService = new RNGCryptoServiceProvider();
        }

        public (Guid, IEnumerable<byte>) GenerateNewIdentityAsync(int keySize)
        {
            var key = new byte[keySize];
            var newId = Guid.NewGuid();

            _rngCryptoService.GetBytes(key);

            _cache.Set(newId, key);
            
            _logger.LogInformation($"Generated new Id {newId} and new key");
            _logger.LogInformation($"Id {newId} and its Key were cached for a day");
            
            return (newId, key);
        }

        public async Task<bool> VerifyDataAsync(Stream data, Guid userId, string signature)
        {
            using var hmac = new HMACSHA256(_cache.Get<byte[]>(userId));
            var value = Convert.ToBase64String(hmac.ComputeHash(data));
            
            _logger.LogInformation($"Passed signature by user from user {userId} : {signature}");
            _logger.LogInformation($"Computed signature : {value}");
            
            return String.Equals(value, signature);
        }

        public void Dispose()
        {
            _rngCryptoService?.Dispose();
        }
    }
}