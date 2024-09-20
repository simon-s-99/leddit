using System;
using System.ComponentModel.DataAnnotations;

namespace post.Models
{
    public class Post
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public Guid UserId { get; set; }
    }
}
