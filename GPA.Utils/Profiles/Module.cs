namespace GPA.Utils.Profiles
{
    public class Module
    {
        public string Id { get; set; }
        public List<Component> Components { get; set; } = new List<Component>();
    }
}
