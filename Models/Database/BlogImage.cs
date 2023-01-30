using NuJournalPro.Models.Media;
using System.Reflection.Metadata;

namespace NuJournalPro.Models.Database
{
    public class BlogImage : CompressedImage
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string? NuJournalId { get; set; }

        // Database Navigation Properties
        public virtual Blog? Blog { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
    }
}
