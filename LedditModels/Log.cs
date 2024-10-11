using System.ComponentModel.DataAnnotations;

namespace LedditModels
{
    public class Log
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
