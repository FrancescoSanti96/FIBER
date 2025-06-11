using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Template.EntityModel.Models
{
    public class UserSetting
    {
        [Key]
        [ForeignKey("User")]
        public int IdUser { get; set; }

        [Key]
        [ForeignKey("Province")]
        public int IdProvince { get; set; }

        [Key]
        [ForeignKey("Colture")]
        public int IdColture { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Province Province { get; set; } = null!;
        public virtual Colture Colture { get; set; } = null!;
    }
}
