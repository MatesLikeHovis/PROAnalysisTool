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

namespace WPFBMI.Pages
{
    public partial class TrackReportDialog : Page
    {
        public ObservableCollection<SelectionItem> Libraries { get; set; }
        public ObservableCollection<SelectionItem> Albums { get; set; }
        public ObservableCollection<SelectionItem> Genres {  get; set; }
        public ObservableCollection<SelectionItem> Subgenres {  get; set; }
        public ObservableCollection<TrackItem> Tracks {  get; set; }
        public List<EFTrack> EFTrackList { get; set; }
        public List<RootTrack> RootTrackList {  get; set; }
        public List<EFUsage> EFUsageList { get; set; }
        public List<TrackInfo> AllTracks { get; set; }
        public List<Library> LibrariesList { get; set; }
        public List<Album> AlbumsList { get; set; }
        public List<Genre> GenresList { get; set; }
        public List<Subgenre> SubgenresList {  get; set; }
        public List<EFStatement> StatementsList {  get; set; }
        public List<int> Last4Statements { get; set; }
        public List<int> Last10Statements {  get; set; }
        public int LastStatement {  get; set; }
        public string currentStatementFilter {  get; set; }
        public TrackReportDialog()
        {
            InitializeComponent();
            this.DataContext = this;
            InitializeCollections();
            InitializeData();
            GetLastStatementsLists();
            ComputeTrackData();
            InitializeInterface();
            RefreshData();
        }

        public async void InitializeData()
        {
            EFTrackList = await EFData.GetTrackList();
            RootTrackList = await EFData.GetRootTrackList();
            EFUsageList = await EFData.GetUsagesList();
            LibrariesList = await EFData.GetLibrariesList();
            AlbumsList = await EFData.GetAlbumsList();
            GenresList = await EFData.GetGenresList();
            SubgenresList = await EFData.GetSubgenresList();
            StatementsList = await EFData.GetStatementsList();
            AllTracks = new();
        }

        public async void GetLastStatementsLists()
        {
            Last10Statements = new();
            Last4Statements = new();
            List<int> statementPeriods = new();
            foreach (var statement in StatementsList)
            {
                statementPeriods.Add(Int32.Parse(statement.period));
            }
            statementPeriods.Sort();
            for (int i = 0; i < 10; i++)
            {
                if (statementPeriods.Count > i)
                {
                    if (i < 4) { Last4Statements.Add(statementPeriods[statementPeriods.Count-(i+1)]); }
                    Last10Statements.Add(statementPeriods[statementPeriods.Count-(i+1)]);
                    if (i == 0) { LastStatement = statementPeriods[statementPeriods.Count-(i+1)];}
                }
            }
            StringBuilder message = new();
            message.Append("Last 10 Statements:\n");
            foreach (var st in Last10Statements)
            {
                message.Append(st.ToString()+"\n");
            }
            message.Append("Last 4 Statements:\n");
            foreach (var st in Last4Statements)
            {
                message.Append(st.ToString() + "\n");
            }
            message.Append($"Last Statement: {LastStatement.ToString()}");
            currentStatementFilter = "All";
        }

