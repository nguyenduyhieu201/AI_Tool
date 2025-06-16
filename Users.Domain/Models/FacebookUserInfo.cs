using System;

namespace Users.Domain.Models
{
    /// <summary>
    /// Value Object chứa thông tin user từ Facebook
    /// </summary>
    public class FacebookUserInfo
    {
        /// <summary>
        /// Facebook user ID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Email của user
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Tên của user
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// Họ của user
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// URL ảnh đại diện của user
        /// </summary>
        public string PictureUrl { get; }

        private FacebookUserInfo() { } // For EF Core

        public FacebookUserInfo(
            string id,
            string email,
            string firstName,
            string lastName,
            string pictureUrl)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Facebook ID cannot be empty", nameof(id));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));

            Id = id;
            Email = email;
            FirstName = firstName;
            LastName = lastName;
            PictureUrl = pictureUrl;
        }

        public override bool Equals(object obj)
        {
            if (obj is not FacebookUserInfo other)
                return false;

            return Id == other.Id &&
                   Email == other.Email &&
                   FirstName == other.FirstName &&
                   LastName == other.LastName &&
                   PictureUrl == other.PictureUrl;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Email, FirstName, LastName, PictureUrl);
        }

        public static bool operator ==(FacebookUserInfo left, FacebookUserInfo right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;
            return left.Equals(right);
        }

        public static bool operator !=(FacebookUserInfo left, FacebookUserInfo right)
        {
            return !(left == right);
        }
    }
} 