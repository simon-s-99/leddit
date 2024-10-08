using System.ComponentModel.DataAnnotations;

namespace LedditModels;

public class Post
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public Guid UserId { get; set; }
}