        public void ComputeTrackData()
        {
            List<string> noRootList = new();
            foreach (EFUsage usage in EFUsageList)
            {
                bool newUsage = true;
                foreach (TrackInfo track in AllTracks)
                {
                    if (track.rootTrack.id == usage.track.root_id)
                    {
                        Decimal thisAmount = (Decimal)usage.royalty_amount;
                        int usagePeriod = Int32.Parse(usage.statement.period);
                        track.royaltyAmount += thisAmount;
                        if (Last10Statements.Contains(usagePeriod)) { track._last10Royalty += thisAmount; }
                        if (Last4Statements.Contains(usagePeriod)) { track._last4Royalty += thisAmount; }
                        if (usagePeriod == LastStatement) { track._lastRoyalty += thisAmount; }
                        if (usagePeriod < track.firstUsage)
                        {
                            track.firstUsage = usagePeriod;
                            track.usageCount = GetUsageCount(usagePeriod);
                        }
                        track.usages++;
                        newUsage = false;
                        if (!track.track_ids.Contains(usage.track.id))
                        {
                            track.track_ids.Add(usage.track.id);
                        }
                    } 
                }
                if (newUsage)
                {
                    TrackInfo track = new();
                    EFTrack sourceTrack = EFTrackList.FirstOrDefault(s=>s.id == usage.track_id);
                    RootTrack sourceRoot = RootTrackList.FirstOrDefault(s => s.id == sourceTrack.root_id);
                    if (sourceRoot != null)
                    {
                        Decimal thisAmount = (Decimal)usage.royalty_amount;
                        int usagePeriod = Int32.Parse(usage.statement.period);
                        track.rootTrack = sourceRoot;
                        track.usages = 1;
                        track.track_ids = new();
                        track.royaltyAmount = (Decimal)usage.royalty_amount;
                        track._last10Royalty = Last10Statements.Contains(usagePeriod) ? thisAmount : 0;
                        track._last4Royalty = Last4Statements.Contains(usagePeriod) ? thisAmount : 0;
                        track._lastRoyalty = LastStatement == usagePeriod ? thisAmount : 0;
                        track.library = LibrariesList.FirstOrDefault(s => s.id == sourceRoot.library_id);
                        track.album = AlbumsList.FirstOrDefault(s => s.id == sourceRoot.album_id);
                        track.genre = GenresList.FirstOrDefault(s => s.id == sourceRoot.genre_id);
                        track.subgenre = SubgenresList.FirstOrDefault(s => s.id == sourceRoot.subgenre_id);
                        track.firstUsage = usagePeriod;
                        track.usageCount = GetUsageCount(usagePeriod);
                        track.track_ids.Add(sourceTrack.id);
                        AllTracks.Add(track);
                    }
                    else { if (!noRootList.Contains(usage.track.title_name)) { noRootList.Add(usage.track.title_name); } }
                }
            }
            foreach (TrackInfo track in AllTracks)
            {
                track._perUsage = GetAveragePerUsage(track.usageCount, track.royaltyAmount);
            }
            StringBuilder message = new();
            foreach (string title in noRootList)
            {
                message.Append(title + "\n");
            }
        }

        public int GetUsageCount(int firstPeriod)
        {
            int period = firstPeriod;
            int count = 1;
            while (period != LastStatement)
            {
                int lastNum = period % 10;
                if (lastNum == 4) { period += 7; }
                else { period++; }
                count++;
            }
            return count;
        }

        public Decimal GetAveragePerUsage(int usageCount, Decimal RoyaltyAmount)
        {
            return RoyaltyAmount / (Decimal)usageCount;
        }

        public void RadioButtonHandler(object sender, EventArgs e)
        {
            if (RadioAllButton.IsChecked == true)
            {
                currentStatementFilter = "All";
            }
            else if (RadioLast10Button.IsChecked == true)
            {
                currentStatementFilter = "Last10";
            }
            else if (RadioLast4Button.IsChecked == true)
            {
                currentStatementFilter = "Last4";
            }
            else if (RadioLast1Button.IsChecked == true)
            {
                currentStatementFilter = "Last1";
            }
            RefreshData();
        }

        public void InitializeCollections()
        {
            Libraries = new ObservableCollection<SelectionItem>();
            Albums = new ObservableCollection<SelectionItem>();
            Genres = new ObservableCollection<SelectionItem>();
            Subgenres = new ObservableCollection<SelectionItem>();
            Tracks = new();
        }

