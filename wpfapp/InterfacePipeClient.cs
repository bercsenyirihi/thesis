using H.Pipes;
using PipeMessages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class InterfacePipeClient : IDisposable
    {
        const string pipeName = "MyPipeToApp";

        private static InterfacePipeClient instance;
        private PipeClient<PipeMessage> client;

        public static InterfacePipeClient Instance
        {
            get
            {
                return instance ?? new InterfacePipeClient();
            }
        }

        private InterfacePipeClient()
        {
            instance = this;
        }
        public async Task InitializeAsync()
        {
            if (client != null && client.IsConnected)
                return;

            client = new PipeClient<PipeMessage>(pipeName);
            client.Disconnected += (o, args) => Trace.WriteLine("Disconnected from server");
            client.Connected += (o, args) => Trace.WriteLine("Connected to server");
            client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await client.ConnectAsync();
        }

        public async Task GetBaseTokenList()
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.GetBaseTokenList
            });
        }

        public async Task GetQuoteTokenList(string BaseToken)
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.GetQuoteTokenList,
                BaseToken = BaseToken
            });
        }

        public async Task GetCurrentData(string BaseToken, string QuoteToken)
        {
            await client.WriteAsync(new PipeMessage
            {
                Action = ActionType.GetCurrentData,
                BaseToken = BaseToken,
                QuoteToken = QuoteToken
            });
        }

        private void OnExceptionOccurred(Exception exception)
        {
            Trace.WriteLine($"An exception occured: {exception}");
        }

        public void Dispose()
        {
            if (client != null)
                client.DisposeAsync().GetAwaiter().GetResult();
        }
    }
}