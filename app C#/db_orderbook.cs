using System.Threading.Tasks.Dataflow;


namespace Example
{
    public class orderbook_reader
    {
        BufferBlock<InternalMessage> buffer;
        public orderbook_reader(BufferBlock<InternalMessage> buffer)
        {
            Console.WriteLine("APP: orderbook started");
            this.buffer = buffer;
            msg_processor();
        }
        public async void msg_processor()
        {
            while (true)
            {
                InternalMessage msg = await this.buffer.ReceiveAsync();
                switch (msg.optype)
                {
                    // Drops table
                    case "drop":
                        break;
                    // Drops database
                    case "dropDatabase":
                        break;
                    // gets called with dropdatabase, right after it
                    case "invalidate":
                        break;
                    case "insert":
                        sqlite_commands.CreateTableOrderBook(Globals.OrderbookConnection!, msg.collectionname!);
                        sqlite_commands.UpsertOrderBook(Globals.OrderbookConnection!, msg.collectionname!, msg._id!, msg.quantity!);
                        break;
                    case "delete":
                        sqlite_commands.DeleteData(Globals.OrderbookConnection!, msg.collectionname!, msg._id!);
                        break;
                    case "update":
                        sqlite_commands.UpsertOrderBook(Globals.OrderbookConnection!, msg.collectionname!, msg._id!, msg.quantity!);
                        break;
                    default:
                        break;
                }
            }
        }
    }


}