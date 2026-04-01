using System;
using System.ComponentModel.DataAnnotations;

namespace SmartCourseManagement.API.Models
{
    /// <summary>
    /// Abstract base class for all entities. Provides audit fields and soft-delete support.
    /// All entities inherit from this to get consistent tracking across the application.
    /// </summary>
    public abstract class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>UTC timestamp when the record was created. Auto-set by AppDbContext.SaveChangesAsync.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>UTC timestamp when the record was last updated. Auto-set by AppDbContext.SaveChangesAsync.</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Email of the user who created this record. Auto-set from JWT claims.</summary>
        public string? CreatedBy { get; set; }

        /// <summary>Email of the user who last updated this record. Auto-set from JWT claims.</summary>
        public string? UpdatedBy { get; set; }

        /// <summary>Soft-delete flag. When true the record is hidden from all queries.</summary>
        public bool IsDeleted { get; set; }
    }
}
