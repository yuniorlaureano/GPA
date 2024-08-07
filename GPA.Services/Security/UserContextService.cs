﻿using GPA.Utils.Constants.Claims;
using Microsoft.AspNetCore.Http;

namespace GPA.Services.Security
{
    public interface IUserContextService
    {
        Guid GetCurrentUserId();
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

#if DEBUG
            if (currentUser is null)
            {
                return Guid.Parse("00000000-0000-0000-0000-000000000001");
            }
            return Guid.Parse(currentUser!);
#else
            if (currentUser is null || currentUser is { Length: 0 })
            {
                throw new InvalidOperationException("No está autenticado en el sistema");
            }
            return Guid.Parse(currentUser!);
#endif
        }
    }
}
