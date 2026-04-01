using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Stores refresh tokens issued alongside JWT access tokens.
    /// When a JWT expires the client uses this token to obtain a new JWT without re-authenticating.
    /// Each token is invalidated (IsUsed = true) after a single use to prevent replay attacks.
    /// </summary>
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }

        // FK to the owning user
        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
