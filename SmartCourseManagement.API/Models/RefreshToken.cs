using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Stores refresh tokens for JWT token renewal.
    /// Allows users to get a new access token without re-entering credentials.
    /// </summary>
    public class RefreshToken : BaseEntity
    {
        /// <summary>The refresh token string (unique and secure)</summary>
        [Required]
        [MaxLength(500)]
        public string Token { get; set; }

        /// <summary>FK to the user who owns this token</summary>
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        /// <summary>When the refresh token expires (UTC)</summary>
        [Required]
        public DateTime ExpiryDate { get; set; }

        /// <summary>Whether this token has been revoked</summary>
        [Required]
        public bool IsRevoked { get; set; } = false;

        /// <summary>When the token was revoked (if applicable)</summary>
        public DateTime? RevokedDate { get; set; }

        /// <summary>IP address where the token was created (for audit purposes)</summary>
        [MaxLength(50)]
        public string IpAddress { get; set; }

        /// <summary>User agent of the device that created the token</summary>
        [MaxLength(500)]
        public string UserAgent { get; set; }
    }
}
