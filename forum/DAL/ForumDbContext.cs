using forum.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace forum.DAL;

public class ForumDbContext : IdentityDbContext
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options)
    {
        //Database.EnsureCreated();
    }

    //public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<Post> Posts { get; set; }

    //public DbSet<PostTag> PostTags { get; set; }
    public DbSet<Comment> Comments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.Entity<PostTag>().HasKey(pt => new { pt.PostId, pt.TagId });


        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTag"));


        //https://learn.microsoft.com/en-us/ef/core/modeling/relationships/many-to-many
        // Link Posts and Tags using the help table PostTag

        modelBuilder.Entity<Post>()
            .HasMany(p => p.Tags)
            .WithMany(t => t.Posts)
            .UsingEntity(j => j.ToTable("PostTag"));


        // Link Posts with Categories
        modelBuilder.Entity<Post>().HasOne(p => p.Category).WithMany().HasForeignKey(p => p.CategoryId);


        // Link Comments with Posts
        //modelBuilder.Entity<Comment>().HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId);


        modelBuilder.Entity<Comment>()
            .HasMany(c => c.CommentReplies)
            .WithOne(c => c.ParentComment)
            .HasForeignKey(c => c.ParentCommentId);
        //modelBuilder.Entity<Post>().HasOne(p => p.User).WithMany(u => u.Posts).HasForeignKey(p => p.UserId);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
    }
}