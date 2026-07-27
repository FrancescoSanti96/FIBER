using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Template.EntityModel.Models
{
    public class Colture
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        // Navigation properties
        public virtual ICollection<User> SubscribedUsers { get; set; } = [];
        public virtual ICollection<Bulletin> Bulletins { get; set; } = [];
    }
}
