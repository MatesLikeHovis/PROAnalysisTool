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
using WPFBMI.Models;

namespace WPFBMI
{

    public partial class EditMetadataPage : Page, INotifyPropertyChanged
    {
        public ObservableCollection<DisplayTrack> Tracks { get; set; }
        public List<DisplayTrack> AllTracks { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditMetadataPage()
        {
            this.DataContext = this;
            GetTrackCollection();
            InitializeComponent();
        }

        public async void GetTrackCollection()
        {
            List<RootTrack> trackData = await EFData.GetRootTrackList();
            AllTracks = ConvertToDisplay(trackData);
            Tracks = new(AllTracks);
        }

        public void DisplayComplete(object sender, RoutedEventArgs e)
        {
            Tracks.Clear();
            foreach (var track in AllTracks)
            {
                Tracks.Add(track);
            }
        } 

        public void DisplayIncomplete(object sender, RoutedEventArgs e) 
        {
            Tracks.Clear();
            foreach (var track in AllTracks)
            {
                if (track.library == "" || track.genre == "" || track.library == "" || track.album == "")
                {
                    Tracks.Add(track);
                }
            }
        }
        public List<DisplayTrack> ConvertToDisplay(List<RootTrack> tracks)
        {
            List<DisplayTrack> newTracks = new();
            foreach (RootTrack track in tracks)
            {
                DisplayTrack newTrack = new DisplayTrack();
                newTrack.library = track.library != null ? track.library.ToString() : "";
                newTrack.genre = track.genre != null ? track.genre.ToString() : "";
                newTrack.subgenre = track.subgenre != null ? track.subgenre.ToString() : "";
                newTrack.album = track.album != null ? track.album.ToString() : "";
                newTrack.title_name = track.trackName;
                newTrack.id = track.id;
                newTrack.track = track;
                newTracks.Add(newTrack);
            }
            return newTracks;
        }

        public void ChangeGenre(object sender, EventArgs e)
        {
            Button button = sender as Button;
            DisplayTrack displayTrack = button?.DataContext as DisplayTrack;
            RootTrack track = displayTrack.track;
            SelectGenreDialog dialog = new(track);
            var result = dialog.ShowDialog();
            RootTrack updateTrack = dialog.UpdatedTrack;
            track.genre = updateTrack.genre;
            track.genre_id = updateTrack.genre_id;
            EFData.UpdateRootTrack(track);
            if (result == true)
            {
                foreach (DisplayTrack qTrack in Tracks)
                {
                    if (qTrack.id == track.id) { qTrack.Genre = dialog.UpdatedTrack.genre.name; }
                }
            }
        }
        public void ChangeSubGenre(object sender, EventArgs e)
        {
            Button button = sender as Button;
            DisplayTrack displayTrack = button?.DataContext as DisplayTrack;
            RootTrack track = displayTrack.track;
            SelectSubGenreDialog dialog = new(track);
            var result = dialog.ShowDialog();
            RootTrack updateTrack = dialog.UpdatedTrack;
            track.subgenre = updateTrack.subgenre;
            track.subgenre_id = updateTrack.subgenre_id;
            EFData.UpdateRootTrack(track);
            if (result == true)
            {
                foreach (DisplayTrack qTrack in Tracks)
                {
                    if (qTrack.id == track.id) { qTrack.Subgenre = dialog.UpdatedTrack.subgenre.name; }
                }
            }
        }
        public void ChangeLibrary(object sender, EventArgs e)
        {
            Button button = sender as Button;
            DisplayTrack displayTrack = button?.DataContext as DisplayTrack;
            RootTrack track = displayTrack.track;
            SelectLibraryDialog dialog = new(track);
            var result = dialog.ShowDialog();
            RootTrack updateTrack = dialog.UpdatedTrack;
            track.library = updateTrack.library;
            track.library_id = updateTrack.library_id;
            EFData.UpdateRootTrack(track);
            if (result == true)
            {
                foreach (DisplayTrack qTrack in Tracks)
                {
                    if (qTrack.id == track.id) { qTrack.Library = dialog.UpdatedTrack.library.name; }
                }
            }
        }

        public void ChangeAlbum(object sender, EventArgs e)
        {
            Button button = sender as Button;
            DisplayTrack displayTrack = button?.DataContext as DisplayTrack;
            RootTrack track = displayTrack.track;
            SelectAlbumDialog dialog = new(track);
            var result = dialog.ShowDialog();
            RootTrack updateTrack = dialog.UpdatedTrack;
            track.album = updateTrack.album;
            track.album_id = updateTrack.album_id;
            EFData.UpdateRootTrack(track);
            if (result == true)
            {
                foreach (DisplayTrack qTrack in Tracks)
                {
                    if (qTrack.id == track.id) { qTrack.Album = dialog.UpdatedTrack.library.name; }
                }
            }
        }
        public void CancelAndExit(object sender, EventArgs e) {
            ExitPage();
        }

        public void ExitPage()
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            } else
            {
                this.NavigationService.Content = null;
            }
        }

        public class DisplayTrack : INotifyPropertyChanged
        {
            public int id { get; set; }
            public RootTrack track { get; set; }
            public string? title_name { get; set; }
            public string? title_num { get; set; }
            public string? genre { get; set; }
            public string? subgenre { get; set; }
            public string? library { get; set; }
            public string? album {  get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;

            public string Genre
            {
                get
                {
                    return genre;
                } set
                {
                    genre = value;
                    OnPropertyChanged(nameof(Genre));
                }
            }
            public string Subgenre
            {
                get
                {
                    return subgenre;
                }
                set
                {
                    subgenre = value;
                    OnPropertyChanged(nameof(Subgenre));
                }
            }

            public string Library
            {
                get
                {
                    return library;
                }
                set
                {
                    library = value;
                    OnPropertyChanged(nameof(Library));
                }
            }
            public string Album
            {
                get
                {
                    return album;
                } set
                {
                    album = value;
                    OnPropertyChanged(nameof(Album));
                }
            }


            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
