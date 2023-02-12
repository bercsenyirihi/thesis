using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks.Dataflow;

namespace Example
{
    class mongo_commands
    {
        public static async void read_all_collections(BufferBlock<InternalMessage> buffer)
        {
            Console.WriteLine("APP: mongo started");
            var client = new MongoClient("mongodb://localhost:27017");
            var db = client.GetDatabase("OrderBook");
            var collections = db.ListCollectionNames();

            List<Action> actions = new List<Action>();
            actions.Add(() => process_mongo(buffer, db));
            actions.Add(() => ws_starter.Start());
            Parallel.Invoke(actions.ToArray());
        }

        public static async void process_mongo(BufferBlock<InternalMessage> buffer, IMongoDatabase db)
        {
            Console.WriteLine("APP: mongo_processor started");
            try
            {
                while (true)
                {
                    using (var cursor = db.Watch())
                    {
                        foreach (var change in cursor.ToEnumerable())
                        {
                            string? optype = change.BackingDocument.ToBsonDocument()["operationType"].ToString();
                            var msg = new InternalMessage();
                            switch (optype)
                            {
                                // Drops table
                                case "drop":
                                    msg.optype = optype;
                                    msg.dbname = change.BackingDocument.ToBsonDocument()["ns"]["db"].ToString();
                                    msg.collectionname = change.BackingDocument.ToBsonDocument()["ns"]["coll"].ToString();
                                    await buffer.SendAsync(msg);
                                    break;

                                // Drops database
                                case "dropDatabase":
                                    msg.optype = optype;
                                    msg.dbname = change.BackingDocument.ToBsonDocument()["ns"]["db"].ToString();
                                    await buffer.SendAsync(msg);
                                    break;

                                // gets called with dropdatabase, right after it
                                case "invalidate":
                                    msg.optype = optype;
                                    await buffer.SendAsync(msg);
                                    break;

                                case "insert":
                                    msg.optype = optype;
                                    msg.dbname = change.BackingDocument.ToBsonDocument()["ns"]["db"].ToString();
                                    msg.collectionname = change.BackingDocument.ToBsonDocument()["ns"]["coll"].ToString();
                                    msg._id = change.BackingDocument.ToBsonDocument()["documentKey"]["_id"].ToString();
                                    msg.quantity = change.BackingDocument.ToBsonDocument()["fullDocument"]["Quantity"].ToString();
                                    await buffer.SendAsync(msg);
                                    break;

                                case "delete":
                                    msg.optype = optype;
                                    msg.dbname = change.BackingDocument.ToBsonDocument()["ns"]["db"].ToString();
                                    msg.collectionname = change.BackingDocument.ToBsonDocument()["ns"]["coll"].ToString();
                                    msg._id = change.BackingDocument.ToBsonDocument()["documentKey"]["_id"].ToString();
                                    await buffer.SendAsync(msg);
                                    break;

                                case "update":
                                    msg.optype = optype;
                                    msg.dbname = change.BackingDocument.ToBsonDocument()["ns"]["db"].ToString();
                                    msg.collectionname = change.BackingDocument.ToBsonDocument()["ns"]["coll"].ToString();
                                    msg._id = change.BackingDocument.ToBsonDocument()["documentKey"]["_id"].ToString();
                                    msg.quantity = change.BackingDocument.ToBsonDocument()["updateDescription"]["updatedFields"]["Quantity"].ToString();
                                    await buffer.SendAsync(msg);
                                    break;

                                default:
                                    msg.optype = optype;
                                    await buffer.SendAsync(msg);
                                    break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("APP: Caught exception: " + e.Message);
            }

        }
    }
}