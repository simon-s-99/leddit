using System;
using System.ComponentModel.DataAnnotations;

namespace post.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public int UserId { get; set; }
    }
}
