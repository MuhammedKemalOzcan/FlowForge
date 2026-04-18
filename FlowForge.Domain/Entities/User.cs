using FlowForge.Domain.Enums;
using FlowForge.Domain.ValueObjects;

namespace FlowForge.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; }
        public ExternalIdentityId ExternalIdentityId { get; private set; }
        public Email Email { get; private set; }
        public string FullName { get; private set; }
        public Status Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private User()
        { }

        public static User CreateFromIdentityProvider(ExternalIdentityId externalId, Email email, string fullName)
        {
            if (externalId is null) throw new ArgumentException("External id cannot be null", nameof(externalId));
            if (email is null) throw new ArgumentException("Email cannot be null", nameof(email));
            if (string.IsNullOrEmpty(fullName)) throw new ArgumentException("Name cannot be null", nameof(fullName));
            if (fullName.Length > 100) throw new ArgumentException("Name length must be below 100", nameof(fullName));

            User user = new User()
            {
                Id = Guid.NewGuid(),
                ExternalIdentityId = externalId,
                Email = email,
                FullName = fullName,
                Status = Status.Active,
                CreatedAt = DateTime.UtcNow
            };

            return user;
        }

        //Provider üzerinden email veya isim değişikliğinde lokal copy'i güncelliyoruz.
        public void SyncFromIdentityProvider(Email email, string fullName)
        {
            if (email is null) throw new ArgumentException("Email cannot be null", nameof(email));
            if (string.IsNullOrEmpty(fullName)) throw new ArgumentException("Name cannot be null", nameof(fullName));
            if (fullName.Length > 100) throw new ArgumentException("Name length must be below 100", nameof(fullName));

            Email = email;
            FullName = fullName;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Suspend()
        {
            if (Status == Status.Suspended) return;
            if (Status == Status.Deleted) return;

            Status = Status.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Reactivate()
        {
            if (Status == Status.Active) return;
            if (Status == Status.Deleted) return;
            Status = Status.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Delete()
        {
            if (Status == Status.Deleted) return;
            Status = Status.Deleted;
            UpdatedAt = DateTime.UtcNow;
        }
}