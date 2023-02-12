using H.Pipes;
using H.Pipes.Args;
using PipeMessages;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WpfApp1
{
    public class PipeServer_Runner
    {
        public static void runner()
        {
            Trace.WriteLine("STARTED");
            InterfacePipeServer pipeServer = new InterfacePipeServer();
            pipeServer.InitializeAsync().GetAwaiter().GetResult();
            while (Globals.isRunning)
            {
                Task.Delay(50).GetAwaiter().GetResult();
            }
        }
    }
    public class InterfacePipeServer : IDisposable
    {
        const string PIPE_NAME = "MyPipeToWPF";

        private PipeServer<PipeMessage>? server;

        public InterfacePipeServer()
        {

        }

        public async Task InitializeAsync()
        {
            server = new PipeServer<PipeMessage>(PIPE_NAME);

            server.ClientConnected += async (o, args) => await OnClientConnectedAsync(args);
            server.ClientDisconnected += (o, args) => OnClientDisconnected(args);
            server.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
            server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);
            await server.StartAsync();
        }

        private async Task OnClientConnectedAsync(ConnectionEventArgs<PipeMessage> args)
        {
            Trace.WriteLine($"Client {args.Connection.Id} is now connected!");
        }

        private void OnClientDisconnected(ConnectionEventArgs<PipeMessage> args)
        {
            Trace.WriteLine($"Client {args.Connection.Id} disconnected");
        }

        private void OnMessageReceived(PipeMessage? message)
        {
            if (message == null)
                return;

            switch (message.Action)
            {
                case ActionType.ReturnBaseTokenList:
                    Globals.PipeMessageList!.SendAsync(message.List);
                    break;

                case ActionType.ReturnQuoteTokenList:
                    Globals.PipeMessageList!.SendAsync(message.List);
                    break;

                case ActionType.ReturnCurrentData:
                    Trace.WriteLine(message.List);
                    Globals.PipeMessageList!.SendAsync(message.List);
                    break;


                default:
                    Trace.WriteLine("Not Implemented");
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
