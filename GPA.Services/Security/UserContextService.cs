using GPA.Utils.Constants.Claims;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.Security
{
    public interface IUserContextService
    {
        string? GetCurrentUserId();
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? GetCurrentUserId()
        {
            return _httpContextAccessor
                .HttpContext?
                .User?
                .Claims?
                .FirstOrDefault(x => x.Type == GPAClaimTypes.UserId)?.Value;

        }
    }
}
