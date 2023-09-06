using Microsoft.EntityFrameworkCore;

namespace forum.Models;

public class PostDbContext : DbContext
{
    public PostDbContext(DbContextOptions<PostDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    public DbSet<Post> Posts { get; set; }


    /*public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId);
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany(c => c.Posts).HasForeignKey(p => p.CategoryId);
        modelBuilder.Entity<Post>().HasMany(p => p.Tags).WithMany(t => t.Posts);
        modelBuilder.Entity<Comment>().HasOne(c => c.User).WithMany(u => u.Comments).HasForeignKey(c => c.UserId);
        modelBuilder.Entity<Comment>().HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId);
        modelBuilder.Entity<Comment>().HasOne(c => c.Parent).WithMany(c => c.Comments).HasForeignKey(c => c.ParentId);
    }*/
}