using System.Security.Claims;

namespace TaskManager.Api.Extensions;

internal static class ClaimsPrincipalExtensions
{
    internal static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out userId);
    }
}
