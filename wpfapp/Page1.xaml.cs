using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Threading;
using System.Diagnostics;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        DispatcherTimer updateTimer = new DispatcherTimer();
        double[] liveData = new double[200];
        string baseToken;
        string quoteToken;
        double samplesPerDay = (24 * 60 * 60 * 20);


        public Page1()
        {
            InitializeComponent();

            updateTimer.Tick += updateGraph;
            updateTimer.Tick += updateDataSet;
            updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            var sig = wpfPlot1.Plot.AddSignal(liveData, sampleRate: samplesPerDay);
            sig.OffsetX = DateTime.Now.ToOADate();
            wpfPlot1.Plot.AxisAutoX(margin: 0);
            wpfPlot1.Plot.Title("");
            //disable drag
            wpfPlot1.Configuration.Pan = false;
            wpfPlot1.Plot.XAxis.DateTimeFormat(true);
            wpfPlot1.Refresh();
        }

        async void updateDataSet(object sender, EventArgs e)
        {
            Array.Copy(liveData, 1, liveData, 0, liveData.Length - 1);
            await InterfacePipeClient.Instance.GetCurrentData(baseToken, quoteToken);
            double nextValue;
            if (Double.TryParse(await Globals.PipeMessageList!.ReceiveAsync(), out nextValue))
            {
                liveData[liveData.Length - 1] = nextValue;
            }
        }

        async void updateGraph(object sender, EventArgs e)
        {
            wpfPlot1.Plot.Clear();
            var sig = wpfPlot1.Plot.AddSignal(liveData, sampleRate: samplesPerDay);
            sig.OffsetX = DateTime.Now.ToOADate();
            wpfPlot1.Plot.AxisAutoX(margin: 0);
            wpfPlot1.Plot.Title("");
            wpfPlot1.Plot.SetAxisLimits(yMin: liveData.Min() / 1.2, yMax: liveData.Max() * 1.2);
            //disable drag
            wpfPlot1.Configuration.Pan = false;
            wpfPlot1.Plot.XAxis.DateTimeFormat(true);
            wpfPlot1.Refresh();
        }

        private async void cmbBaseToken_DropDownOpened(object sender, EventArgs e)
        {
            updateTimer.Stop();
            wpfPlot1.Plot.Clear();
            liveData = new double[200];
            var sig = wpfPlot1.Plot.AddSignal(liveData, sampleRate: samplesPerDay);
            sig.OffsetX = DateTime.Now.ToOADate();
            wpfPlot1.Plot.AxisAutoX(margin: 0);
            wpfPlot1.Plot.Title("");
            //disable drag
            wpfPlot1.Configuration.Pan = false;
            wpfPlot1.Plot.XAxis.DateTimeFormat(true);
            wpfPlot1.Refresh();

            cmbQuoteToken.Text = "";
            StartButton.IsEnabled = false;
            cmbQuoteToken.IsEnabled = false;

            await InterfacePipeClient.Instance.GetBaseTokenList();
            string line = await Globals.PipeMessageList!.ReceiveAsync();
            if (!Double.TryParse(line, out double num))
            {
                List<string> listElements = line.Split(',').ToList();
                cmbBaseToken.ItemsSource = listElements;
                cmbQuoteToken.IsEnabled = true;
            }
            else
                cmbBaseToken_DropDownOpened(sender, e);
        }

        private async void cmbQuoteToken_DropDownOpened(object sender, EventArgs e)
        {
            StartButton.IsEnabled = false;
            await InterfacePipeClient.Instance.GetQuoteTokenList(cmbBaseToken.Text);
            string line = await Globals.PipeMessageList!.ReceiveAsync();
            cmbQuoteToken.ItemsSource = line.Split(',').ToList();
            StartButton.IsEnabled = true;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (cmbBaseToken.Text != "" && cmbQuoteToken.Text != "")
            {
                baseToken = cmbBaseToken.Text;
                quoteToken = cmbQuoteToken.Text;
                updateTimer.Start();
            }
        }
    }
}
