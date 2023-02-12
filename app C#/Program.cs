using Microsoft.Data.Sqlite;
using System.Threading.Tasks.Dataflow;
using System.Diagnostics;

namespace Example
{
    public static class Globals
    {
        public static SqliteConnection? OrderbookConnection { get; set; }
        public static SqliteConnection? TransformedConnection { get; set; }
        public static Process? wsprocess { get; set; }
    }
    public class Program
    {
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, EventArgs) => CurrentDomain_ProcessExit();
            Thread.GetDomain().UnhandledException += (sender, EventArgs) => CurrentDomain_ProcessExit();
            const string connectionStringOrderbook = "Data Source=InMemorySample;Mode=Memory";
            //const string connectionStringOrderbook = "Data Source=hello1.db";
            Globals.OrderbookConnection = new SqliteConnection(connectionStringOrderbook);
            Globals.OrderbookConnection.Open();

            const string connectionStringTransformed = "Data Source=InMemorySample;Mode=Memory";
            //const string connectionStringTransformed = "Data Source=hello2.db";
            Globals.TransformedConnection = new SqliteConnection(connectionStringTransformed);
            Globals.TransformedConnection.Open();

            Console.WriteLine("APP: starting");

#if RUNTIMETEST
            Console.WriteLine("APP: TimeTest ENABLED");
#endif

            var buffer = new BufferBlock<InternalMessage>();
            List<Action> actions = new List<Action>();
            actions.Add(() => mongo_commands.read_all_collections(buffer));
            actions.Add(() => new orderbook_reader(buffer));
            actions.Add(() => transformer.Processor());
            actions.Add(() => PipeServer_Runner.runner());
            Parallel.Invoke(actions.ToArray());
        }

        public static void CurrentDomain_ProcessExit()
        {
            Globals.wsprocess.Kill();
        }
    }


}