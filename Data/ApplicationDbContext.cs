using GameLoggd.Models;
using GameLoggd.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GameLoggd.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserFollow> UserFollows => Set<UserFollow>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<UserGameLog> UserGameLogs => Set<UserGameLog>();
    public DbSet<ReviewLike> ReviewLikes => Set<ReviewLike>();
    public DbSet<UserList> UserLists => Set<UserList>();
    public DbSet<UserListItem> UserListItems => Set<UserListItem>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Platform> Platforms => Set<Platform>();

    public DbSet<ReviewComment> ReviewComments => Set<ReviewComment>();
    public DbSet<UserListLike> UserListLikes => Set<UserListLike>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<UserFavoriteGame> UserFavoriteGames => Set<UserFavoriteGame>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Game>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Developer).HasMaxLength(200);
            b.Property(x => x.Description).HasMaxLength(4000);
            b.Property(x => x.ImagePath).HasMaxLength(500);
            b.Property(x => x.Slug).HasMaxLength(250).IsRequired();
            b.HasIndex(x => x.Title);
            b.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<UserFollow>(b =>
        {
            b.HasKey(x => new { x.ObserverId, x.TargetId });
            
            b.HasOne(x => x.Observer)
                .WithMany()
                .HasForeignKey(x => x.ObserverId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Target)
                .WithMany()
                .HasForeignKey(x => x.TargetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Review>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ReviewLike>(b =>
        {
            b.HasKey(x => new { x.ReviewId, x.UserId });
        });

        builder.Entity<ReviewComment>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Review)
                .WithMany(r => r.Comments)
                .HasForeignKey(x => x.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);
            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            b.Property(x => x.Content).HasMaxLength(2000).IsRequired();
        });

        builder.Entity<UserListLike>(b =>
        {
            b.HasKey(x => new { x.UserListId, x.UserId });
            b.HasOne(x => x.UserList)
                .WithMany(l => l.Likes)
                .HasForeignKey(x => x.UserListId)
                .OnDelete(DeleteBehavior.Cascade);
             b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Genre>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            b.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<Platform>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Slug).HasMaxLength(100).IsRequired();
            b.Property(x => x.IconClass).HasMaxLength(100);
            b.HasIndex(x => x.Slug).IsUnique();
        });

        builder.Entity<Message>(b =>
        {
            b.HasKey(x => x.Id);
            b.HasOne(x => x.Sender)
                .WithMany()
                .HasForeignKey(x => x.SenderId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade cycles
            b.HasOne(x => x.Receiver)
                .WithMany()
                .HasForeignKey(x => x.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<UserFavoriteGame>(b =>
        {
            b.HasKey(x => new { x.UserId, x.Slot });

            b.Property(x => x.Slot).IsRequired();
            b.Property(x => x.CreatedAt).IsRequired();

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.Game)
                .WithMany()
                .HasForeignKey(x => x.GameId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(x => new { x.UserId, x.GameId }).IsUnique();
        });
    }
}
