using Microsoft.Extensions.Hosting;
using NuJournalPro.Models.Media;

namespace NuJournalPro.Models.Database
{
    public class PostImage : CompressedImage
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public string? NuJournalUserId { get; set; }

        // Database Navigation Properties
        public virtual Post? Post { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
    }
}
