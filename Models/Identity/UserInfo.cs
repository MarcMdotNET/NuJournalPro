using NuJournalPro.Models.Database;

namespace NuJournalPro.Models.Identity
{
    public class UserInfo
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? FullNameMiddleInitial { get; set; }
        public string? DisplayName { get; set; }
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