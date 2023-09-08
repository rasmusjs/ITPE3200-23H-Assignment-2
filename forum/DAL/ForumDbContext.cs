using Microsoft.EntityFrameworkCore;

namespace forum.Models;

public class ForumDbContext : DbContext
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.CommentParent) // Comment can have one parent
            .WithMany(c => c.CommentReplies) // Can have many children
            .HasForeignKey(c => c.ParentCommentId) // Foreign key to represent the parent-child relationship
            .IsRequired(false);
    }


/*
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId);
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany(c => c.Posts).HasForeignKey(p => p.CategoryId);
        modelBuilder.Entity<Post>().HasMany(p => p.Tags).WithMany(t => t.Posts);
        modelBuilder.Entity<Comment>().HasOne(c => c.User).WithMany(u => u.CommentReplies).HasForeignKey(c => c.UserId);
        modelBuilder.Entity<Comment>().HasOne(c => c.Post).WithMany(p => p.CommentReplies).HasForeignKey(c => c.PostId);
        modelBuilder.Entity<Comment>().HasOne(c => c.Parent).WithMany(c => c.CommentReplies).HasForeignKey(c => c.ParentId);
    }*/
}