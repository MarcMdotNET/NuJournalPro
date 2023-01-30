using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Models.Database
{
    public class Post
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string? NuJournalUserId { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Abstract")]
        [StringLength(1024, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Content")]
        [MinLength(64, ErrorMessage = "The {0} must be at least {1} characters long.")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Created")]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [Display(Name = "Modified")]
        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; }

        public PostStatus PostStatus { get; set; } = PostStatus.Draft;
        public PostVisibility PostVisibility { get; set; } = PostVisibility.Public;
        public string? Slug { get; set; }

        // Database Navigation Properties        
        public virtual Blog? Blog { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
        public virtual PostImage? PostImage { get; set; }
        public virtual ICollection<Tag> Tags { get; set; } = new HashSet<Tag>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
