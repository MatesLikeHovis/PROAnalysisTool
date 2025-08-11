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

namespace WPFBMI
{
    /// <summary>
    /// Interaction logic for SelectAlbumDialog.xaml
    /// </summary>
    public partial class SelectAlbumDialog : Window
    {
        public ObservableCollection<DisplayItem> Albums { get; set; }
        private RootTrack _selectedTrack;
        public RootTrack UpdatedTrack => _selectedTrack;
        public SelectAlbumDialog(RootTrack selectedTrack)
        {
            InitializeComponent();
            _selectedTrack = selectedTrack;
            this.DataContext = this;
            PopulateAlbumList();
        }
        public async void PopulateAlbumList()
        {
            List<Album> albumList = await EFData.GetAlbumsList();
            List<DisplayItem> displayItems = new();
            foreach (Album album in albumList)
            {
                DisplayItem item = new();
                item.ID = album.id;
                item.Name = album.name;
                item.Album = album;
                displayItems.Add(item);
            }
            displayItems.OrderBy(s => s.Name).ToList();
            Albums = new(displayItems);
        }

        public async void SelectItem(object sender, EventArgs e)
        {
            var button = sender as Button;
            var selectedItem = button?.DataContext as DisplayItem;
            Album selectedAlbum = selectedItem.Album;
            _selectedTrack.album = selectedAlbum;
            UpdatedTrack.album = selectedAlbum;
            UpdatedTrack.album_id = selectedAlbum.id;
            this.DialogResult = true;
        }

        public async void CreateNew(object sender, EventArgs e)
        {
            CreateItemDialog dialog = new CreateItemDialog("New Album");

            // Show the dialog and check if it was accepted (OK button clicked)
            if (dialog.ShowDialog() == true)
            {
                // Get the returned value from the dialog
                string newItem = dialog.ItemType;
                Album newAlbum = await EFData.AddAlbumRequest(newItem);
                if (newAlbum != null)
                {
                    _selectedTrack.album = newAlbum;
                    this.DialogResult = true;
                }
            }
        }

        public void CancelAndExit(object sender, EventArgs e)
        {
            Close();
        }

        public class DisplayItem
        {
            public string Name { get; set; }
            public int ID { get; set; }
            public Album Album { get; set; }

        }
    }
}
