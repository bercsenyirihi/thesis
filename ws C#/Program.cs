using CSharpTest.Net.Commands;
using MongoDB.Driver;


namespace Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            new CommandInterpreter(new Commands()).Run(args);
        }

        /// <summary>
        /// Class <c>Commands</c> processes command line arguments
        /// </summary>
        public class Commands
        {
            /// <summary>
            /// Method <c>init</c> processes the init value (read in from command line argument)
            /// </summary>
            public void init(int value)
            {
                Console.WriteLine("WS: init = " + value);
                StartAll(value);
            }
        }

        /// <summary>
        /// Class <c>StartAll</c> Starts a limited predefined amount of WS processes
        /// </summary>
        public static void StartAll(int initcount)
        {
            try
            {
#if RUNTIMETEST
            Console.WriteLine("WS: TimeTest ENABLED");
#endif
                // drop db
                var db_settings = new mongo_commands_database();
                // so it gets done, otherwise it is not completed
                var db_wait = db_settings.DropCollections();
                List<List<string>> symbols = my_api_request.getsymbols();
                List<Action> actions = new List<Action>();
                int maxnum;
                if (initcount < 0)
                {
                    maxnum = symbols.Count();
                }
                else
                {
                    maxnum = initcount;
                }
                int i = 0;
                foreach (List<string> symbol in symbols)
                {
                    if (i < maxnum)
                    {
                        actions.Add(() => new my_websocket_ms().ws_starter(symbol));
                    }
                    i = i + 1;
                }

                Parallel.Invoke(actions.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught exception: " + e.Message);
            }

        }

    }


}