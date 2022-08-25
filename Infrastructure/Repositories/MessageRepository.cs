using Core;
using Core.Entities;
using MongoDB.Driver;

namespace Infrastructure.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        private readonly MongoClient _mongoClient;

        private readonly IMongoCollection<Tweet> _tweetsCollection;
        private readonly IMongoCollection<Reply> _repliesCollection;

        // todo: maybe to devide on two repos. But read about mongo drive. It needs a singliton
        public MessageRepository(string connectionString, string databaseName, string tweetCollectionName)
        {
            _mongoClient = new MongoClient(connectionString);
            var database = _mongoClient.GetDatabase(databaseName);
            _tweetsCollection = database.GetCollection<Tweet>(tweetCollectionName);
            _repliesCollection = database.GetCollection<Reply>("Reply");
        }

        public async Task<bool> TweetExistsAsync(string id, CancellationToken cancellationToken)
        {
            return await _tweetsCollection
                .Find(p => p.Id == id)
                .AnyAsync(cancellationToken);
        }

        public async Task CreateAsync(Tweet tweet, CancellationToken cancellationToken) =>
            await _tweetsCollection.InsertOneAsync(
                tweet,
                new InsertOneOptions { BypassDocumentValidation = false },
                cancellationToken);

        public async Task AddReplyAsync(Reply reply, CancellationToken cancellationToken) =>
            await _repliesCollection.InsertOneAsync(
                reply,
                new InsertOneOptions { BypassDocumentValidation = false },
                cancellationToken);
    }
}
