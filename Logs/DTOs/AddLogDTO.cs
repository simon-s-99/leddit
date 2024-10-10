using System.ComponentModel.DataAnnotations;

namespace Logs.DTOs
{
    public class AddLogDTO
    {
        [Required]
        public string Body { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
