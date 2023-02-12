using H.Pipes;
using H.Pipes.Args;
using PipeMessages;


namespace Example
{
    public class PipeServer_Runner
    {
        public static void runner()
        {
            AppPipeServer pipeServer = new AppPipeServer();
            AppPipeClient.Instance.InitializeAsync().ContinueWith(t => Console.WriteLine($"Error while connecting to pipe server: {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);
            pipeServer.InitializeAsync().GetAwaiter().GetResult();
            while (true)
            {
                Task.Delay(50).GetAwaiter().GetResult();
            }
        }
    }
    public class AppPipeServer : IDisposable
    {
        const string PIPE_NAME = "MyPipeToApp";

        private PipeServer<PipeMessage>? server;

        public AppPipeServer()
        {

        }

        public async Task InitializeAsync()
        {
            Console.WriteLine("APP: PipeServer started");
            server = new PipeServer<PipeMessage>(PIPE_NAME);

            server.ClientConnected += async (o, args) => await OnClientConnectedAsync(args);
            server.ClientDisconnected += (o, args) => OnClientDisconnected(args);
            server.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
            server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await server.StartAsync();
        }

        private async Task OnClientConnectedAsync(ConnectionEventArgs<PipeMessage> args)
        {
            Console.WriteLine($"APP: Client {args.Connection.Id} is now connected!");
        }

        private void OnClientDisconnected(ConnectionEventArgs<PipeMessage> args)
        {
            Console.WriteLine($"APP: Client {args.Connection.Id} disconnected");
            Environment.Exit(0);
        }

        private async void OnMessageReceived(PipeMessage? message)
        {
            if (message == null)
                return;

            switch (message.Action)
            {
                case ActionType.GetBaseTokenList:
                    Console.WriteLine("APP: PipeServer.GetBaseTokenList");
                    string tableNames = string.Join(",", sqlite_commands.ReadTableNames(Globals.TransformedConnection!));
                    await AppPipeClient.Instance.ReturnBaseTokenList(tableNames);
                    break;

                case ActionType.GetQuoteTokenList:
                    Console.WriteLine("APP: PipeServer.GetBaseTokenList." + message.BaseToken);
                    string rowNames = string.Join(",", sqlite_commands.ReadTransfomedRowNames(Globals.TransformedConnection!, message.BaseToken));
                    await AppPipeClient.Instance.ReturnQuoteTokenList(rowNames);
                    break;

                case ActionType.GetCurrentData:
                    Console.WriteLine("APP: PipeServer.GetCurrentData." + message.QuoteToken + "_" + message.BaseToken);
                    string row = string.Join(",", sqlite_commands.ReadTransformedRow(Globals.TransformedConnection!, message.BaseToken, message.QuoteToken));
                    await AppPipeClient.Instance.ReturnCurrentData(row);
                    break;

                default:
                    Console.WriteLine("Action not recognised: " + message.Action);
                    break;
            }
        }

        private void OnExceptionOccurred(Exception ex)
        {
            Console.WriteLine($"Exception occured in pipe: {ex}");
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async Task DisposeAsync()
        {
            if (server != null)
                await server.DisposeAsync();
        }
    }
}
