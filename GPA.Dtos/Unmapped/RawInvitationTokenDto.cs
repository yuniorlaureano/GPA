namespace GPA.Dtos.Unmapped
{
    public class RawInvitationTokenDto
    {
        public Guid Id { get; set; }
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public bool Revoked { get; set; }
        public bool Redeemed { get; set; }
        public Guid CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public Guid? RevokedBy { get; set; }
        public string? RevokedByName { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
    }
}
