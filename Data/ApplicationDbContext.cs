using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NuJournalPro.Models.Database;

namespace NuJournalPro.Data
{
    public class ApplicationDbContext : IdentityDbContext<NuJournalUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Page>? Pages { get; set; }
        public DbSet<Blog>? Blogs { get; set; }
        public DbSet<Post>? Posts { get; set; }
        public DbSet<Comment>? Comments { get; set; }
        public DbSet<Tag>? Tags { get; set; }
        public DbSet<PageImage>? PageImage { get; set; }
        public DbSet<BlogImage>? BlogImage { get; set; }
        public DbSet<PostImage>? PostImage { get; set; }
        public DbSet<ProfilePicture>? ProfilePicture { get; set; }
    }
}