using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Policy;
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
using WPFBMI.Models;

namespace WPFBMI.Pages
{
    /// <summary>
    /// Interaction logic for CategoryReportPage.xaml
    /// </summary>
    public partial class CategoryReportPage : Page
    {
        public ObservableCollection<DisplayItem> Items { get; set; }

        private string _category;
        private List<EFUsage> usages;
        private List<EFStatement> statements;
        private List<int> Last10Statements;
        private List<int> Last4Statements;
        private Dictionary<int, string> RootTrackNamesByID;
        private int LastStatement;
        public CategoryReportPage(string category)
        {
            InitializeComponent();
            InitializeCollection();
            _category = category;
            this.DataContext = this;
            GetLastStatementsPerformance();
            GetDataSet();
            GetData();
        }

        public void GetLastStatementsPerformance()
        {
            GetLastStatementsLists();
        }

        public async void GetLastStatementsLists()
        {
            statements = await EFData.GetStatementsList();
            Last10Statements = new();
            Last4Statements = new();
            List<int> statementPeriods = new();
            foreach (var statement in statements)
            {
                statementPeriods.Add(statement.id);
            }
            statementPeriods.Sort();
            for (int i = 0; i < 10; i++)
            {
                if (statementPeriods.Count > i)
                {
                    if (i < 4) { Last4Statements.Add(statementPeriods[statementPeriods.Count - (i + 1)]); }
                    Last10Statements.Add(statementPeriods[statementPeriods.Count - (i + 1)]);
                    if (i == 0) { LastStatement = statementPeriods[statementPeriods.Count - (i + 1)]; }
                }
            }

        }

        public async void GetData()
        {
            Items.Clear();
            Dictionary<string, DisplayItem> itemDict = new();
            foreach (var usage in usages)
            {
                string catName = "";
                if (usage.track == null) { continue; }
                if (usage.track.root_track == null) { continue; }
                if (_category == "library" && usage.track.root_track.library == null) { continue; }
                if (_category == "genre" && usage.track.root_track.genre == null) { continue; }
                if (_category == "subgenre" && usage.track.root_track.subgenre == null) { continue; }
                if (_category == "album" && usage.track.root_track.album == null) { continue; }
                if (_category == "Country" && usage.country.country_name == null) { continue; }

                switch (_category)
                {
                    case "library":
                        catName = usage.track.root_track.library.name;
                        break;
                    case "genre":
                        catName = usage.track.root_track.genre.name;
                        break;
                    case "subgenre":
                        catName = usage.track.root_track.subgenre.name;
                        break;
                    case "album":
                        catName = usage.track.root_track.album.name;
                        break;
                    case "country":
                        catName = usage.country.country_name;
                        break;
                    default:
                        catName = "Error";
                        break;
                }
                Decimal Last1 = usage.statement_id == LastStatement ? (Decimal)usage.royalty_amount : 0;
                Decimal Last4 = Last4Statements.Contains(usage.statement_id) ? (Decimal)usage.royalty_amount : 0;
                Decimal Last10 = Last10Statements.Contains(usage.statement_id) ? (Decimal)usage.royalty_amount : 0;
                RootTrackNamesByID[(int)usage.track.root_id] = usage.track.root_track.trackName;
                if (itemDict.ContainsKey(catName))
                {
                    itemDict[catName].activeCount++;
                    itemDict[catName].amountLast1 += Last1;
                    itemDict[catName].amountLast4 += Last4;
                    itemDict[catName].amountLast10 += Last10;
                    itemDict[catName].amount += (Decimal)usage.royalty_amount;
                    if (itemDict[catName].TrackamountsbyCategory.ContainsKey((int)usage.track.root_id)) { itemDict[catName].TrackamountsbyCategory[(int)usage.track.root_id] += (Decimal)usage.royalty_amount; }
                    else { itemDict[catName].TrackamountsbyCategory[(int)usage.track.root_id] = (Decimal)usage.royalty_amount; }
                } else
                {
                    DisplayItem item = new();
                    item.name = catName;
                    item.activeCount = 1;
                    item.amountLast1 = Last1;
                    item.amountLast4 = Last4;
                    item.amountLast10 = Last10;
                    item.amount = (Decimal)usage.royalty_amount;
                    item.TrackamountsbyCategory = new();
                    item.TrackamountsbyCategory[(int)usage.track.root_id] = (Decimal)usage.royalty_amount;
                    itemDict[catName] = item;
                }
            }
            foreach (string category in itemDict.Keys)
            {
                Decimal amount = Decimal.MinValue;
                string trackName = "";
                foreach (int track_id in itemDict[category].TrackamountsbyCategory.Keys)
                {
                    Decimal thisAmount = itemDict[category].TrackamountsbyCategory[track_id];
                    if (thisAmount > amount)
                    {
                        amount = thisAmount;
                        trackName = RootTrackNamesByID[track_id];
                    }
                }
                itemDict[category].bestTrack = trackName;
                itemDict[category].bestTrackAmount = amount;
                Items.Add(itemDict[category]);
            }
        }

        public void InitializeCollection()
        {
            Items = new();
            RootTrackNamesByID = new();
        }

        public async void GetDataSet()
        {
            usages = await EFData.GetUsagesList();
        }


        public class DisplayItem : INotifyPropertyChanged
        {
            public string _name {  get; set; }
            public int _activeCount {  get; set; }
            public Decimal _amount { get; set; }
            public Decimal _amountLast4 { get; set; }
            public Decimal _amountLast10 {  get; set; }
            public Decimal _amountLast1 { get; set; }
            public string _bestTrack {  get; set; }
            public Decimal _bestTrackAmount { get; set; }
            public Dictionary<int, Decimal> TrackamountsbyCategory {  get; set; }
            public string name { get => _name; set {  _name = value; OnPropertyChanged(nameof(name)); } }
            public int activeCount { get => _activeCount; set { _activeCount = value; OnPropertyChanged(nameof(activeCount)); } }
            public Decimal amount { get => _amount; set { _amount = value; OnPropertyChanged(nameof(amount)); } }
            public Decimal amountLast1 { get => _amountLast1; set { _amountLast1 = value; OnPropertyChanged(nameof(amountLast1)); } }
            public Decimal amountLast4 { get => _amountLast4; set { _amountLast4 = value; OnPropertyChanged(nameof(amountLast4)); } }
            public Decimal amountLast10 { get => _amountLast10; set { _amountLast10 = value; OnPropertyChanged(nameof(amountLast10)); } }
            public string bestTrack { get => _bestTrack; set { _bestTrack = value; OnPropertyChanged(nameof(bestTrack)); } }
            public Decimal bestTrackAmount { get => _bestTrackAmount; set { _bestTrackAmount = value; OnPropertyChanged(nameof(bestTrackAmount)); } }


            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
