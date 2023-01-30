using NuJournalPro.Models.Media;

namespace NuJournalPro.Models.Database
{
    public class ProfilePicture : CompressedImage
    {
        public int Id { get; set; }
        public string? NuJournalUserId { get; set; }

        // Database Navigation Properties
        public virtual NuJournalUser? NuJournalUser { get; set; }
    }
}
