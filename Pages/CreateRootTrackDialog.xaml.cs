using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
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
    /// Interaction logic for CreateRootTrackDialog.xaml
    /// </summary>
    public partial class CreateRootTrackDialog : Window
    {

        public RootTrack newTrack;
        EFTrack assignmentTrack;
        public CreateRootTrackDialog(EFTrack assigningTrack)
        {
            assignmentTrack = assigningTrack;
            newTrack = new();
            InitializeComponent();
            CreateDefaultName();
            this.Title = "Create Root Track For " + assigningTrack.title_name;
            SubtitleLabel.Content = assigningTrack.title_name;
        }

        public void CreateDefaultName()
        {
            string usage_name = assignmentTrack.title_name.ToLower();
            if (usage_name != null)
            {
                TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                string default_name = textInfo.ToTitleCase(usage_name);
                newTrack.trackName = default_name;
                TitleBox.Text=default_name;
            }
        }

        public void UpdateUI()
        {
            GenreBox.Content = newTrack.genre != null ? newTrack.genre.name : "Empty";
            SubGenreBox.Content = newTrack.subgenre != null ? newTrack.subgenre.name : "Empty";
            LibraryBox.Content = newTrack.library != null ? newTrack.library.name : "Empty";
            AlbumBox.Content = newTrack.album != null ? newTrack.album.name : "Empty";
        }

        public async void SelectGenre(object sender, RoutedEventArgs e)
        {
            SelectGenreDialog dialog = new SelectGenreDialog(newTrack);
            if (dialog.ShowDialog() == true)
            {
                RootTrack updatedTrack = dialog.UpdatedTrack;
                dialog.Close();
                UpdateUI();
            }
        }

        public void SelectSubGenre(object sender, RoutedEventArgs e)
        {
            SelectSubGenreDialog dialog = new SelectSubGenreDialog(newTrack);
            if (dialog.ShowDialog() == true)
            {
                RootTrack updatedTrack = dialog.UpdatedTrack;
                dialog.Close();
                UpdateUI();
            }
        }

        public void SelectLibrary(object sender, RoutedEventArgs e)
        {
            SelectLibraryDialog dialog = new SelectLibraryDialog(newTrack);
            if (dialog.ShowDialog() == true)
            {
                RootTrack updatedTrack = dialog.UpdatedTrack;
                dialog.Close();
                UpdateUI();
            }
        }

        public void SelectAlbum(object sender, RoutedEventArgs e)
        {
            SelectAlbumDialog dialog = new SelectAlbumDialog(newTrack);
            if (dialog.ShowDialog() == true)
            {
                RootTrack updatedTrack = dialog.UpdatedTrack;
                dialog.Close();
                UpdateUI();
            }
        }

        public void CreateRootTrack()
        {
            newTrack.trackName = TitleBox.Text;
            newTrack.genre_id = newTrack.genre != null ? newTrack.genre.id : 0;
            newTrack.subgenre_id = newTrack.subgenre != null ? newTrack.subgenre.id : 0;
            newTrack.library_id = newTrack.library != null ? newTrack.library.id : 0;
            newTrack.album_id = newTrack.album != null ? newTrack.album.id : 0;
        }

        public async void SaveAndAssign(object sender, EventArgs e)
        {
            CreateRootTrack();
            if (newTrack.trackName == null || newTrack.trackName == "" || newTrack.trackName == "Enter Title")
            {
                MessageBox.Show("You must assign at least a track name to create a root track.", "Warning");
                return;
            }
            await EFData.CreateAndAssignNewRootTrack(assignmentTrack, newTrack);
            this.DialogResult = true;
        }

        public void CloseWindow()
        {
            this.Close();
        }
    }
}
