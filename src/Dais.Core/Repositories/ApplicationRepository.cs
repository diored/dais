using DioRed.Dais.Core.Dto;

using MongoDB.Driver;

namespace DioRed.Dais.Core.Repositories;

internal interface IApplicationRepository
{
    string Add(string ownerId, string name, string[] callbacks);
    ApplicationDto? Find(string ownerId, string callback);
}

internal class MongoApplicationRepository(
    IMongoDatabase db,
    string collectionName
) : IApplicationRepository
{
    private readonly IMongoCollection<ApplicationDto> _applicationsCollection = db.GetCollection<ApplicationDto>(collectionName);

    public string Add(string ownerId, string name, string[] callbacks)
    {
        string id = IdGenerator.Generate();

        ApplicationDto dto = new()
        {
            Id = id,
            OwnerId = ownerId,
            Name = name,
            Callbacks = callbacks
        };

        _applicationsCollection.InsertOne(dto);

        return id;
    }

    public ApplicationDto? Find(string ownerId, string callback)
    {
        var ownerFilter = Builders<ApplicationDto>.Filter.Eq(app => app.OwnerId, ownerId);
        var callbackFilter = Builders<ApplicationDto>.Filter.AnyEq(app => app.Callbacks, callback);

        return _applicationsCollection.Find(ownerFilter & callbackFilter).FirstOrDefault();
    }
}