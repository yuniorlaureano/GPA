using GPA.Data.Security;
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
        private readonly string _path = "";
        private readonly string _permission = "";

        public ProfileFilterAttribute(string path, string permission)
        {
            var pathTokens = path.Split('.');
            _path = path;
            _permission = permission;
            permissionPath = SetPermissionPath(pathTokens, permission);
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var permissionComparer = context.HttpContext.RequestServices.GetService(typeof(IPermissionComparer)) as IPermissionComparer;
            var profileRepo = context.HttpContext.RequestServices.GetService(typeof(IGPAProfileRepository)) as IGPAProfileRepository;

            var profileId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == GPAClaimTypes.ProfileId)?.Value;
            var userId = context.HttpContext.User.Claims.FirstOrDefault(c => c.Type == GPAClaimTypes.UserId)?.Value;

            var profile = await GetProfile(profileRepo, profileId, context, userId);
            var valid = ValidatePermission(permissionComparer, profile, context);
            if (!valid)
            {
                context.Result = new ObjectResult(SetPermissionMessage(_path.Split("."), _permission))
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
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

        private async Task<string?> GetProfile(IGPAProfileRepository profileRepo, string? profileId, AuthorizationFilterContext context, string? userId)
        {
            if (profileId is null || profileId is { Length: 0 })
            {
                context.Result = new ObjectResult("No tiene perfil asignado. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return null;
            }

            var profile = await profileRepo.GetProfileValue(Guid.Parse(profileId));
            if (profile is null)
            {
                context.Result = new ObjectResult("No tiene perfil asignado. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }
            else if(string.IsNullOrEmpty(profile?.value))
            {
                context.Result = new ObjectResult("El perfil que está utilizando no tiene permisos asignados. Comunicarse con el administrador")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return profile?.value;
        }

        private PermissionPathWithValue SetPermissionPath(string[] pathTokens, string permission)
        {
            return ProfileConstants.CreatePath(
                app: pathTokens[0],
                module: pathTokens[1],
                component: pathTokens[2],
                valueToCompare: permission);
        }

        private PermissionMessage SetPermissionMessage(string[] pathTokens, string permission)
        {
            return new PermissionMessage
            {
                Message = $"Permiso '{permission}', en '{pathTokens[2]}' requerido."
            };
        }
    }
}
