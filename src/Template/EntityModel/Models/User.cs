using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Template.Cryptography;

namespace Template.EntityModel.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        public DateTime? LastLoggedAt { get; set; }

        [Required]
        [ForeignKey("Role")]
        public int RoleId { get; set; }

        // Navigation properties
        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<Colture> Coltures { get; set; } = [];
        public virtual ICollection<Province> Provinces { get; set; } = [];
        public virtual ICollection<Bulletin> Bulletins { get; set; } = [];

        public bool IsMatchWithPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return PasswordHasher.Verify(password, Password);
        }
    }
}
