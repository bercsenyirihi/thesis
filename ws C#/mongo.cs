using MongoDB.Bson;
using MongoDB.Driver;

namespace Example
{
    public class mongo_commands_collection
    {
        private readonly IMongoCollection<CryptoData> _orderBook;

        private static class MongoDBSettings
        {
            public static String ConnectionString = "mongodb://localhost:27017";
            public static String DatabaseName = "OrderBook";
            public static String? CollectionName;
        }

        public mongo_commands_collection(String CollectionName)
        {
            MongoDBSettings.CollectionName = CollectionName;
            var mongoClient = new MongoClient(
                MongoDBSettings.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                MongoDBSettings.DatabaseName);

            _orderBook = mongoDatabase.GetCollection<CryptoData>(
                CollectionName);

        }

        public async Task DropDocuments() =>
            await _orderBook.DeleteManyAsync("{}");

        public async Task Upsert(String price, CryptoData updatedBook) =>
            await _orderBook.ReplaceOneAsync(filter: new BsonDocument("_id", price), options: new ReplaceOptions { IsUpsert = true },
                replacement: updatedBook);
        public async Task Remove(String price) =>
            await _orderBook.DeleteOneAsync(x => x._id == price);
    }

    public class mongo_commands_database
    {
        private readonly IMongoDatabase mongoDatabase;
        private readonly IMongoClient mongoClient;
        private static class MongoDBSettings
        {
            public static String ConnectionString = "mongodb://localhost:27017";
            public static String DatabaseName = "OrderBook";
        }

        public mongo_commands_database()
        {
            mongoClient = new MongoClient(MongoDBSettings.ConnectionString);

            mongoDatabase = mongoClient.GetDatabase(
                MongoDBSettings.DatabaseName);

        }

        public async Task DropCollections() =>
            await mongoClient.DropDatabaseAsync("OrderBook");

    }
}