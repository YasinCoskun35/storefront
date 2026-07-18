namespace Storefront.IntegrationTests.Infrastructure;

/// <summary>
/// Forces all integration test classes into a single collection.
/// This means:
/// - All classes share ONE CustomWebApplicationFactory (one DB container, one test host)
/// - Tests across classes run SEQUENTIALLY, avoiding race conditions
/// </summary>
[CollectionDefinition("IntegrationTests")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
