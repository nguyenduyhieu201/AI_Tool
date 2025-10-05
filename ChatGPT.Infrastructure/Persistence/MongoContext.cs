using ChatGPT.Domain.Models;
using MongoDB.Driver;
using Thread = ChatGPT.Domain.Models.Thread;

namespace ChatGPT.Infrastructure.Persistence;

public class MongoOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
}

public class MongoContext
{
    public IMongoDatabase Database { get; }
    public IMongoCollection<Thread> Threads => Database.GetCollection<Thread>("threads");
    public IMongoCollection<Message> Messages => Database.GetCollection<Message>("messages");

    public MongoContext(MongoOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        Database = client.GetDatabase(options.Database);
        EnsureIndexes();
    }

    private void EnsureIndexes()
    {
        // threads: { userId: 1, updatedAt: -1 }
        var threadsKeys = Builders<Thread>.IndexKeys
            .Ascending(x => x.UserId)
            .Descending(x => x.UpdatedAt);
        Threads.Indexes.CreateOne(new CreateIndexModel<Thread>(threadsKeys));

        // messages: { threadId: 1, createdAt: 1 }
        var messagesKeys = Builders<Message>.IndexKeys
            .Ascending(x => x.ThreadId)
            .Ascending(x => x.CreatedAt);
        Messages.Indexes.CreateOne(new CreateIndexModel<Message>(messagesKeys));
    }
}







