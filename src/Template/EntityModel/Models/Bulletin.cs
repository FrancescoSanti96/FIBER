using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Template.EntityModel.Models
{
    public class Bulletin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey("User")]
        public int IdUser { get; set; }

        [MaxLength(255)]
        public string Summary { get; set; } = null;

        [Required]
        [ForeignKey("Province")]
        public int IdProvince { get; set; }

        [Required]
        [ForeignKey("Colture")]
        public int IdColture { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public bool Published { get; set; }

        public DateTime? PublishDate { get; set; }

        [Required]
        public DateOnly ExpireDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Province Province { get; set; } = null!;
        public virtual Colture Colture { get; set; } = null!;
    }
}
