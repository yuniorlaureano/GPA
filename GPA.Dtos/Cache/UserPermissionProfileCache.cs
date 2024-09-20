namespace GPA.Dtos.Cache
{
    public class UserPermissionProfileCache
    {
        public Guid ProfileId { get; set; }
        public string? Value { get; set; }
        public bool IsUserDeleted { get; set; }

        public UserPermissionProfileCache()
        {
        }

        public UserPermissionProfileCache(Guid profileId, string? value, bool isDeleted)
        {
            ProfileId = profileId;
            Value = value;
            IsUserDeleted = isDeleted;
        }
    }
}
