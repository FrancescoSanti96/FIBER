using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Template.EntityModel.Models
{
    public class Province
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(10)]
        public string Code { get; set; }

        // Navigation properties
        public virtual ICollection<UserSetting> UserSettings { get; set; } = new List<UserSetting>();
        public virtual ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
    }
}
