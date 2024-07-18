namespace GPA.Utils.Permissions
{
    public class PathStep
    {
        public string PropertyName { get; set; }
        public string? ArrayPropertyValue { get; set; }
        public bool IsSimpleArray { get; set; } = false;
    }

    public class PermissionPaths
    {
        public List<PathStep> PermissionPath { get; set; } = new List<PathStep>();
    }

    public class PermissionPathWithValue : PermissionPaths
    {
        public string ValueToCompare { get; set; }
    }
}
