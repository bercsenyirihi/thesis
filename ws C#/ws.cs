using MongoDB.Driver;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Threading.Tasks.Dataflow;
using Websocket.Client;

namespace Example
{
    public class my_websocket_ms
    {
        public void ws_starter(List<string> symbol)
        {
            string wss = "wss://stream.binance.com:9443/ws/" + symbol[0].ToLower() + symbol[1].ToLower() + "@depth@100ms";
            var msglist = new BufferBlock<string>();

            try
            {
                Parallel.Invoke(() => ws_runner(wss, msglist), () => updater(symbol, msglist));
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception in starter " + symbol[0] + symbol[1] + ": " + e.Message);
            }

        }

        public void ws_runner(string wss, BufferBlock<string> msglist)
        {
            Console.CursorVisible = false;
            try
            {
                var exitEvent = new ManualResetEvent(false);
                var url = new Uri(wss);

                var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
                {
                    Options =
                            {
                                KeepAliveInterval = TimeSpan.FromSeconds(5),

                            }
                });

                using (var client = new WebsocketClient(url))
                {
                    client.ReconnectTimeout = TimeSpan.FromSeconds(30);
                    client.ReconnectionHappened.Subscribe(info =>
                    {
                        //Console.WriteLine("Reconnection happened, type: " + info.Type);
                    });
                    client.MessageReceived.Subscribe(msg =>
                    {
                        msglist.SendAsync(msg.ToString());
                    });
                    client.Start();
                    exitEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.ToString());
            }
            Console.ReadKey();
        }
        public async void updater(List<string> symbol, BufferBlock<string> msglist)
        {
            try
            {
                // to reset running if err is encountered
                // maybe add a counter to not loop in an error?
                while (true)
                {
                    Console.WriteLine("WS: init started " + symbol[0] + symbol[1]);
                    // timings for checking if incoming socket info is up to date or one was skipped
                    double? lastUpdateId = null;
                    // mongo db connections
                    var mydbbids = new mongo_commands_collection(symbol[1] + "_" + symbol[0]);
                    var mydbasks = new mongo_commands_collection(symbol[0] + "_" + symbol[1]);
                    // drop table for init state
                    await mydbbids.DropDocuments();
                    await mydbasks.DropDocuments();
                    Console.WriteLine("WS: init finished " + symbol[0] + symbol[1]);

                    while (true)
                    {
                        dynamic json = JsonConvert.DeserializeObject(await msglist.ReceiveAsync());
                        if (lastUpdateId == null)
                        {
                            string tmp = await my_api_request.getinitialdata(symbol[0] + symbol[1]);
                            if (string.IsNullOrEmpty(tmp?.ToString()))
                                break;
                            dynamic initdata = JsonConvert.DeserializeObject(tmp);
                            lastUpdateId = initdata!["lastUpdateId"];
                            // create list to store commands for bulk write
                            var listInitialBids = new List<WriteModel<CryptoData>>();
                            var listInitialAsks = new List<WriteModel<CryptoData>>();
                            foreach (dynamic i in initdata["bids"])
                            {
#if RUNTIMETEST
                                await mydbbids.UpsertAsync(i[0].ToString(), new CryptoData(i[0].ToString(), i[1].ToString(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())));
#else
                                await mydbbids.Upsert(i[0].ToString(), new CryptoData(i[0].ToString(), i[1].ToString()));
#endif
                            }
                            foreach (dynamic i in initdata["asks"])
                            {
#if RUNTIMETEST
                                await mydbasks.UpsertAsync(i[0].ToString(), new CryptoData(i[0].ToString(), i[1].ToString(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString())));
#else
                                await mydbasks.Upsert(i[0].ToString(), new CryptoData(i[0].ToString(), i[1].ToString()));
#endif
                            }
                        }
                        if (Convert.ToDouble(json!["U"]) <= lastUpdateId + 1 && Convert.ToDouble(json["u"]) >= lastUpdateId + 1)
                        {
                            if (lastUpdateId + 1 != Convert.ToDouble(json["U"]))
                            {
                                Console.WriteLine("WS: err " + symbol[0] + symbol[1]);
                                break;
                                // should reset the table and restart the whole process
                            }
                            else
                            {
                                // from bids sessionS
                                lastUpdateId = json["u"];
                                // create list to store commands for bulk write
                                var listUpdateBids = new List<WriteModel<CryptoData>>();
                                var listUpdateAsks = new List<WriteModel<CryptoData>>();
                                foreach (dynamic i in json["b"])
                                {
                                    // if the quantity is 0, delete is called (no duplicate id)
                                    if (i[1].ToString() == "0.00000000")
                                    {
                                        //delete data from db by id
                                        var filter = Builders<CryptoData>.Filter;
                                        string price_string = i[0].ToString();
                                        await mydbbids.Remove(price_string);
                                        //listUpdateBids.Add(new DeleteOneModel<CryptoData>(filter.Eq(x => x._id, price_string)));
                                    }
                                    // if the quantity is not 0, update is called (no duplicate id) -> what if new id is needed?
                                    else
                                    {
                                        //update data in db
                                        var filter = Builders<CryptoData>.Filter;
                                        var update = Builders<CryptoData>.Update;
                                        string price_string = i[0].ToString();
                                        string quantity_string = i[1].ToString();
#if RUNTIMETEST
                                        await mydbbids.UpsertAsync(price_string, new CryptoData(price_string, quantity_string, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()));
#else
                                        await mydbbids.Upsert(price_string, new CryptoData(price_string, quantity_string));
#endif
                                    }
                                }
                                // from asks session
                                foreach (dynamic i in json["a"])
                                {
                                    if (i[1].ToString() == "0.00000000")
                                    {
                                        //delete data from db by id
                                        var filter = Builders<CryptoData>.Filter;
                                        string price_string = i[0].ToString();
                                        await mydbasks.Remove(price_string);
                                        //listUpdateAsks.Add(new DeleteOneModel<CryptoData>(filter.Eq(x => x._id, price_string)));
                                    }
                                    else
                                    {
                                        //update data in db
                                        var filter = Builders<CryptoData>.Filter;
                                        var update = Builders<CryptoData>.Update;
                                        string price_string = i[0].ToString();
                                        string quantity_string = i[1].ToString();
#if RUNTIMETEST
                                        await mydbasks.UpsertAsync(price_string, new CryptoData(price_string, quantity_string, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()));
#else
                                        await mydbasks.Upsert(price_string, new CryptoData(price_string, quantity_string));
#endif
                                    }
                                }
                                // exec if there is something to exec
                                if (listUpdateBids.Count != 0)
                                {
                                    //Console.WriteLine("exec bid");
                                    //await mydbbids.BulkCreateAsync(listUpdateBids);
                                }
                                if (listUpdateAsks.Count != 0)
                                {
                                    //Console.WriteLine("exec ask");
                                    //await mydbasks.BulkCreateAsync(listUpdateAsks);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR in updater: " + ex.ToString());
            }
        }
    }
}
