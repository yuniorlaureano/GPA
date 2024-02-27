namespace GPA.Dtos.Security
{
    public class RoleClaimModuleDto
    {
        public int Id { get; set; }
        public string Module { get; set; }
        public string Permissions { get; set; }
    }

    public class PermissionDto
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public RoleClaimModuleDto[] Modules { get; set; }
    }
}
