using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for AssignRootTrackDialog.xaml
    /// </summary>
    public partial class AssignRootTrackDialog : Window
    {

        public ObservableCollection<DisplayTrack> Tracks { get; set; }
        private ObservableCollection<DisplayTrack> _originalTracks;
        public RootTrack selectedRoot;
        public EFTrack selectedSubTrack;

        public AssignRootTrackDialog(RootTrack track, EFTrack subtrack)
        {
            selectedRoot = track;
            selectedSubTrack = subtrack;
            InitializeComponent();
            GetRootTracks();
            this.DataContext = this;
            this.Title = subtrack.title_name + " Root Track Assignment";
            SelectedRootTrackLabel.Content = track == null ? "Empty" : track.trackName;
            SelectedSubTrackLabel.Content = subtrack == null ? "Empty" : subtrack.title_name;
        }

        public async void AssignSelected(object sender, EventArgs e)
        {
            if (TrackGrid.SelectedItems.Count != 0)
            {
                if (TrackGrid.SelectedItem != null)
                {
                    var selection = TrackGrid.SelectedItem as DisplayTrack;
                    if (selection != null)
                    {
                        selectedRoot = await EFData.GetRootTrackFromID(selection.id);
                        selectedSubTrack.root_track = selectedRoot;
                        selectedSubTrack.root_id = selectedRoot.id;
                        await EFData.UpdateTrack(selectedSubTrack);
                        
                        this.DialogResult = true;
                    }
                }
            }
        }

        public async void ClearItem(object sender, EventArgs e)
        {
            selectedSubTrack.root_track = null;
            selectedSubTrack.root_id = null;
            await EFData.UpdateTrack(selectedSubTrack);
            this.DialogResult = true;
        }

        public async void CreateNew(object sender, EventArgs e)
        {
            var dialog = new CreateRootTrackDialog(selectedSubTrack);
            var result = dialog.ShowDialog();
            if (result == true)
            {
                RootTrack newTrack = dialog.newTrack;
                selectedSubTrack.root_track = newTrack;
                selectedSubTrack.root_id = newTrack.id;
                await EFData.UpdateTrack(selectedSubTrack);
                this.DialogResult = true;
            }
        }

        public async void GetRootTracks()
        {
            List<RootTrack> rootTracks = await EFData.GetRootTrackList();
            List<DisplayTrack> displayTracksList = RootTracksToDisplayTracks(rootTracks);
            Tracks = new(displayTracksList);
            _originalTracks = new(displayTracksList);
        }

        public List<DisplayTrack> RootTracksToDisplayTracks(List<RootTrack> tracks)
        {
            List<DisplayTrack> displaytracks = new();
            foreach (RootTrack track in tracks)
            {
                DisplayTrack newTrack = new();
                newTrack.id = track.id;
                newTrack.genre = track.genre;
                newTrack.root_name = track.trackName;
                newTrack.library = track.library;
                newTrack.album = track.album;
                displaytracks.Add(newTrack);
            }
            return displaytracks;
        }

        public class DisplayTrack
        {
            public int id { get; set; }
            public string root_name {  get; set; }
            public Genre genre {  get; set; }
            public Album album {  get; set; }
            public Library library { get; set; }
        }
    }
}
