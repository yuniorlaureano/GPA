using GPA.Utils.Constants.Claims;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.Security
{
    public interface IUserContextService
    {
        Guid GetCurrentUserId();
        string GetCurrentUserName();
    }

    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContextService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GetCurrentUserId()
        {
            var currentUser = _httpContextAccessor
                .HttpContext?
                .User?
                .Claims?
                .FirstOrDefault(x => x.Type == GPAClaimTypes.UserId)?.Value;

            return Guid.Parse(currentUser ?? "00000000-0000-0000-0000-000000000001");
        }


        public string GetCurrentUserName()
        {
            var currentUserName = _httpContextAccessor
                .HttpContext?
                .User?
                .Claims?
                .FirstOrDefault(x => x.Type == GPAClaimTypes.FullName)?.Value;

            return currentUserName ?? "Test use";
        }
    }
}
