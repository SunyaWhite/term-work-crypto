using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using ClientApp.Utils;
using CommonFiles;
using Microsoft.Extensions.Logging;

namespace ClientApp.Services
{
    public class VerificationService
    {
        private readonly JsonSerializerOptions _opts;
        private readonly ILogger<VerificationService> _logger;
        private readonly string _host;
        private Guid Id { get; set; }
        private byte[] Key { get; set; }

        public VerificationService(string host, ILogger<VerificationService> logger)
        {
            _host = host;
            _opts = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.Default,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = false,
                PropertyNameCaseInsensitive = true
            };
            _logger = logger;
            
            _logger.LogInformation($"Client is listening to url : {_host}");
        }

        /// <summary>
        /// Getting new Id and new Key to sign data
        /// </summary>
        /// <returns></returns>
        public async Task GetNewIdentity()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_host);
                var response =
                    await client.GetFromJsonAsync<KeyResponse>($"/api/verification/newId");
                _logger.LogInformation("Successfully get new identity");

                Id = response.Id;
                Key = response.Key.ToArray();

                _logger.LogInformation($"New Id is {Id}");
                _logger.LogInformation($"New Key is {Key.Display()}");
                _logger.LogInformation("New identity and key are set");

            }
        }

        /// <summary>
        /// Generate signature for payload
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Task<string> SignDataAsync<T>(T payload)
        {
            var jsonData = JsonSerializer.Serialize(payload, _opts);
            
            using var hmac = new HMACSHA256(Key);
            using var streamData = new MemoryStream(Encoding.Default.GetBytes(jsonData));
            
            _logger.LogInformation("Signature is formed ...");
            
            return Task.FromResult(Convert.ToBase64String(hmac.ComputeHash(streamData)));
        }

        /// <summary>
        /// Generate corrupted signature for payload
        /// </summary>
        /// <param name="payload"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private Task<string> SignDataWithCorruptedSignature<T>(T payload)
        {
            var jsonData = JsonSerializer.Serialize(payload, _opts);
            
            using var hmac = new HMACSHA256(Key);
            using var buffer = new MemoryStream(Encoding.Default.GetBytes(jsonData));
            var hmacSignature = hmac.ComputeHash(buffer);
            // Insert small changes
            hmacSignature.InsertCorruption();
            
            _logger.LogInformation("Corrupted signature is formed ...");
            
            return Task.FromResult(Convert.ToBase64String(hmacSignature));
        }

        /// <summary>
        /// Send some dummy request. Generate HMAC to sign it
        /// </summary>
        /// <param name="falseSignature"></param>
        /// <returns></returns>
        public async Task SendSomeRequestAsync(bool falseSignature = false)
        {
            try
            {
                var rand = new Random();
                var randomInt = rand.Next(0, 2000);

                var someText = TextData.LOREM_IPSUM;
                
                    var payload = new CustomRequest
                {
                    Text = someText,
                    Int = randomInt
                };

                _logger.LogInformation("Payload is formed ...");

                // Generating signatures
                var signature = falseSignature
                    ? await SignDataWithCorruptedSignature(payload)
                    : await SignDataAsync(payload);

                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_host);
                    // Settings special headers
                    client.DefaultRequestHeaders.Add(HeaderOptions.USER_ID, Id.ToString());
                    client.DefaultRequestHeaders.Add(HeaderOptions.SIGNATURE, signature);
                    var response = await client.PostAsJsonAsync($"/api/custom/handle", payload, _opts);

                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadFromJsonAsync<CustomResponse>();

                    _logger.LogInformation(
                        $"Successfully received response from server. Response: length = {result.Length} sum = {result.Sum}");
                }
            }
            catch (Exception exc)
            {
                _logger.LogError(exc.Message);
            }
        }
    }
}