using Xunit;

namespace CrateSync.Platform.Api.IntegrationTests.Infrastructure;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiIntegrationCollection
    : ICollectionFixture<CustomWebApplicationFactory>
{
    public const string Name = "CrateSync API integration";
}
