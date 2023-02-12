using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Net;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace WpfApp1
{

    public static class Globals
    {
        public static BufferBlock<string>? PipeMessageList;
        public static bool isRunning = true;
        public static IList<string> ListOfSymbols = new List<string>();
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Process appprocess = new Process();
        Thread pipeThread;
        Thread symbolThread;
        public bool isAlive { get; set; } = true;
        WelcomePage welcomePage = new WelcomePage();
        Page1 page1 = new Page1();
        Page2 page2 = new Page2();
        public MainWindow()
        {           
            InitializeComponent();
            Main.Content = welcomePage;
            inits();
        }

        public void inits()
        {
            appprocess.StartInfo.FileName = "./app C#.exe";
            appprocess.StartInfo.UseShellExecute = false;
            appprocess.StartInfo.CreateNoWindow = false;
            appprocess.StartInfo.CreateNoWindow = true;
            appprocess.Start();

            Globals.PipeMessageList = new BufferBlock<string>();

            InterfacePipeClient.Instance.InitializeAsync().ContinueWith(t => Console.WriteLine($"Error while connecting to pipe server: {t.Exception}"), TaskContinuationOptions.OnlyOnFaulted);

            pipeThread = new Thread(() => PipeServer_Runner.runner());
            pipeThread.Start();

            symbolThread = new Thread(() => GetSymbols());
            symbolThread.Start();
        }

        private void BtnClickWelcome(object sender, RoutedEventArgs e)
        {
            Main.Content =welcomePage;
        }

        private void BtnClickPage1(object sender, RoutedEventArgs e)
        {
            Main.Content = page1;
        }

        private void BtnClickPage2(object sender, RoutedEventArgs e)
        {
            Main.Content = page2;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Globals.isRunning = false;
        }

        public static void GetSymbols()
        {
            try
            {
                string json = new WebClient().DownloadString("https://api.binance.com/api/v3/exchangeInfo");

                dynamic items = JsonConvert.DeserializeObject(json);
                List<string> ret = new List<string>();
                foreach (var item in items.symbols)
                {
                    ret.Add(item.symbol.ToString());
                }
                Globals.ListOfSymbols = ret;
            }
            catch (Exception e)
            {
                Trace.WriteLine("GetSymbols: " + e.Message);
            }

        }
    }
}
