using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BloggingApp.Models
{
    public class Comment
    {
        public Comment()
        {
            CreationDateTime = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public User User { get; set; }

        [Required]
        public Post Post { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime CreationDateTime { get; set; }
    }
}