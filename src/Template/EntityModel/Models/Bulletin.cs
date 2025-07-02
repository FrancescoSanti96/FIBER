using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

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

        public string Body { get; set; } = null;

        [Required]
        public bool Published { get; set; }

        public DateTime? PublishDate { get; set; }

        public DateOnly? ExpireDate { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Province> Provinces { get; set; } = [];
        public virtual ICollection<Colture> Coltures { get; set; } = [];
    }
}
