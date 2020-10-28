using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using ServerApp.Middlewares;

namespace ServerApp.Utils
{
    public static class EndpointsExtension
    {
        public static IEndpointConventionBuilder MapVerificationMiddleware(this IEndpointRouteBuilder endpoints
            , string pattern)
        {
            var pipeline = endpoints.CreateApplicationBuilder()
                .UseMiddleware<VerificationMiddleware>()
                .Build();

            return endpoints.Map(pattern, pipeline);
        }
    }
}