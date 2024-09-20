using GPA.Data.Security;
using GPA.Dtos.Cache;
using GPA.Utils.Caching;
using GPA.Utils.Constants.Claims;
using GPA.Utils.Permissions;
using GPA.Utils.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GPA.Api.Utils.Filters
{
    public class ProfileFilterAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly PermissionPathWithValue permissionPath;
        private string errorMessage = "";

        public ProfileFilterAttribute(string path, string permission)
        {
            var pathTokens = path.Split('.');
            errorMessage = $"Permiso '{PermissionsTranslate.Translates[permission]}', en '{PermissionsTranslate.Translates[pathTokens[2]]}' requerido.";
            permissionPath = SetPermissionPath(pathTokens, permission);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var permissionComparer = context.HttpContext.RequestServices.GetService(typeof(IPermissionComparer)) as IPermissionComparer;
            var profileRepo = context.HttpContext.RequestServices.GetService(typeof(IGPAProfileRepository)) as IGPAProfileRepository;
            var cache = context.HttpContext.RequestServices.GetService(typeof(IGenericCache<UserPermissionProfileCache>)) as IGenericCache<UserPermissionProfileCache>;

            var profileId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == GPAClaimTypes.ProfileId)?.Value;
            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == GPAClaimTypes.UserId)?.Value;

            if (string.IsNullOrEmpty(profileId))
            {
                context.Result = new ObjectResult("No tiene perfil asignado. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ObjectResult("Session expirada")
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            var cachedProfile = await cache.GetOrCreate(CacheType.Permission, GetToken(context), async () =>
            {
                var profile = await profileRepo.GetProfileValue(Guid.Parse(profileId), Guid.Parse(userId));
                if (profile is null)
                {
                    return null;
                }

                return new UserPermissionProfileCache(profileId: Guid.Parse(profileId), value: profile?.value, isDeleted: profile?.isDeleted ?? true);
            });

            if (cachedProfile is null)
            {
                context.Result = new ObjectResult("No tiene perfil asignado. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            if (cachedProfile?.IsUserDeleted == true)
            {
                context.Result = new ObjectResult("El usuario está desactivado")
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
                return;
            }

            if (string.IsNullOrEmpty(cachedProfile?.Value))
            {
                context.Result = new ObjectResult("El perfil que está utilizando no tiene permisos asignados. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }

            var valid = ValidatePermission(permissionComparer, cachedProfile?.Value, context);
            if (!valid)
            {
                context.Result = new ObjectResult(errorMessage)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return;
            }
        }

        private bool ValidatePermission(IPermissionComparer permissionComparer, string? profile, AuthorizationFilterContext context)
        {
            if (profile is null)
            {
                return false;
            }

            return permissionComparer.PermissionMatchesPathStep(profile, permissionPath);
        }

        private PermissionPathWithValue SetPermissionPath(string[] pathTokens, string permission)
        {
            return ProfileConstants.CreatePath(
                app: pathTokens[0],
                module: pathTokens[1],
                component: pathTokens[2],
                valueToCompare: permission);
        }

        private string GetToken(AuthorizationFilterContext context)
        {
            var authorizationHeader = context.HttpContext.Request.Headers["Authorization"];
            if (authorizationHeader.Count == 0 || string.IsNullOrEmpty(authorizationHeader[0]))
            {
                return string.Empty;
            }

            return authorizationHeader[0].Split(" ")[1];
        }
    }
}
