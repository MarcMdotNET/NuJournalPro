﻿using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;

namespace NuJournalPro.Models.Database
{
    public class Blog
    {
        public int Id { get; set; }
        public string? NuJournalUserId { get; set; }

        [Required]
        [Display(Name = "Title")]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        [StringLength(1024, ErrorMessage = "The {0} must be at least {2} and at most {1} characters lonf.", MinimumLength = 2)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Created")]
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; } = DateTime.UtcNow;

        [Display(Name = "Modified")]
        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; }
        
        // Database Navigation Properties
        [Display(Name = "Author")]
        public virtual NuJournalUser? NuJournalUser { get; set; }
        public virtual BlogImage? BlogImage { get; set; }
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}
