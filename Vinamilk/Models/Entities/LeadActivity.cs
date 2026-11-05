using System.ComponentModel.DataAnnotations;

namespace Vinamilk.Models.Entities
{
    public class LeadActivity
    {
        [Key]
        public int Id { get; set; }

        public int LeadId { get; set; }
        public Lead? Lead { get; set; }

        [Required]
        public string ActivityType { get; set; } = string.Empty;

        public int PointsAwarded { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}