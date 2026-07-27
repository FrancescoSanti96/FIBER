using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Template.EntityModel.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = [];
    }
}
