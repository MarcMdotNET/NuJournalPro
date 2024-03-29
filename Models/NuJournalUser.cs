﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;
using Microsoft.Extensions.Hosting;
using System.Reflection.Metadata;
using NuJournalPro.Models.Database;

namespace NuJournalPro.Models
{
    public class NuJournalUser : IdentityUser
    {
        // Personal Information
        [Required]
        [Display(Name = "First Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Display(Name = "Middle Name or Initial")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 1)]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(MiddleName))
                {
                    return $"{FirstName} {LastName}";
                }
                else if (MiddleName.Length == 1)
                {
                    return $"{FirstName} {MiddleName}. {LastName}";
                }
                else
                {
                    return $"{FirstName} {MiddleName} {LastName}";
                }
            }
        }

        [NotMapped]
        public string FullNameMiddleInitial
        {
            get
            {
                if (string.IsNullOrEmpty(MiddleName))
                {
                    return $"{FirstName} {LastName}";
                }
                else if (MiddleName.Length == 1)
                {
                    return $"{FirstName} {MiddleName}. {LastName}";
                }
                else
                {
                    return $"{FirstName} {MiddleName[0]}. {LastName}";
                }
            }
        }

        // Public Display Name
        [Required]
        [Display(Name = "Public Display Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string DisplayName { get; set; } = string.Empty;

        // User Join - Date and Time
        [Display(Name = "Joined")]
        [DataType(DataType.DateTime)]
        public DateTime Joined { get; set; } = DateTime.UtcNow;

        // User Modified - Date and Time
        [Display(Name = "Modified")]
        [DataType(DataType.DateTime)]
        public DateTime Modified { get; set; } = DateTime.UtcNow;

        // The username that created this user.
        public string CreatedByUser { get; set; } = "User Registration";

        // The user role that created this user.
        public List<string> CreatedByRoles { get; set; } = new List<string>() { "Application" };

        // Returns a string containing all user roles that created this user.
        [NotMapped]
        public string CreatedByRolesString
        {
            get
            {
                return string.Join(", ", CreatedByRoles);
            }
        }

        // The username that modified this user last time.
        public string? ModifiedByUser { get; set; }
        // The user role that modified this user last time.
        public List<string>? ModifiedByRoles { get; set; }

        // The user role(s) that modified this user last time.
        [NotMapped]
        public string? ModifiedByRolesString
        {
            get
            {
                if (ModifiedByRoles != null)
                {
                    return string.Join(", ", ModifiedByRoles);
                }
                else
                {
                    return null;
                }
            }
        }

        // User Roles String
        [Display(Name = "User Role")]
        public List<string> UserRoles { get; set; } = new List<string>();

        // Returns a string containing all user roles that this user has.
        [NotMapped]
        public string UserRolesString
        {
            get
            {
                if (UserRoles != null)
                {
                    return string.Join(", ", UserRoles);
                }
                else
                {
                    return "No Role";
                }
            }
        }

        // UserName with UserRoles combined
        [NotMapped]
        public string UserNameWithRoles
        {
            get
            {
                return $"{UserName} ({UserRolesString})";
            }
        }

        // UserName with Complete Information
        [NotMapped]
        public string UserNameWithDetails
        {
            get
            {
                return $"{UserName} [{UserRolesString}] - {FullNameMiddleInitial} ({DisplayName})";
            }
        }

        // Social Media Links
        [Display(Name = "GitHub Repository")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? GitHubUrl { get; set; }

        [Display(Name = "Twitter Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? TwitterUrl { get; set; }

        [Display(Name = "LinkedIn Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? LinkedInUrl { get; set; }

        [Display(Name = "YouTube Channel")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? YouTubeUrl { get; set; }

        [Display(Name = "Facebook Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? FacebookUrl { get; set; }

        [Display(Name = "Instagram Profile")]
        [StringLength(256, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        [Url]
        public string? InstagramUrl { get; set; }

        // Database Navigation Properties
        public virtual ProfilePicture? ProfilePicture { get; set; }
        public virtual ICollection<Page> Pages { get; set; } = new HashSet<Page>();
        public virtual ICollection<Blog> Blogs { get; set; } = new HashSet<Blog>();
        public virtual ICollection<Post> Posts { get; set; } = new HashSet<Post>();
    }
}
