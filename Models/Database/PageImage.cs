using Microsoft.AspNetCore.Mvc.RazorPages;
using NuJournalPro.Models.Media;

namespace NuJournalPro.Models.Database
{
    public class PageImage : CompressedImage
    {
        public int Id { get; set; }
        public int PageId { get; set; }
        public string? NuJournalUserId { get; set; }

        // Database Navigation Properties
        public virtual Page? Page { get; set; }
        public virtual NuJournalUser? NuJournalUser { get; set; }
    }
}
