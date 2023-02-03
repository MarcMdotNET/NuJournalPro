using NuJournalPro.Models.Database;
using System.ComponentModel.DataAnnotations;

namespace NuJournalPro.Models.Identity
{
    public class UserInfo
    {
        [Display(Name = "Email / Username")]
        public string? UserName { get; set; }
        
        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "First Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? MiddleName { get; set; }

        [Display(Name = "Last Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]        
        public string? LastName { get; set; }

        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [Display(Name = "Full Name")]
        public string? FullNameMiddleInitial { get; set; }
        
        [Display(Name = "Public Display Name")]
        public string? DisplayName { get; set; }        
        
        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }
        
        [Display(Name = "Profile Picture")]
        public ProfilePicture? ProfilePicture { get; set; }
        
        public List<string>? UserRoles { get; set; }
        
        public string? UserRolesString
        {
            get
            {
                if (UserRoles == null) return null;
                else return string.Join(", ", UserRoles);
            }
        }        
    }
}