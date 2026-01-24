using Deep.Programs.Features.ProgramStatistics;
using MongoDB.Driver;

namespace Deep.Programs.Data;

public sealed class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IMongoDatabase database)
    {
        _database = database;
    }

    public IMongoCollection<ProgramStatistic> ProgramStatistics =>
        _database.GetCollection<ProgramStatistic>("program-statistics");
}