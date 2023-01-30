using NuJournalPro.Enums;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Models.Database
{
    public class Page
    {
        public int Id { get; set; }
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

        public PageStatus PageStatus { get; set; } = PageStatus.Draft;
        public PageVisibility PageVisibility { get; set; } = PageVisibility.Public;
        public string? Slug { get; set; }

        // Database Navigation Properties
        public virtual NuJournalUser? NuJournalUser { get; set; }
        public virtual PageImage? PageImage { get; set; }
    }
}
