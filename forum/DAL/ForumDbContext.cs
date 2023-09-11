using Microsoft.EntityFrameworkCore;

namespace forum.Models;

public class ForumDbContext : DbContext
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
        //ChangeTracker.LazyLoadingEnabled = true;
        //Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
        // Link Posts and Tags using the help table PostTag
        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTag"));

        // Link Posts with Categories
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);


        //modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId);
        /*modelBuilder.Entity<Comment>()
            .HasOne(c => c.CommentParent) // Comment can have one parent
            .WithMany(c => c.CommentReplies) // Can have many children
            .HasForeignKey(c => c.ParentCommentId) // Foreign key to represent the parent-child relationship
            .IsRequired(false);*/
    }
}