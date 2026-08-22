using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApiEvent.Domain.Entities;

namespace WebApiEvent.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();

            builder.Property(u => u.Login)
                .IsRequired()
                .HasMaxLength(256);

            // Уникальный индекс на логин — защита от дубликатов на уровне БД.
            builder.HasIndex(u => u.Login).IsUnique();

            // SHA-256 даёт 32 байта → 64 символа в hex-строке.
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}
