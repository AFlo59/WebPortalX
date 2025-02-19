using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebPortalX.Core.Models;

namespace WebPortalX.Infrastructure.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<UserManager>
    {
        public void Configure(EntityTypeBuilder<UserManager> builder)
        {
            builder.ToTable("Users");
            
            builder.HasKey(u => u.Id);
            
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(u => u.PasswordHash)
                .IsRequired();
            
            builder.Property(u => u.Role)
                .HasConversion<string>()
                .IsRequired();
        }
    }
} 