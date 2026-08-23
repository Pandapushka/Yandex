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

            // PBKDF2 (Identity PasswordHasher): формат "{iterations}.{salt}.{subkey}" в base64.
            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Role)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(50);
        }
    }
}
