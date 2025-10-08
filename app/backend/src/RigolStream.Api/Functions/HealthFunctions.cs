using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace RigolStream.Api.Functions;

/// <summary>Liveness probe — cheap, no instrument access.</summary>
public sealed class HealthFunctions
{
    [Function("Health")]
    public IActionResult Health(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req)
    {
        return new OkObjectResult(new
        {
            status = "ok",
            service = "RigolStream.Api",
            utc = DateTimeOffset.UtcNow,
        });
    }
}
