using Testcontainers.PostgreSql;

namespace EventApi.IntegrationTests
{
    public class PostgreSqlFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _container;

        public string ConnectionString => _container.GetConnectionString();

        public PostgreSqlFixture()
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16")
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _container.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _container.DisposeAsync();
        }
    }
}