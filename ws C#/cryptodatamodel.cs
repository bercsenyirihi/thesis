using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Example
{

    public class CryptoData
    {
#if RUNTIMETEST
        public CryptoData(string a, string b, string c)
        {
            _id = a;
            Quantity = b;
            writetime = c;
        }
#else
        public CryptoData(string a, string b)
        {
            _id = a;
            Quantity = b;
        }
#endif

        [BsonElement("Price")]
        public string? _id { get; set; }

        public string? Quantity { get; set; }

#if RUNTIMETEST
        public string? writetime { get; set; }
#endif
    }
}