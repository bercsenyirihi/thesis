using Newtonsoft.Json;
using System.Net;

namespace Example
{
    public class my_api_request
    {
        public static List<List<string>> getsymbols()
        {
            try
            {
                string json = new WebClient().DownloadString("https://api.binance.com/api/v3/exchangeInfo");

                dynamic items = JsonConvert.DeserializeObject(json);
                List<List<string>> ret = new List<List<string>>();
                foreach (var item in items.symbols)
                {
                    List<string> tmp = new List<string>();
                    tmp.Add(item.baseAsset.ToString());
                    tmp.Add(item.quoteAsset.ToString());
                    ret.Add(tmp);
                }
                return ret;
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught api exception: " + e.Message);
                return null;
            }

        }

        public static async Task<string?> getinitialdata(string symbol, int limit = 1000)
        {
            try
            {
                string? res = null;
                string path = "https://api.binance.com/api/v3/depth?symbol=" + symbol.ToUpper() + "&limit=" + limit.ToString();
                var client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(path);
                if (response.IsSuccessStatusCode)
                {
                    res = await response.Content.ReadAsStringAsync();
                }
                return res;

            }
            catch (Exception e)
            {
                Console.WriteLine("Caught api exception " + symbol + ": " + e.Message);
                return "";
            }
        }
    }
}