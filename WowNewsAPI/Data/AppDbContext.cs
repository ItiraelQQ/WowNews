using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WowNewsAPI.Models;

namespace WowNewsAPI.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<News> News { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<News>(entity =>
            {
                entity.HasKey(n => n.Id);
                entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
                entity.Property(n => n.Content).IsRequired();
                entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Content).IsRequired();
                entity.HasOne(c => c.User)
                      .WithMany() // Assuming the inverse relationship is not needed
                      .HasForeignKey(c => c.UserId)
                      .OnDelete(DeleteBehavior.Restrict); // Prevent cascading deletes if necessary

                entity.HasOne(c => c.News)
                      .WithMany() // Assuming the inverse relationship is not needed
                      .HasForeignKey(c => c.NewsId)
                      .OnDelete(DeleteBehavior.Cascade); // Or Restrict, depending on the desired behavior
            });
        }


    }

}
