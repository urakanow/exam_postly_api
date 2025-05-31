using exam_postly_api.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace exam_postly_api
{
    public class ApplicationDBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<Favorite> Favorites { get; set; }

        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasKey(user => user.Id);

            modelBuilder.Entity<RefreshToken>().HasKey(refreshToken => refreshToken.Id);
            modelBuilder.Entity<RefreshToken>()
                .HasOne(refreshToken => refreshToken.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(refreshToken => refreshToken.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Offer>().HasKey(offer => offer.Id);
            modelBuilder.Entity<Offer>()
                .HasOne(offer => offer.User)
                .WithMany(user => user.Offers)
                .HasForeignKey(offer => offer.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Image>().HasKey(image => image.Id);
            modelBuilder.Entity<Image>()
                .HasOne(image => image.Offer)
                .WithMany(offer => offer.Images)
                .HasForeignKey(image => image.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Favorite>().HasKey(favor => favor.Id);
            modelBuilder.Entity<Favorite>()
                .HasOne(favor => favor.User)
                .WithMany(user => user.Favorites)
                .HasForeignKey(favor => favor.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Favorite>()
                .HasOne(favor => favor.Offer)
                .WithMany(offer => offer.Favorites)
                .HasForeignKey(favor => favor.OfferId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
