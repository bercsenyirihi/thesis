using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Drawing;
using Path = System.IO.Path;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Page2.xaml
    /// </summary>
    public partial class Page2 : Page
    {
        private ScottPlot.Plottable.ScatterPlot? MyScatterPlot = null;
        private ScottPlot.Plottable.MarkerPlot HighlightedPoint;
        private int LastHighlightedIndex = -1;
        double[] inp0;
        double[] inp1;
            
        public Page2()
        {
            InitializeComponent();
        }

        private void NumericOnly(System.Object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTextNumeric(e.Text);
        }


        private static bool IsTextNumeric(string str)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex("[^0-9]");
            return reg.IsMatch(str);
        }

        private async void cmbSymbol_DropDownOpened(object sender, EventArgs e)
        {
            while (Globals.ListOfSymbols.Count == 0) { };
            CmbSymbol.ItemsSource = Globals.ListOfSymbols;
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddMilliseconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            /* Vals:
             * Symbol ComboBox: CmbSymbol
             * Interval TextBox: IntervalTextBox
             * Interval ComboBox: CmbInterval
             * Length TextBlock: LengthTextBlock
             * Start Button: StartButton
             */
            // -> pass real values

            if (!(CmbSymbol.Text == "" || IntervalTextBox.Text == "" || CmbInterval.Text == "" || LengthTextBlock.Text == ""))
            {
                pyScript(CmbSymbol.Text + " " + IntervalTextBox.Text + " " + CmbInterval.Text + " " + LengthTextBlock.Text);
                List<List<double>> dataset = readCsv();
                // set size
                var plt = new ScottPlot.Plot(1000, 680);
                plt.Title(CmbSymbol.Text);
                List<DateTime> dates = new List<DateTime>();
                foreach (double d in dataset[0])
                {
                    dates.Add(UnixTimeStampToDateTime(d));
                }
                inp0 = dates.Select(x => x.ToOADate()).ToArray();
                inp1 = (double[])dataset[4].ToArray();
                draw();
            }
        }

        private async void MovingAverageButton_Click(object sender, RoutedEventArgs e)
        {
            /* Vals:
             * Symbol ComboBox: CmbSymbol
             * Interval TextBox: IntervalTextBox
             * Interval ComboBox: CmbInterval
             * Length TextBlock: LengthTextBlock
             * Average Button: AverageButton
             */
            // -> pass real values

            if (!(CmbSymbol.Text == "" || IntervalTextBox.Text == "" || CmbInterval.Text == "" || LengthTextBlock.Text == ""))
            {
                pyScript(CmbSymbol.Text + " " + IntervalTextBox.Text + " " + CmbInterval.Text + " " + LengthTextBlock.Text);
                List<List<double>> dataset = readCsv();
                List<DateTime> dates = new List<DateTime>();
                foreach (double d in dataset[0])
                {
                    dates.Add(UnixTimeStampToDateTime(d));
                }

                Popup popup = new Popup(Convert.ToInt32(LengthTextBlock.Text));
                popup.ShowDialog();
                string input = popup.PopupTextBox.Text;
                //no input
                if (input == "")
                {
                    Invalid invalid = new Invalid();
                    invalid.Show();
                }
                else
                {
                    //invalid input
                    if (Convert.ToInt32(input) > Convert.ToInt32(LengthTextBlock.Text))
                    {
                        Invalid invalid = new Invalid();
                        invalid.Show();
                    }
                    else
                    //valid input
                    {
                        inp0 = dates.Select(x => x.ToOADate()).ToArray();
                        inp0 = inp0.Skip(4).ToArray();
                        int periodLength = Convert.ToInt32(input);
                        inp1 = (double[])Enumerable
                            .Range(0, dataset[4].Count - periodLength)
                            .Select(n => dataset[4].Skip(n).Take(periodLength).Average())
                            .ToList().ToArray();
                        draw();
                    }
                }
            }
        }

        private async void AverageButton_Click(object sender, RoutedEventArgs e)
        {
            /* Vals:
             * Symbol ComboBox: CmbSymbol
             * Interval TextBox: IntervalTextBox
             * Interval ComboBox: CmbInterval
             * Length TextBlock: LengthTextBlock
             * Start Button: StartButton
             */
            // -> pass real values

            if (!(CmbSymbol.Text == "" || IntervalTextBox.Text == "" || CmbInterval.Text == "" || LengthTextBlock.Text == ""))
            {
                pyScript(CmbSymbol.Text + " " + IntervalTextBox.Text + " " + CmbInterval.Text + " " + LengthTextBlock.Text);
                List<List<double>> dataset = readCsv();
                // set size
                var plt = new ScottPlot.Plot(1000, 680);
                plt.Title(CmbSymbol.Text);
                List<DateTime> dates = new List<DateTime>();
                foreach (double d in dataset[0])
                {
                    dates.Add(UnixTimeStampToDateTime(d));
                }
                inp0 = dates.Select(x => x.ToOADate()).ToArray();
                inp1 = new double[inp0.Length];
                inp1[0] = dataset[4][0];
                for (int i = 1; i < inp0.Length; i++) 
                { 
                    inp1[i] = (inp1[i - 1] * i + dataset[4][i]) / (i + 1);
                }
                draw();
            }
        }

        public void draw()
        {
            wpfPlot.Plot.Clear();
            MyScatterPlot = wpfPlot.Plot.AddScatter(inp0, inp1);
            wpfPlot.Plot.XAxis.DateTimeFormat(true);

            // Add a red circle we can move around later as a highlighted point indicator
            HighlightedPoint = wpfPlot.Plot.AddPoint(0, 0);
            HighlightedPoint.Color = System.Drawing.Color.Red;
            HighlightedPoint.MarkerSize = 10;
            HighlightedPoint.MarkerShape = ScottPlot.MarkerShape.openCircle;
            HighlightedPoint.IsVisible = false;
            wpfPlot.Configuration.Pan = false;
            wpfPlot.Refresh();

            int minIndex = Array.IndexOf(inp1, inp1.Min());
            int maxIndex = Array.IndexOf(inp1, inp1.Max());
            /* Labels:
             * Label_Overall_MAX_Val
             * Label_Overall_MAX_Date
             * Label_Overall_MIN_Val
             * Label_Overall_MIN_Date
             * Lable_Highlited_Val
             * Lable_Highlited_Date
             */
            Label_Overall_MAX_Val.Content = inp1[maxIndex];
            Label_Overall_MAX_Date.Content = DateTime.FromOADate(inp0[maxIndex]);
            Label_Overall_MIN_Val.Content = inp1[minIndex];
            Label_Overall_MIN_Date.Content = DateTime.FromOADate(inp0[minIndex]);

        }

        private void wpfPlot_MouseMove(object sender, MouseEventArgs e)
        {
            if (MyScatterPlot != null)
            {
                // determine point nearest the cursor
                (double mouseCoordX, double mouseCoordY) = wpfPlot.GetMouseCoordinates();
                double xyRatio = wpfPlot.Plot.XAxis.Dims.PxPerUnit / wpfPlot.Plot.YAxis.Dims.PxPerUnit;
                (double pointX, double pointY, int pointIndex) = MyScatterPlot.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);

                // place the highlight over the point of interest
                HighlightedPoint.X = pointX;
                HighlightedPoint.Y = pointY;
                HighlightedPoint.IsVisible = true;

                // render if the highlighted point chnaged
                if (LastHighlightedIndex != pointIndex)
                {
                    LastHighlightedIndex = pointIndex;
                    wpfPlot.Render();
                }
                Lable_Highlited_Val.Content = inp1[pointIndex];
                Lable_Highlited_Date.Content = DateTime.FromOADate(inp0[pointIndex]);
            }
        }


        
        public void pyScript(string arg)
        {
            
            var command = AppDomain.CurrentDomain.BaseDirectory + @"venv\Scripts\python.exe" + " " + AppDomain.CurrentDomain.BaseDirectory + "main.py " + arg;
            ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = new Process())
            {
                proc.StartInfo = procStartInfo;
                proc.Start();
            }
            Trace.WriteLine(command);
        }

        private async void cmbInterval_DropDownOpened(object sender, EventArgs e)
        {
            string[] vals = { "M", "w", "d", "h", "m", "s" };
            List<string> valsList = new List<string>(vals);
            CmbInterval.ItemsSource = valsList;
        }

        private List<List<double>> readCsv()
        {
            try
            {

                //init dataset
                List<List<double>> dataset = new List<List<double>>();
                for (int i = 0; i <= 11; i++)
                    dataset.Add(new List<double>());

                string sourceFilePath = "./output.csv";
                // put in timeout
                while (!File.Exists(sourceFilePath))
                {
                    Thread.Sleep(1000);
                }
                if (File.Exists(sourceFilePath))
                {
                    Thread.Sleep(1000);
                    using (var reader = new StreamReader(sourceFilePath))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            var values = line.Split(',');
                            for (int i = 0; i <= 11; i++)
                                dataset[i].Add(Convert.ToDouble(values[i]));
                        }
                    }
                }
                return dataset;
            }
            catch (Exception ex)
            {
                return new List<List<double>>();
            }
        }
    }
}