        public async void InitializeInterface()
        {
            List<Library> libraryList = await EFData.GetLibrariesList();
            List<Genre> genresList = await EFData.GetGenresList();
            List<Subgenre> subgenresList = await EFData.GetSubgenresList();
            List<Album> albumsList = await EFData.GetAlbumsList();
            Libraries.Add(new SelectionItem { name = "No Library", isSelected = true, id = -1 });
            foreach (Library library in libraryList)
            {
                SelectionItem item = new();
                item.name = library.name;
                item.isSelected = true;
                item.id = library.id;
                Libraries.Add(item);
            }
            LibraryFilterBox.ItemsSource = Libraries;
            Genres.Add(new SelectionItem { name = "No Genre", isSelected = true, id = -1 });
            foreach (Genre genre in genresList)
            {
                SelectionItem item = new();
                item.name = genre.name;
                item.isSelected = true;
                item.id = genre.id;
                Genres.Add(item);
            }
            GenreFilterBox.ItemsSource = Genres;
            Albums.Add(new SelectionItem { name = "No Album", isSelected = true, id = -1 });
            foreach (Album album in albumsList)
            {
                SelectionItem item = new();
                item.name = album.name;
                item.isSelected = true;
                item.id = album.id;
                Albums.Add(item);
            }
            AlbumFilterBox.ItemsSource = Albums;
            Subgenres.Add(new SelectionItem { name = "No Subgenre", isSelected = true, id = -1 });
            foreach (Subgenre subgenre in subgenresList)
            {
                SelectionItem item = new();
                item.name = subgenre.name;
                item.isSelected = true;
                item.id = subgenre.id;
                Subgenres.Add(item);
            }
            SubGenreFilterBox.ItemsSource = Subgenres;
        }

        private void PreventSelection(object sender, SelectionChangedEventArgs e)
        {
            ((ComboBox)sender).SelectedItem = null;
        }

        public async void OnFilterSelectionChanged(object sender, EventArgs e)
        {
            RefreshData();
        }

        public void SelectAll(object sender, EventArgs e)
        {
            string buttonLabel = ((Button)sender).Name.ToString();
            switch (buttonLabel)
            {
                case "LibSelectAllBtn":
                    foreach (SelectionItem item in Libraries)
                    {
                        item.isSelected = true;
                    }
                    break;
                case "GenSelectAllBtn":
                    foreach (SelectionItem item in Genres)
                    {
                        item.isSelected = true;
                    }
                    break;
                case "SubSelectAllBtn":
                    foreach (SelectionItem item in Subgenres)
                    {
                        item.isSelected = true;
                    }
                    break;
                case "AlbSelectAllBtn":
                    foreach (SelectionItem item in Albums)
                    {
                        item.isSelected = true;
                    }
                    break;
                default:
                    MessageBox.Show("Error: Select All Button Malfunction");
                    break;
            }
            RefreshData();
        }

        public void SelectNone(object sender, EventArgs e)
        {
            string buttonLabel = ((Button)sender).Name.ToString();
            switch (buttonLabel)
            {
                case "LibSelectNoneBtn":
                    foreach (SelectionItem item in Libraries)
                    {
                        item.isSelected = false;
                    }
                    break;
                case "GenSelectNoneBtn":
                    foreach (SelectionItem item in Genres)
                    {
                        item.isSelected = false;
                    }
                    break;
                case "SubSelectNoneBtn":
                    foreach (SelectionItem item in Subgenres)
                    {
                        item.isSelected = false;
                    }
                    break;
                case "AlbSelectNoneBtn":
                    foreach (SelectionItem item in Albums)
                    {
                        item.isSelected = false;
                    }
                    break;
                default:
                    MessageBox.Show("Error: Select None Button Malfunction");
                    break;

            }
            RefreshData();
        }

