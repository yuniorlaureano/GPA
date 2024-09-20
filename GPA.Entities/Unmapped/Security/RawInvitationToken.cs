namespace GPA.Entities.Unmapped.Security
{
    public class RawInvitationToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public bool Revoked { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
