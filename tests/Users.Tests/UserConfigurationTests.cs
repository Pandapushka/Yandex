using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Users.Domain.Entities;
using Users.Infrastructure.Persistence;

namespace Users.Tests
{
    public class UserConfigurationTests
    {
        [Fact]
        public void Login_ShouldHaveUniqueIndex()
        {
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var entityType = db.Model.FindEntityType(typeof(User))!;
            var loginIndex = entityType.FindProperty(nameof(User.Login))!
                .GetContainingIndexes().Single();

            loginIndex.IsUnique.Should().BeTrue();
        }
    }
}
