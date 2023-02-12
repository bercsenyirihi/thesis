namespace Example
{
    public class InternalMessage
    {
        public string? optype { get; set; }
        public string? dbname { get; set; }
        public string? collectionname { get; set; }
        public string? _id { get; set; }
        public string? quantity { get; set; }
#if RUNTIMETEST
        public long? servertime { get; set; }
        public long? writetime { get; set; }
        public long? processtime { get; set; }
        public string log()
        {
            switch (this.optype)
            {
                case "drop":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                case "dropDatabase":
                    return "optype: " + optype + "   dbname: " + dbname +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                case "invalidate":
                    return "optype: " + optype +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                case "insert":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id + "   quantity: " + quantity +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   write time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - writetime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                case "delete":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                case "update":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id + "   quantity: " + quantity +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   write time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - writetime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
                default:
                    return "optype: " + optype + "    NEW OPTYPE" +
                    "\nserver time diff: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - servertime) + "   process time: " + (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - processtime);
            }
        }
#else
        public string log()
        {
            switch (this.optype)
            {
                case "drop":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname;
                case "dropDatabase":
                    return "optype: " + optype + "   dbname: " + dbname;
                case "invalidate":
                    return "optype: " + optype;
                case "insert":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id + "   quantity: " + quantity;
                case "delete":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id;
                case "update":
                    return "optype: " + optype + "   dbname: " + dbname + "   collname: " + collectionname + "   _id: " + _id + "   quantity: " + quantity;
                default:
                    return "optype: " + optype + "    NEW OPTYPE";
            }
        }
#endif
    }
}