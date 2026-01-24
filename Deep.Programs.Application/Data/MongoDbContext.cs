using Deep.Programs.Application.Features.ProgramStatistics;
using MongoDB.Driver;

namespace Deep.Programs.Application.Data;

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