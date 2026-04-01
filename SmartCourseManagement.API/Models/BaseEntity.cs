using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Base class for all entities. Provides audit fields and soft delete support.
    /// Every record automatically tracks who created/modified it and when.
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>When the record was created (UTC)</summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Who created the record (UserId or username)</summary>
        [MaxLength(100)]
        public string? CreatedBy { get; set; } = "System";

        /// <summary>When the record was last modified (UTC)</summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Who last modified the record (UserId or username)</summary>
        [MaxLength(100)]
        public string? UpdatedBy { get; set; } = "System";

        /// <summary>Soft delete flag. When true, record is logically deleted but not removed from DB</summary>
        [Required]
        public bool IsDeleted { get; set; } = false;

        /// <summary>When the record was soft-deleted (UTC). Null if not deleted</summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>Who deleted the record (UserId or username). Null if not deleted</summary>
        [MaxLength(100)]
        public string? DeletedBy { get; set; }
    }
}
