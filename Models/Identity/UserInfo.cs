namespace NuJournalPro.Models.Identity
{
    public class UserInfo
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
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