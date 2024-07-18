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
        private readonly string _path;
        private readonly string _permission;

        public ProfileFilterAttribute(string path, string permission)
        {
            _path = path;
            _permission = permission;
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
                context.Result = new ObjectResult("No tiene permisos para acceder a este recurso.")
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

            var pathTokens = _path.Split('.');
            var permissionPath = ProfileConstants.CreatePath(
                app: pathTokens[0],
                module: pathTokens[1],
                component: pathTokens[2],
                valueToCompare: _permission);

            return permissionComparer.PermissionMatchesPathStep(profile, permissionPath);
        }

        private async Task<string?> GetProfile(IGPAProfileRepository profileRepo, string? profileId, AuthorizationFilterContext context, string? userId)
        {
            string? profile = null;
            if (profileId is null)
            {
                context.Result = new ObjectResult("No tiene un perfil asociado, debe elegir el perfir.")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return null;
            }

            profile = await profileRepo.GetProfileValue(Guid.Parse(profileId));
            if (profile is null)
            {
                context.Result = new ObjectResult("El perfil no existe.")
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return profile;
        }
    }
}
