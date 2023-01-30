using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum PageStatus
    {
        [Description("Draft")]
        Draft,
        [Description("Pending Review")]
        PendingReview,
        [Description("Published")]
        Published,
        [Description("Archived")]
        Archived
    }
}
