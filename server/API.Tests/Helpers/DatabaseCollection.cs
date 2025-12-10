namespace API.Tests.Helpers;

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
{
    // Created just to define collection for tests that need database to attach fixture
    // Makes it so all tests using test container run sequentially
}