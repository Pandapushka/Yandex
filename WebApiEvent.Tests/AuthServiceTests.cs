using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebApiEvent.Application.DTOs.Auth;
using WebApiEvent.Application.Interfaces;
using WebApiEvent.Application.Services;
using WebApiEvent.Domain.Enums;
using WebApiEvent.Domain.Exceptions;
using WebApiEvent.Infrastructure.Persistence;
using WebApiEvent.Infrastructure.Repositories;
using WebApiEvent.Infrastructure.Security;
using WebApiEvent.Infrastructure.Security.Models;

namespace WebApiEvent.Tests
{
    public class AuthServiceTests
    {
        private readonly IServiceProvider _serviceProvider;

        public AuthServiceTests()
        {
            var dbName = Guid.NewGuid().ToString();
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(dbName));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddSingleton(Options.Create(new JwtOptions
            {
                Key = "this-is-a-very-long-secret-key-for-jwt-hs256-at-least-32-characters",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                LifetimeMinutes = 30
            }));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IAuthService, AuthService>();
            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task RegisterAsync_ValidUser_SavesHashedPassword()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            await auth.RegisterAsync(new RegisterRequest("user1", "password123"), UserRole.User);

            var user = db.Users.Single(u => u.Login == "user1");
            user.PasswordHash.Should().NotBe("password123");
            user.Role.Should().Be(UserRole.User);
        }

        [Fact]
        public async Task RegisterAsync_DuplicateLogin_ThrowsValidationException()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

            await auth.RegisterAsync(new RegisterRequest("dupe", "password123"), UserRole.User);

            Func<Task> act = async () =>
                await auth.RegisterAsync(new RegisterRequest("dupe", "password123"), UserRole.User);
            await act.Should().ThrowAsync<CustomValidationException>();
        }

        [Fact]
        public async Task RegisterAsync_ShortPassword_ThrowsValidationException()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

            Func<Task> act = async () =>
                await auth.RegisterAsync(new RegisterRequest("user", "123"), UserRole.User);
            await act.Should().ThrowAsync<CustomValidationException>();
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await auth.RegisterAsync(new RegisterRequest("user1", "password123"), UserRole.User);

            var result = await auth.LoginAsync(new LoginRequest("user1", "password123"));

            result.Token.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();
            await auth.RegisterAsync(new RegisterRequest("user1", "password123"), UserRole.User);

            Func<Task> act = async () =>
                await auth.LoginAsync(new LoginRequest("user1", "wrong-password"));
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }

        [Fact]
        public async Task LoginAsync_NonExistentUser_ThrowsInvalidCredentialsException()
        {
            using var scope = _serviceProvider.CreateScope();
            var auth = scope.ServiceProvider.GetRequiredService<IAuthService>();

            Func<Task> act = async () =>
                await auth.LoginAsync(new LoginRequest("ghost", "password123"));
            await act.Should().ThrowAsync<InvalidCredentialsException>();
        }
    }
}
