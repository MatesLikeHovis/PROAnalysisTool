using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using WPFBMI.Models;

namespace WPFBMI.Pages
{
    /// <summary>
    /// Interaction logic for PieChartPage.xaml
    /// </summary>
    public partial class ChartPage : Page, INotifyPropertyChanged
    {
        private PlotModel _model;

        public PlotModel model
        {
            get => _model;
            private set
            {
                _model = value;
                OnPropertyChanged(nameof(model));
            }
        }
        public string sourceType { get; set; }
        public string sourceName {  get; set; }
        public List<KeyValuePair<string, double>> data { get; set; }



        public ChartPage(string SourceType, string SourceName)
        {
            this.sourceName = SourceName;
            this.sourceType = SourceType;
            data = new();
            InitializeComponent();
            this.DataContext = this;
            FetchData();
        }

        public async void FetchData()
        {
            await GetData();
            await RefreshCharts();
        }

        public async Task GetData()
        {
            List<EFUsage> usages = await EFData.GetUsageQuery(new List<string> { sourceType }, new List<string> { sourceName });
            MessageBox.Show("Query Completed");
            Dictionary<string, double> values = new();
            foreach (EFUsage usage in usages)
            {
                string readablePeriod = GetReadablePeriod(usage.statement.period);
                if (values.ContainsKey(readablePeriod)) 
                {
                    values[readablePeriod] += usage.royalty_amount;
                } else
                {
                    values[readablePeriod] = usage.royalty_amount;
                }
            }
            data.Clear();
            foreach (string key in values.Keys)
            { 
                KeyValuePair<string, double> pair = new(key, values[key]);
                data.Add(pair);
            }
        }

        public string GetReadablePeriod(string period)
        {
            StringBuilder response = new();
            response.Append(period.Substring(0, 4));
            response.Append(" ");
            response.Append($"Q{period.Substring(4, 1)}");
            return response.ToString();
        }

        public async Task RefreshCharts()
        {
            this.model = new PlotModel { Title = $"{sourceName}" };
            var statementAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Statements"
            };
            foreach (var item in this.data)
            {
                statementAxis.Labels.Add(item.Key);
            }

            if (data.Count > 16)
            {
                statementAxis.LabelFormatter = (labelIndex) =>
                {
                    if (labelIndex >= 0 && labelIndex < statementAxis.Labels.Count)
                    {
                        var fullLabel = statementAxis.Labels[(int)labelIndex]; // e.g., "2022 Q1"
                        if (fullLabel.Contains("Q1"))
                        {
                            return fullLabel.Split(' ')[0]; // return "2022"
                        }
                    }
                    return ""; // Empty for Q2/Q3/Q4
                };
            }

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Royalty Amount",
                MinimumPadding = 0.1,
                AbsoluteMinimum = 0
            };
            var series = new LineSeries
            {
                Title = "Values",
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColors.DarkBlue,
                MarkerFill = OxyColors.LightBlue
            };
            for (int i = 0; i < data.Count; i++)
            {
                series.Points.Add(new DataPoint(i, data[i].Value));
            }
            this.model.Axes.Add(statementAxis);
            this.model.Axes.Add(valueAxis);
            this.model.Series.Add(series);
            this.model.InvalidatePlot(true);
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
