using H.Pipes;
using PipeMessages;

namespace Example
{
    public class AppPipeClient : IDisposable
    {
        const string pipeName = "MyPipeToWPF";

        private static AppPipeClient instance;
        private PipeClient<PipeMessage> client;

        public static AppPipeClient Instance
        {
            get
            {
                return instance ?? new AppPipeClient();
            }
        }

        private AppPipeClient()
        {
            instance = this;
        }
        public async Task InitializeAsync()
        {
            if (client != null && client.IsConnected)
                return;
            Console.WriteLine("APP: PipeClient started");
            client = new PipeClient<PipeMessage>(pipeName);
            client.Disconnected += (o, args) => Console.WriteLine("APP: Disconnected from server");
            client.Connected += (o, args) => Console.WriteLine("APP: Connected to server");
            client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);
            await client.ConnectAsync();
        }

        public async Task ReturnBaseTokenList(string BaseTokenList)
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.ReturnBaseTokenList,
                List = BaseTokenList
            });
        }

        public async Task ReturnQuoteTokenList(string BaseTokenList)
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.ReturnQuoteTokenList,
                List = BaseTokenList
            });
        }

        public async Task ReturnCurrentData(string row)
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.ReturnCurrentData,
                List = row
            });
        }

        private void OnExceptionOccurred(Exception exception)
        {
            Console.WriteLine($"APP: An exception occured: {exception}");
        }

        public void Dispose()
        {
            if (client != null)
                client.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}