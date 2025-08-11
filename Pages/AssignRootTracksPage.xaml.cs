using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
using WPFBMI.Models;

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for AssignRootTracksPage.xaml
    /// </summary>
    public partial class AssignRootTracksPage : Page
    {

        public ObservableCollection<DisplayTrack> Tracks { get; set; }
        private List<DisplayTrack> allTracks {  get; set; }
        private ObservableCollection<DisplayTrack> _originalTracks;
        public bool onlyUnassigned;


        public AssignRootTracksPage()
        {
            onlyUnassigned = false;
            InitializeComponent();
            PopulateTracks();
            this.DataContext = this;
        }

        public async void PopulateTracks()
        {
            List<EFTrack> trackList = await EFData.GetTrackList();
            List<RootTrack> rootTrackList = await EFData.GetRootTrackList();
            _originalTracks = new(ToDisplayTracks(trackList, rootTrackList));
            Tracks = new(_originalTracks);
            allTracks = new(_originalTracks);
        }

        public async void RefreshContent()
        {
            Tracks.Clear();
            allTracks.Clear();
            _originalTracks.Clear();
            List<EFTrack> trackList = await EFData.GetTrackList();
            List<RootTrack> rootTrackList = await EFData.GetRootTrackList();
            List<DisplayTrack> updatedTracks = new(ToDisplayTracks(trackList, rootTrackList));
            foreach (var track in updatedTracks)
            {
                _originalTracks.Add(track);
                Tracks.Add(track);
                allTracks.Add(track);
            }
            UnassignedButtonHandler(null, null);
        }

        public List<DisplayTrack> ToDisplayTracks(List<EFTrack> EFTracks, List<RootTrack> RootTracks)
        {
            List<DisplayTrack> list = new();
            foreach (EFTrack track in EFTracks)
            {
                RootTrack rootTrack = RootTracks.FirstOrDefault(s => s.id == track.root_id);
                DisplayTrack newTrack = new();
                newTrack.id = track.id;
                newTrack.root_id = track.root_id;
                newTrack.title_name = track.title_name;
                newTrack.root_id = track.root_id;
                if (rootTrack != null)
                {
                    newTrack.root_name = rootTrack.trackName == null ? "" : rootTrack.trackName;
                    newTrack.genre = rootTrack.genre == null ? "" : rootTrack.genre.ToString();
                    newTrack.library = rootTrack.library == null ? "" : rootTrack.library.ToString();
                    newTrack.subgenre = rootTrack.subgenre == null ? "" : rootTrack.subgenre.ToString();
                    newTrack.album = rootTrack.album == null ? "" : rootTrack.album.ToString();
                }
                list.Add(newTrack);
            }
            return list;
        }

        public async void UnassignedButtonHandler(object sender, EventArgs e)
        {
            onlyUnassigned = OnlyUnassignedButton.IsChecked.GetValueOrDefault();
            if (onlyUnassigned) {
                Tracks.Clear();
                foreach (DisplayTrack track in allTracks)
                {
                    if (track.root_id == null || track.root_id == 0)
                    {
                        Tracks.Add(track);
                    }
                }
            } else
            {
                Tracks.Clear();
                foreach (DisplayTrack track in allTracks)
                {
                    Tracks.Add(track);
                }
            }
        }

        public async void GoBack(object sender, EventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
            
        }

        public async void DoAutoAssignment(object sender, EventArgs e)
        {
            int changes = await EFData.AutoCreateRootTracks();
            var result = MessageBox.Show($"{changes} tracks were updated");
            if (result == MessageBoxResult.None ||  result == MessageBoxResult.OK)
            {
                RefreshContent();
            }
        }

        public async void AssignOrReassignTrack(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedTrack = button?.DataContext as DisplayTrack;
            int rootID = -1;
            RootTrack track = null;
            if (selectedTrack.root_id != null) { rootID = selectedTrack.root_id.GetValueOrDefault(); }
            EFTrack subtrack = await EFData.GetTrackFromID(selectedTrack.id);
            if (selectedTrack.root_id != null) { track = await EFData.GetRootTrackFromID(rootID); }
            if (selectedTrack != null)
            {
                // Show a dialog to either assign or reassign the RootTrack
                var dialog = new AssignRootTrackDialog(track, subtrack);
                var result = dialog.ShowDialog();
                if (result == true)
                {
                    EFTrack newTrack = dialog.selectedSubTrack;
                    DisplayTrack changedTrack = Tracks.FirstOrDefault(s=>s.id==newTrack.id);
                    if (changedTrack != null)
                    {
                        changedTrack.root_id = newTrack.root_id != null ? newTrack.root_id : null;
                        changedTrack.root_name = newTrack.root_track != null ? newTrack.root_track.trackName : null;
                    }
                }
            }
        }

        public class DisplayTrack : INotifyPropertyChanged
        {
            public int? _root_id;
            public string? _root_name;
            public string? _genre;
            public string? _library;
            public string? _subgenre;
            public string? _album;
            public int id { get; set; }
            public string title_name { get; set; }
            public string root_name { get => _root_name; set { _root_name = value; OnPropertyChanged(nameof(root_name)); } }
            public int? root_id { get=>_root_id; set { _root_id = value; OnPropertyChanged(nameof(root_id)); } }
            public string genre {  get; set; }
            public string publisher { get; set; }
            public string library { get; set; }
            public string subgenre { get; set; }
            public string album { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;

            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
