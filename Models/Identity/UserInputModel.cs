using NuJournalPro.Models.Database;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace NuJournalPro.Models.Identity
{
    public class UserInputModel
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }

        [Display(Name = "Middle Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 1)]
        public string? MiddleName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

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

        [Required]
        [Display(Name = "Public Display Name")]
        [StringLength(128, ErrorMessage = "The {0} ust be at least {2} and no more than {1} characters long.", MinimumLength = 2)]
        public string? DisplayName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email / Username")]
        public string? Email { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [StringLength(128, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }
        public ProfilePicture? ProfilePicture { get; set; }
        public IFormFile? ImageFile { get; set; }

        [Display(Name = "Joined")]
        [DataType(DataType.DateTime)]
        public DateTime? Joined { get; set; }

        [Display(Name = "Modified")]
        [DataType(DataType.DateTime)]
        public DateTime? Modified { get; set; }

        // The username that created this user.
        public string? CreatedByUser { get; set; }

        // The user role that created this user.
        public List<string>? CreatedByRoles { get; set; }
        public string? CreatedByRolesString
        {
            get
            {
                if (CreatedByRoles is not null)
                {
                    return string.Join(", ", CreatedByRoles);
                }
                else
                {
                    return null;
                }
            }
        }

        // The username that modified this user last time.
        public string? ModifiedByUser { get; set; }

        // The user role that modified this user last time.
        public List<string>? ModifiedByRoles { get; set; }

        // Returns a string containing all user roles that this user has.
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
    }
}
