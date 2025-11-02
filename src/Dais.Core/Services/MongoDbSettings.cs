namespace DioRed.Dais.Core.Services;

public class MongoDbSettings
{
    public required string ConnectionString { get; init; }
    public required string DatabaseName { get; init; }
    public required CollectionNames Collections { get; init; }

    public class CollectionNames
    {
        public required string Applications { get; init; }
        public required string Clients { get; init; }
        public required string Users { get; init; }
    }
}