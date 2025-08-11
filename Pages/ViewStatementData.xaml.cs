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
using System.Windows.Shapes;
using WPFBMI.Models;
using static WPFBMI.Pages.TrackReportDialog;

namespace WPFBMI.Pages
{
    /// <summary>
    /// Interaction logic for ViewStatementData.xaml
    /// </summary>
    public partial class ViewStatementData : Page
    {

        public ObservableCollection<UsageRecord> Items { get; set; }
        public ObservableCollection<SelectionItem> StatementList { get; set; }
        public List<EFStatement> Statements { get; set; }
        public List<EFUsage> Usages { get; set; }
        public List<EFTrack> Tracks { get; set; }
        public List<RootTrack> RootTracks { get; set; }
        public int currentStatement {  get; set; }

        public ViewStatementData()
        {
            InitializeComponent();
            InitializeCollections();
            this.DataContext = this;
            StatementSelector.ItemsSource = StatementList;
            GetData();
            PopulateStatementList();
            PopulateUsageList();
        }

        public void InitializeCollections()
        {
            Statements = new();
            Usages = new();
            Tracks = new();
            RootTracks = new();
            Items = new();
            StatementList = new();
        }

        public async void GetData()
        {
            Statements = await EFData.GetStatementsList();
            Usages = await EFData.GetUsagesList();
            Tracks = await EFData.GetTrackList();
            RootTracks = await EFData.GetRootTrackList();
        }

        public async void PopulateStatementList()
        {
            StatementList.Clear();
            foreach (var statement in Statements)
            {
                SelectionItem item = new SelectionItem();
                item.name = PeriodNameToString(statement.period);
                item.period = statement.period;
                item.id = statement.id;
                StatementList.Add(item);
            }
            if (StatementList.Count > 0)
            {
                StatementList[StatementList.Count - 1].isSelected = true;
                currentStatement = StatementList[StatementList.Count - 1].id;
            }
        }

        public string PeriodNameToString(string period)
        {
            if (period.Length == 5)
            {
                string year = period.Substring(0, 4);
                string monthInt = period.Substring(4, 1);
                return year + " Q" + monthInt;
            }
            else { return period; }
        }

        public async void PopulateUsageList()
        {
            Items.Clear();
            List<EFUsage> usages = Usages.Where(s => s.statement_id == currentStatement).ToList();
            foreach (EFUsage usage in usages)
            {
                UsageRecord record = new();
                record.royalty_amount = (Decimal)usage.royalty_amount;
                int minutes = usage.timing_secs / 60;
                int seconds = usage.timing_secs % 60;
                StringBuilder timing = new();
                if (minutes >= 100) { timing.Append(minutes); } else
                {
                    timing.Append(minutes > 9 ? "0" + minutes.ToString() : "00" + minutes.ToString());
                }
                timing.Append(':');
                timing.Append(seconds > 9 ? seconds.ToString() : "0" + seconds.ToString());
                if (timing.ToString() == "000:00") { timing.Clear(); }
                record.airTime = timing.ToString();
                record.perfCounts = usage.perf_counts;
                record.rootTrack = usage.track != null ? usage.track.root_track: null;
                record.track = usage.track;
                record.show = usage.show;
                record.channel = usage.channel;
                record.country = usage.country;
                record.trackName = record.track != null ? record.track.title_name : "";
                record.rootTrackName = record.rootTrack != null ? record.rootTrack.trackName : "";
                record.showName = record.show != null ? record.show.show_name : "";
                record.channelName = record.channel != null ? record.channel.source_name : "";
                record.countryName = record.country != null ? record.country.country_name : "";
                Items.Add(record);
            }
        }

        public async void UpdateSelection(object sender, EventArgs e)
        {
            var selectedStatement = (SelectionItem)StatementSelector.SelectedItem;
            string currentStatementName = selectedStatement.period;
            currentStatement = Statements.FirstOrDefault(s => s.period == currentStatementName).id;
            PopulateUsageList();
        }

        public class UsageRecord : INotifyPropertyChanged
        {
            public string trackName { get; set; }
            public string rootTrackName {  get; set; }
            public EFShow show { get; set; }
            public EFChannel channel { get; set; }
            public EFTrack track {  get; set; }
            public EFCountry country { get; set; }
            public RootTrack rootTrack { get; set; }
            public Decimal royalty_amount {  get; set; }
            public string airTime { get; set; }
            public int perfCounts {  get; set; }
            public string channelName {  get; set; }
            public string showName { get; set; }
            public string countryName { get; set; }
            public string TrackName { get => trackName; set {  trackName = value; OnPropertyChanged(nameof(TrackName)); } }
            public string RootTrackName { get => rootTrackName; set { rootTrackName = value; OnPropertyChanged(nameof(RootTrackName)); } }
            public string ShowName { get => showName; set { showName = value; OnPropertyChanged(nameof(ShowName)); } }
            public string CountryName { get => countryName; set { CountryName = value; OnPropertyChanged(nameof(CountryName)); } }
            public string ChannelName { get => channelName; set { channelName = value; OnPropertyChanged(nameof(ChannelName)); } }
            public string AirTime { get => airTime; set { airTime = value; OnPropertyChanged(nameof(AirTime)); } }
            public int PerfCounts { get => perfCounts; set { perfCounts = value; OnPropertyChanged(nameof(PerfCounts)); } }


            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class SelectionItem : INotifyPropertyChanged
        {
            public string _name { get; set; }
            public string _period { get; set; }
            public bool _isSelected { get; set; }
            public int id { get; set; }
            public string name
            {
                get => _name;
                set
                {
                    _name = value;
                    OnPropertyChanged(nameof(name));
                }
            }
            public string period
            {
                get => _period;
                set
                {
                    _period = value;
                    OnPropertyChanged(nameof(period));
                }
            }
            public bool isSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(isSelected));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
