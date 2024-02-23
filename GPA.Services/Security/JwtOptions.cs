namespace GPA.Services.Security
{
    public class JwtOptions
    {
        public static string SectionName {get;} = "Jwt";
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Key { get; set; }
        public int Expires { get; set; }
    }
}