        public async void RefreshData()
        {
            Tracks.Clear();
            foreach (TrackInfo track in AllTracks)
            {
                SelectionItem library = track.library != null ? Libraries.FirstOrDefault(s => s.id == track.library.id) : 
                    Libraries.FirstOrDefault(s => s.id == -1); 
                SelectionItem genre = track.genre != null ? Genres.FirstOrDefault(s => s.id == track.genre.id) :
                    Genres.FirstOrDefault(s => s.id == -1);
                SelectionItem subgenre = track.subgenre != null ? Subgenres.FirstOrDefault(s => s.id == track.subgenre.id) :
                    Subgenres.FirstOrDefault(s => s.id == -1);
                SelectionItem album = track.album != null ? Albums.FirstOrDefault(s => s.id == track.album.id) :
                    Albums.FirstOrDefault(s => s.id == -1);
                if (!(library == null || library.isSelected)) { continue; }
                if (!(genre == null || genre.isSelected)) { continue; }
                if (!(subgenre == null || subgenre.isSelected)) { continue; }
                if (!(album == null || album.isSelected)) { continue; }
                TrackItem newTrack = new();
                newTrack.album = track.album;
                newTrack.genre = track.genre;
                newTrack.library = track.library;
                newTrack.subgenre = track.subgenre;
                newTrack.count = track.usages;
                newTrack.perUsage = track._perUsage;
                newTrack.firstPeriod = track.firstUsage;
                newTrack.periodCount = track.usageCount;
                switch (currentStatementFilter)
                {
                    case "All":
                        newTrack.royaltyamount = track.royaltyAmount;
                        break;
                    case "Last1":
                        newTrack.royaltyamount = track._lastRoyalty;
                        break;
                    case "Last4":
                        newTrack.royaltyamount = track._last4Royalty;
                        break;
                    case "Last10":
                        newTrack.royaltyamount = track._last10Royalty;
                        break;
                }
                newTrack.track_name = track.rootTrack.trackName;
                Tracks.Add(newTrack);
            }
        }

        public class TrackInfo
        {
            public RootTrack rootTrack {  get; set; }
            public List<EFTrack> tracks { get; set; }
            public List<int> track_ids {  get; set; }
            public Album album { get; set; }
            public Genre genre {  get; set; }
            public Subgenre subgenre { get; set; }
            public Library library {  get; set; }
            public int usages {  get; set; }
            public Decimal royaltyAmount {  get; set; }
            public Decimal _lastRoyalty {  get; set; }
            public Decimal _last4Royalty { get; set; }
            public Decimal _last10Royalty { get; set; }
            public int firstUsage { get; set; }
            public int usageCount { get; set; }
            public Decimal _perUsage {  get; set; }
        }

        public class TrackItem : INotifyPropertyChanged
        {
            public string _track_name {  get; set; }
            public Library _library {  get; set; }
            public Album _album {  get; set; }
            public Genre _genre {  get; set; }
            public Subgenre _subgenre {  get; set; }
            public Decimal _royaltyamount {  get; set; }
            public int _usageCount {  get; set; }
            public Decimal _perUsage { get; set; }
            public int _periodCount {  get; set; }
            public int _firstPeriod {  get; set; }
            public string track_name
            {
                get => _track_name;
                set
                {
                    _track_name = value;
                    OnPropertyChanged(nameof(track_name));
                }
            }
            public Library library
            {
                get => _library;
                set
                {
                    _library = value;
                    OnPropertyChanged(nameof(library));
                }
            }
            public Album album
            {
                get => _album;
                set
                {
                    _album = value;
                    OnPropertyChanged(nameof(album));
                }
            }
            public Genre genre
            {
                get => _genre;
                set
                {
                    _genre = value;
                    OnPropertyChanged(nameof(genre));
                }
            }
            public Subgenre subgenre
            {
                get => _subgenre;
                set
                {
                    _subgenre = value;
                    OnPropertyChanged(nameof(subgenre));
                }
            }
            public Decimal royaltyamount
            {
                get => _royaltyamount;
                set
                {
                    _royaltyamount = value;
                    OnPropertyChanged(nameof(royaltyamount));
                }
            }
            public Decimal perUsage
            {
                get => _perUsage;
                set
                {
                    _perUsage = value;
                    OnPropertyChanged(nameof(perUsage));
                }
            }
            public int count
            {
                get => _usageCount;
                set
                {
                    _usageCount = value;
                    OnPropertyChanged(nameof(count));
                }
            }
            public int firstPeriod
            {
                get => _firstPeriod;
                set
                {
                    _firstPeriod = value;
                    OnPropertyChanged(nameof(firstPeriod));
                }
            }
            public int periodCount
            {
                get => _periodCount;
                set
                {
                    _periodCount = value;
                    OnPropertyChanged(nameof(periodCount));
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public class SelectionItem : INotifyPropertyChanged
        {
            public string _name {  get; set; }
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
