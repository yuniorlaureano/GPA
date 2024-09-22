using GPA.Common.Entities.Security;

namespace GPA.Entities.Security
{
    public class InvitationToken
    {
        public Guid Id { get; set; }
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public bool Revoked { get; set; }
        public bool Redeemed { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public Guid? RevokedBy { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }

        public GPAUser User { get; set; }
    }
}
