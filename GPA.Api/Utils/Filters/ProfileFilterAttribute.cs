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
        private readonly PermissionMessage permissionMessage;

        public ProfileFilterAttribute(string path, string permission)
        {
            var pathTokens = path.Split('.');
            permissionPath = SetPermissionPath(pathTokens, permission);
            permissionMessage = SetPermissionMessage(pathTokens, permission);
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
                context.Result = new ObjectResult(permissionMessage)
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
            string? profile = null;
            if (profileId is null)
            {
                permissionMessage.Message = "No tiene un perfil asociado, debe elegir el perfir.";
                context.Result = new ObjectResult(permissionMessage)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
                return null;
            }

            profile = await profileRepo.GetProfileValue(Guid.Parse(profileId));
            if (profile is null)
            {
                permissionMessage.Message = "No tiene perfil asignado. Comunicarse con el administrador";
                context.Result = new ObjectResult(permissionMessage)
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return profile;
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
                Module = pathTokens[1],
                Component = pathTokens[2],
                Permission = permission,
                Message = $"Permiso: '{permission}', módulo: '{pathTokens[1]}', sección: '{pathTokens[2]}', requerido."
            };
        }
    }
}
